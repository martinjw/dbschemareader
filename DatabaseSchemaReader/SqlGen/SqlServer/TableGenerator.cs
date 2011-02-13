using System;
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

        protected override string LineEnding()
        {
            return @"
GO
";
        }

        protected override string EscapeName(string name)
        {
            return StringEscaper.Escape(name);
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

            if (Table.Triggers.Count == 1 && column.IsPrimaryKey)
            {
                column.IsIdentity = true;
                //if a table has a trigger, we assume it's an Oracle trigger/sequence which is translated to identity for the pk
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
