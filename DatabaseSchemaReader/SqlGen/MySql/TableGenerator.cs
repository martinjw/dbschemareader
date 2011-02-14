using System;
using System.Text;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.MySql
{
    class TableGenerator : TableGeneratorBase
    {
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
            if (column.IsIdentity) type += " AUTO_INCREMENT";
            if (column.IsPrimaryKey && Table.PrimaryKey.Columns.Count == 1)
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
