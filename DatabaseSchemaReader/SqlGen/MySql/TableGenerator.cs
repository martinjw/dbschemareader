using System;
using System.Linq;
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
            var type = new DataTypeWriter().WriteDataType(column);
            type += (!column.Nullable ? " NOT NULL" : string.Empty);

            var defaultValue = column.DefaultValue;
            if (!string.IsNullOrEmpty(defaultValue))
            {
                defaultValue = FixDefaultValue(defaultValue);
                const string defaultConstraint = " DEFAULT ";

                if (IsStringColumn(column))
                {
                    type += defaultConstraint + "'" + defaultValue + "'";
                }
                else //numeric default
                {
                    type += defaultConstraint + defaultValue;
                }
            }

            //MySql auto-increments MUST BE primary key
            if (column.IsAutoNumber) type += " AUTO_INCREMENT PRIMARY KEY";
            else if (column.IsPrimaryKey && Table.PrimaryKey.Columns.Count == 1)
                type += " PRIMARY KEY";

            return type;
        }

        private static string FixDefaultValue(string defaultValue)
        {
            //Guid defaults. 
            if (SqlTranslator.IsGuidGenerator(defaultValue))
            {
                return "UUID()";
            }
            return SqlTranslator.Fix(defaultValue);
        }

        private static bool IsStringColumn(DatabaseColumn column)
        {
            var dataType = column.DbDataType.ToUpperInvariant();
            var isString = (dataType == "NVARCHAR" || dataType == "VARCHAR" || dataType == "CHAR");
            var dt = column.DataType;
            if (dt != null && dt.IsString) isString = true;
            return isString;
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

            AddIndexes(sb);

            return sb.ToString();
        }
        protected virtual IMigrationGenerator CreateMigrationGenerator()
        {
            return new MySqlMigrationGenerator { IncludeSchema = IncludeSchema };
        }
        private void AddIndexes(StringBuilder sb)
        {
            if (!Table.Indexes.Any()) return;

            var migration = CreateMigrationGenerator();
            foreach (var index in Table.Indexes)
            {
                if (index.IsUniqueKeyIndex(Table)) continue;

                if (index.Columns.Count == 0)
                {
                    //IndexColumns errors 
                    sb.AppendLine("-- add index " + index.Name + " (unknown columns)");
                    continue;
                }

                sb.AppendLine(migration.AddIndex(Table, index));
            }
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
            expression = SqlTranslator.Fix(expression);
            //translate SqlServer-isms into MySql
            return expression
                //column escaping
                .Replace("[", "`")
                .Replace("]", "`");
        }
        #endregion
    }
}
