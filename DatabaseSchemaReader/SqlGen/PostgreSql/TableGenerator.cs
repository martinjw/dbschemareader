using System;
using System.Linq;
using System.Text;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.PostgreSql
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
        private ConstraintWriter CreateConstraintWriter()
        {
            return new ConstraintWriter(Table) { IncludeSchema = IncludeSchema, TranslateCheckConstraint = TranslateCheckExpression };
        }
        private static string TranslateCheckExpression(string expression)
        {
            //translate SqlServer-isms into PostgreSql
            var getDate = expression.IndexOf("getdate()", StringComparison.OrdinalIgnoreCase);
            if (getDate != -1)
            {
                expression = expression.Remove(getDate, 9).Insert(getDate, "current_timestamp");
            }
            return expression
                //column escaping
                .Replace("[", "\"")
                .Replace("]", "\"")
                //MySql column escaping
                .Replace("`", "\"")
                .Replace("`", "\"");
        }
        //protected virtual IMigrationGenerator CreateMigrationGenerator()
        //{
        //    return new SqlServerMigrationGenerator();
        //}
        private void AddIndexes(StringBuilder sb)
        {
            if (!Table.Indexes.Any()) return;

            //var migration = CreateMigrationGenerator();
            //foreach (var index in Table.Indexes)
            //{
            //    if(index.IsUnqiueKeyIndex(Table)) continue;

            //    sb.AppendLine(migration.AddIndex(Table, index));
            //}
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
                const string defaultConstraint = "DEFAULT ";
                var dataType = column.DbDataType.ToUpperInvariant();
                if (dataType == "VARCHAR" || dataType == "TEXT" || dataType == "CHAR")
                {
                    defaultValue = defaultConstraint + "'" + column.DefaultValue + "'";
                }
                else if (column.DataType != null && column.DataType.GetNetType() == typeof(bool))
                {
                    var d = column.DefaultValue.Trim(new[] { '(', ')' });
                    defaultValue = defaultConstraint + (d == "1" ? "TRUE" : "FALSE");
                }
                else //numeric default
                {
                    //remove any parenthesis
                    var d = column.DefaultValue.Trim(new[] { '(', ')' });
                    //special case casting. What about other single integers?
                    if ("money".Equals(column.DbDataType, StringComparison.OrdinalIgnoreCase) && d == "0")
                        d = "((0::text)::money)"; //cast from int to money. Weird.
                    defaultValue = defaultConstraint + d;
                }
                defaultValue = " " + defaultValue;
            }

            var sql = DataTypeWriter.DataType(column);
            if (sql == "BIT") _hasBit = true;


            if (column.IsIdentity) sql = " SERIAL";
            if (column.IsPrimaryKey)
                sql += " NOT NULL";
            else
                sql += " " + (!column.Nullable ? " NOT NULL" : string.Empty) + defaultValue;
            return sql;
        }

        protected override string NonNativeAutoIncrementWriter()
        {
            return string.Empty;
        }
    }
}
