using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.SqlServer
{
    class TableGenerator : TableGeneratorBase
    {
        private bool _hasBit;
        protected DataTypeWriter DataTypeWriter;

        public TableGenerator(DatabaseTable table)
            : base(table)
        {
            SqlType? originSqlType = null;
            if (table.DatabaseSchema != null)
                originSqlType = ProviderToSqlType.Convert(table.DatabaseSchema.Provider);

            DataTypeWriter = new DataTypeWriter(originSqlType);
        }

        public override string Write()
        {
            string desc = null;
            if (!string.IsNullOrEmpty(Table.Description))
            {
                desc = AddTableDescription();
            }
            if (Table.Columns.Any(c => !string.IsNullOrEmpty(c.Description)))
            {
                desc = desc + AddColumnDescriptions();
            }
            return base.Write() + desc;
        }

        protected virtual string AddColumnDescriptions()
        {
            var sb = new StringBuilder();
            sb.AppendLine(SqlFormatProvider().RunStatements());
            foreach (var column in Table.Columns.Where(c => !string.IsNullOrEmpty(c.Description)))
            {
                var description = column.Description.Replace("'", "''");
                sb.AppendLine("EXEC sys.sp_addextendedproperty ");
                sb.AppendLine("@name = N'MS_Description', ");
                sb.AppendLine("@value = N'" + description + "',");
                sb.AppendLine("@level0type = N'Schema', @level0name = '" + (Table.SchemaOwner ?? "dbo") + "',");
                sb.AppendLine("@level1type = N'Table', @level1name = '" + Table.Name + "',");
                sb.AppendLine("@level2type = N'Column', @level2name = '" + column.Name + "'");
                sb.AppendLine(SqlFormatProvider().RunStatements());
            }
            return sb.ToString();
        }

        protected virtual string AddTableDescription()
        {
            var description = Table.Description.Replace("'", "''");
            var sb = new StringBuilder();
            sb.AppendLine(SqlFormatProvider().RunStatements());
            sb.AppendLine("EXEC sys.sp_addextendedproperty ");
            sb.AppendLine("@name = N'MS_Description', ");
            sb.AppendLine("@value = N'" + description + "',");
            sb.AppendLine("@level0type = N'Schema', @level0name = '" + (Table.SchemaOwner ?? "dbo") + "',");
            sb.AppendLine("@level1type = N'Table', @level1name = '" + Table.Name + "'");
            sb.AppendLine(SqlFormatProvider().RunStatements());
            return sb.ToString();
        }

        protected override string ConstraintWriter()
        {
            var sb = new StringBuilder();
            var constraintWriter = CreateConstraintWriter();

            if (Table.PrimaryKey != null)
            {
                sb.AppendLine(constraintWriter.WritePrimaryKey());
            }

            sb.AppendLine(constraintWriter.WriteUniqueKeys());
            //looks like a boolean check, skip it
            constraintWriter.TranslateCheckConstraint = TranslateCheckExpression;
            sb.AppendLine(constraintWriter.WriteCheckConstraints());

            AddIndexes(sb);

            return sb.ToString();
        }
        private static string TranslateCheckExpression(string expression)
        {
            const string pattern = @"\bDate\(\)";
            if (Regex.IsMatch(expression, pattern))
            {
                expression = Regex.Replace(expression, pattern, "getdate()");
            }
            return expression;
        }
        protected virtual ConstraintWriterBase CreateConstraintWriter()
        {
            return new ConstraintWriter(Table) { IncludeSchema = IncludeSchema, EscapeNames = EscapeNames};
        }
        protected virtual IMigrationGenerator CreateMigrationGenerator()
        {
            var migrations = new SqlServerMigrationGenerator();
            //ensure we're not writing schema prefixes
            if (!IncludeSchema) migrations.IncludeSchema = false;
            migrations.EscapeNames = EscapeNames;
            return migrations;
        }

        private void AddIndexes(StringBuilder sb)
        {
            if (!Table.Indexes.Any()) return;

            var migration = CreateMigrationGenerator();
            foreach (var index in Table.Indexes)
            {
                if (index.IsUniqueKeyIndex(Table)) continue;
                //are all indexed columns in table? 
                // If not, could be a function index which must be done manually
                if (IndexColumnsNotInTable(index)) continue;

                sb.AppendLine(migration.AddIndex(Table, index));
            }
        }

        private bool IndexColumnsNotInTable(DatabaseIndex index)
        {
            foreach (var column in index.Columns)
            {
                if (!Table.Columns.Any(c => c.Name == column.Name))
                    return true;
            }
            return false;
        }

        protected override ISqlFormatProvider SqlFormatProvider()
        {
            return new SqlFormatProvider();
        }

        protected virtual bool HandleComputed(DatabaseColumn column)
        {
            return (column.IsComputed);
        }

        protected override string WriteDataType(DatabaseColumn column)
        {
            if (HandleComputed(column))
            {
                return "AS " + column.ComputedDefinition;
            }

            var sql = DataTypeWriter.WriteDataType(column);
            if (sql == "BIT") _hasBit = true;

            var defaultValue = string.Empty;
            if (!string.IsNullOrEmpty(column.DefaultValue) && IncludeDefaultValues)
            {
                var value = FixDefaultValue(column.DefaultValue);
                if (_hasBit)
                {
                    //Access Yes/No boolean
                    if (value.Equals("No", StringComparison.OrdinalIgnoreCase)) value = "0";
                    if (value.Equals("Yes", StringComparison.OrdinalIgnoreCase)) value = "1";
                }
                if (value.StartsWith("(NEXT VALUE FOR ", StringComparison.OrdinalIgnoreCase) && !SupportsNextValueForSequence)
                {
                    //SQLServer 2012 "NEXT VALUE FOR [Sequence]". Allow it to be turned back to identity.
                    column.IsAutoNumber = true;
                    column.IdentityDefinition = new DatabaseColumnIdentity();
                    value = null;
                }
                //strings should already have the single quotes in place
                if (!string.IsNullOrEmpty(value))
                    defaultValue = "DEFAULT " + value;
            }

            if (DataTypeWriter.LooksLikeOracleIdentityColumn(Table, column))
            {
                column.IsAutoNumber = true;
                column.IdentityDefinition = new DatabaseColumnIdentity();
            }
            if (column.IdentityDefinition != null)
            {
                var id = column.IdentityDefinition;
                sql += " IDENTITY(" + id.IdentitySeed + "," + id.IdentityIncrement + ")";
            }
            if (column.IsPrimaryKey)
            {
                sql += " NOT NULL";
                if (!string.IsNullOrEmpty(defaultValue)) sql += " " + defaultValue;
            }
            else
                sql += " " + (!column.Nullable ? " NOT NULL" : string.Empty) + " " + defaultValue;
            return sql;
        }

        /// <summary>
        /// Gets a value indicating whether supports "next value for [sequence]" (SQLServer 2012+). Not publicly changeable here, yet...
        /// </summary>
        protected virtual bool SupportsNextValueForSequence { get { return true; } }

        protected virtual string FixDefaultValue(string defaultValue)
        {
            if (SqlTranslator.IsGuidGenerator(defaultValue) && !"newsequentialid()".Equals(defaultValue, StringComparison.OrdinalIgnoreCase))
            {
                return "newid()";
            }
            return SqlTranslator.Fix(defaultValue);
        }

        protected override string NonNativeAutoIncrementWriter()
        {
            return string.Empty;
        }
    }
}
