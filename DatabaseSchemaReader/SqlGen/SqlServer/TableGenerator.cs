using System;
using System.Linq;
using System.Text;
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
            DataTypeWriter = new DataTypeWriter();
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
            constraintWriter.CheckConstraintExcluder = check => (_hasBit && check.Expression.Contains(" IN (0, 1)"));
            sb.AppendLine(constraintWriter.WriteCheckConstraints());

            AddIndexes(sb);

            return sb.ToString();
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

                sb.AppendLine(migration.AddIndex(Table, index));
            }
        }

        protected override ISqlFormatProvider SqlFormatProvider()
        {
            return new SqlFormatProvider();
        }

        protected override string WriteDataType(DatabaseColumn column)
        {

            var defaultValue = string.Empty;
            if (!string.IsNullOrEmpty(column.DefaultValue))
            {
                var value = FixDefaultValue(column.DefaultValue);
                const string defaultConstraint = "DEFAULT ";
                if (IsStringColumn(column))
                {
                    defaultValue = defaultConstraint + "'" + value + "'";
                }
                else //numeric default
                {
                    defaultValue = defaultConstraint + value;
                }
            }

            var sql = DataTypeWriter.WriteDataType(column);
            if (sql == "BIT") _hasBit = true;

            if (DataTypeWriter.LooksLikeOracleIdentityColumn(Table, column))
            {
                column.IsIdentity = true;
            }
            if (column.IsIdentity) sql += " IDENTITY(1,1)";
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

        private static bool IsStringColumn(DatabaseColumn column)
        {
            var dataType = column.DbDataType.ToUpperInvariant();
            var isString = (dataType == "NVARCHAR2" || dataType == "VARCHAR2" || dataType == "CHAR");
            var dt = column.DataType;
            if (dt != null && dt.IsString) isString = true;
            return isString;
        }

        protected override string NonNativeAutoIncrementWriter()
        {
            return string.Empty;
        }
    }
}
