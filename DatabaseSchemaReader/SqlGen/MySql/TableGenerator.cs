using System;
using System.Text;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.MySql
{
    class TableGenerator : TableGeneratorBase
    {
        //ENGINE=InnoDB DEFAULT CHARSET=utf8 at the end of the table ddl is implicit

        public TableGenerator(DatabaseTable table)
            : base(table)
        {
        }

        #region Overrides of TableGeneratorBase

        protected override ISqlFormatProvider SqlFormatProvider()
        {
            return new SqlFormatProvider();
        }

        protected override string WriteDataType(DatabaseColumn column)
        {
            var type = column.MySqlDataType();
            type += (!column.Nullable ? " NOT NULL" : string.Empty);

            if (!string.IsNullOrEmpty(column.DefaultValue))
            {
                //No names for constraints
                const string defaultConstraint = " DEFAULT ";
                var dataType = column.DbDataType.ToUpperInvariant();
                if (dataType == "NVARCHAR" || dataType == "VARCHAR" || dataType == "CHAR")
                {
                    type += defaultConstraint + "'" + column.DefaultValue + "'";
                }
                else //numeric default
                {
                    type += defaultConstraint + column.DefaultValue;
                }
            }

            //MySql auto-increments MUST BE primary key
            if (column.IsIdentity) type += " AUTO_INCREMENT PRIMARY KEY";
            else if (column.IsPrimaryKey && Table.PrimaryKey.Columns.Count == 1)
                type += " PRIMARY KEY";

            return type;
        }

        protected override string NonNativeAutoIncrementWriter()
        {
            return string.Empty;
        }

        protected override string ConstraintWriter()
        {
            var sb = new StringBuilder();
            var constraintWriter = new ConstraintWriter(Table);
            constraintWriter.IncludeSchema = IncludeSchema;

            constraintWriter.CheckConstraintExcluder = ExcludeCheckConstraint;
            constraintWriter.TranslateCheckConstraint = TranslateCheckExpression;

            //single primary keys done inline
            if (Table.PrimaryKey != null && Table.PrimaryKey.Columns.Count > 1)
            {
                sb.AppendLine(constraintWriter.WritePrimaryKey());
            }

            sb.AppendLine(constraintWriter.WriteUniqueKeys());
            sb.AppendLine(constraintWriter.WriteCheckConstraints());
            return sb.ToString();
        }

        private static bool ExcludeCheckConstraint(DatabaseConstraint check)
        {
            //don't allow SYSDATE in check constraints
            if (check.Expression.IndexOf("getDate()", StringComparison.OrdinalIgnoreCase) != -1)
                return true;
            return false;
        }

        private static string TranslateCheckExpression(string expression)
        {
            //translate SqlServer-isms into MySql
            return expression
                //column escaping
                .Replace("[", "`")
                .Replace("]", "`");
        }
        #endregion
    }
}
