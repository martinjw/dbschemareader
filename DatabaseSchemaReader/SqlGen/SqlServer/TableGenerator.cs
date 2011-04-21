using System.Text;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.SqlServer
{
    class TableGenerator : TableGeneratorBase
    {
        private bool _hasBit;

        public TableGenerator(DatabaseTable table)
            : base(table)
        {
        }

        protected override string ConstraintWriter()
        {
            var sb = new StringBuilder();
            var constraintWriter = new ConstraintWriter(Table);
            constraintWriter.IncludeSchema = IncludeSchema;

            //single primary keys done inline
            if (Table.PrimaryKey != null && Table.PrimaryKey.Columns.Count > 1)
            {
                sb.AppendLine(constraintWriter.WritePrimaryKey());
            }

            sb.AppendLine(constraintWriter.WriteUniqueKeys());
            //looks like a boolean check, skip it
            constraintWriter.CheckConstraintExcluder = check => (_hasBit && check.Expression.Contains(" IN (0, 1)"));
            sb.AppendLine(constraintWriter.WriteCheckConstraints());
            return sb.ToString();
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
                var defaultConstraint = " CONSTRAINT [DF_" + TableName + "_" + column.Name + "] DEFAULT ";
                var dataType = column.DbDataType.ToUpperInvariant();
                if (dataType == "NVARCHAR2" || dataType == "VARCHAR2" || dataType == "CHAR")
                {
                    defaultValue = defaultConstraint + "'" + column.DefaultValue + "'";
                }
                else //numeric default
                {
                    defaultValue = defaultConstraint + column.DefaultValue;
                }
            }

            var sql = column.SqlServerDataType();
            if (sql == "BIT") _hasBit = true;

            if (DataTypeWriter.LooksLikeOracleIdentityColumn(Table, column))
            {
                column.IsIdentity = true;
            }
            if (column.IsIdentity) sql += " IDENTITY(1,1)";
            if (column.IsPrimaryKey && Table.PrimaryKey.Columns.Count == 1)
                sql += " PRIMARY KEY NOT NULL";
            else
                sql += " " + (!column.Nullable ? " NOT NULL" : string.Empty) + " " + defaultValue;
            return sql;
        }

        protected override string NonNativeAutoIncrementWriter()
        {
            return string.Empty;
        }
    }
}
