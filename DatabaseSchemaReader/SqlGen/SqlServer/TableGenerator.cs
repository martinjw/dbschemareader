using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DatabaseSchemaReader.Conversion;
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
        protected virtual ConstraintWriter CreateConstraintWriter()
        {
            return new ConstraintWriter(Table) { IncludeSchema = IncludeSchema };
        }
        protected virtual IMigrationGenerator CreateMigrationGenerator()
        {
            var migrations = new SqlServerMigrationGenerator();
            //ensure we're not writing schema prefixes
            if (!IncludeSchema) migrations.IncludeSchema = false;
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
            if (!string.IsNullOrEmpty(column.DefaultValue))
            {
                var value = FixDefaultValue(column.DefaultValue);
                if (_hasBit)
                {
                    //Access Yes/No boolean
                    if (value.Equals("No", StringComparison.OrdinalIgnoreCase)) value = "0";
                    if (value.Equals("Yes", StringComparison.OrdinalIgnoreCase)) value = "1";
                }

                const string defaultConstraint = "DEFAULT ";
                //strings should already have the single quotes in place
                defaultValue = defaultConstraint + value;
            }

            if (DataTypeWriter.LooksLikeOracleIdentityColumn(Table, column))
            {
                column.IsIdentity = true;
            }
            if (column.IsIdentity) sql += " IDENTITY(" + column.IdentitySeed + "," + column.IdentityIncrement + ")";
            if (column.IsPrimaryKey)
                sql += " NOT NULL";
            else
                sql += " " + (!column.Nullable ? " NOT NULL" : string.Empty) + " " + defaultValue;
            return sql;
        }

        private static string FixDefaultValue(string defaultValue)
        {
            if (SqlTranslator.IsGuidGenerator(defaultValue) && !"newsequentialid()".Equals(defaultValue, StringComparison.OrdinalIgnoreCase))
            {
                return "newid()";
            }
            return SqlTranslator.Fix(defaultValue);
        }

        //private static bool IsStringColumn(DatabaseColumn column)
        //{
        //    var dataType = column.DbDataType.ToUpperInvariant();
        //    var isString = (dataType == "NVARCHAR2" || dataType == "VARCHAR2" || dataType == "CHAR");
        //    var dt = column.DataType;
        //    if (dt != null && dt.IsString) isString = true;
        //    return isString;
        //}

        protected override string NonNativeAutoIncrementWriter()
        {
            return string.Empty;
        }
    }
}
