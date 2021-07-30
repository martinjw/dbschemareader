using System;
using System.Linq;
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

        private string AddColumnDescriptions()
        {
            var sb = new StringBuilder();
            var formatProvider = SqlFormatProvider();
            var tableName = SchemaTableName(Table);
            foreach (var column in Table.Columns.Where(c => !string.IsNullOrEmpty(c.Description)))
            {
                sb.Append("COMMENT ON COLUMN ");
                sb.Append(tableName + "." + (EscapeNames ? formatProvider.Escape(column.Name) : column.Name));
                sb.Append(" IS '");
                sb.Append(column.Description);
                sb.AppendLine("'" + formatProvider.LineEnding());
            }
            return sb.ToString();
        }

        private string AddTableDescription()
        {
            var formatProvider = SqlFormatProvider();
            var sb = new StringBuilder();
            sb.Append("COMMENT ON TABLE ");
            sb.Append(SchemaTableName(Table));
            sb.Append(" IS '");
            sb.Append(Table.Description);
            sb.AppendLine("'" + formatProvider.LineEnding());
            return sb.ToString();
        }

        protected override string ConstraintWriter()
        {
            var sb = new StringBuilder();
            var constraintWriter = new ConstraintWriter(Table)
            {
                IncludeSchema = IncludeSchema,
                CheckConstraintExcluder = ExcludeCheckConstraint,
                TranslateCheckConstraint = TranslateCheckExpression,
                EscapeNames = EscapeNames
            };

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

            //UNLESS it's ORACLE 12c
            return new AutoIncrementWriter(Table).Write();
        }

    }
}
