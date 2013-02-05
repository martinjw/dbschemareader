using System;
using System.Text;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.Oracle
{
    class TableGenerator : TableGeneratorBase
    {

        public TableGenerator(DatabaseTable table)
            : base(table)
        {
        }


        protected override string ConstraintWriter()
        {
            var sb = new StringBuilder();
            var constraintWriter = new ConstraintWriter(Table);
            constraintWriter.IncludeSchema = IncludeSchema;

            constraintWriter.CheckConstraintExcluder = ExcludeCheckConstraint;
            constraintWriter.TranslateCheckConstraint = TranslateCheckExpression;
            sb.AppendLine(constraintWriter.WriteTableConstraints());
            return sb.ToString();
        }


        private static bool ExcludeCheckConstraint(DatabaseConstraint check)
        {
            //Oracle doesn't allow SYSDATE in check constraints
            if (check.Expression.IndexOf("getDate()", StringComparison.OrdinalIgnoreCase) != -1)
                return true;
            if (check.Expression.IndexOf("current_timestamp", StringComparison.OrdinalIgnoreCase) != -1)
                return true;
            return false;
        }

        private static string TranslateCheckExpression(string expression)
        {
            expression = SqlTranslator.Fix(expression);
            //translate SqlServer-isms into Oracle
            return expression
                //column escaping
                .Replace("[", "\"")
                .Replace("]", "\"")
                //MySql column escaping
                .Replace("`", "\"")
                .Replace("`", "\"");
        }

        protected override ISqlFormatProvider SqlFormatProvider()
        {
            return new SqlFormatProvider();
        }

        protected override string WriteDataType(DatabaseColumn column)
        {
            if (column.IsComputed)
            {
                //Generated Always and Virtual are optional keywords
                return "GENERATED ALWAYS AS " + column.ComputedDefinition + " VIRTUAL";
            }
            return new DataTypeWriter().WriteDataType(column);
        }

        protected override string NonNativeAutoIncrementWriter()
        {
            //SQLServer table with IDENTITY- let's create the Oracle equivalent
            return new AutoIncrementWriter(Table).Write();
        }

    }
}
