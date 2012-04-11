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
            expression = SqlTranslator.EnsureCurrentTimestamp(expression);

            return expression
                //column escaping
                .Replace("[", "\"")
                .Replace("]", "\"")
                //MySql column escaping
                .Replace("`", "\"")
                .Replace("`", "\"");
        }
        protected virtual IMigrationGenerator CreateMigrationGenerator()
        {
            return new PostgreSqlMigrationGenerator { IncludeSchema = IncludeSchema };
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
                defaultValue = WriteDefaultValue(column);
            }

            var sql = DataTypeWriter.WriteDataType(column);
            if (sql == "BIT") _hasBit = true;

            if (column.IsIdentity)
            {
                bool isLong = column.DataType != null && column.DataType.GetNetType() == typeof(long);
                // Non trivial identities are hooked to a sequence up by AutoIncrementWriter.
                // Newer postgres versions require specifying UNIQUE explicitly.
                if (column.IsNonTrivialIdentity())
                    sql = (isLong ? " BIGINT" : " INT") + " NOT NULL UNIQUE";
                else
                    sql = isLong ? " BIGSERIAL" : " SERIAL";
            }
            else
            {
                if (column.IsPrimaryKey)
                    sql += " NOT NULL";
                else
                    sql += " " + (!column.Nullable ? " NOT NULL" : string.Empty) + defaultValue;
            }
            return sql;
        }

        private static string WriteDefaultValue(DatabaseColumn column)
        {
            const string defaultConstraint = " DEFAULT ";
            var defaultValue = FixDefaultValue(column.DefaultValue);
            if (IsStringColumn(column))
            {
                defaultValue = defaultConstraint + "'" + defaultValue + "'";
            }
            else if (column.DataType != null && column.DataType.GetNetType() == typeof(bool))
            {
                var d = defaultValue.Trim(new[] { '(', ')' });
                defaultValue = defaultConstraint + (d == "1" ? "TRUE" : "FALSE");
            }
            else //numeric default
            {
                //remove any parenthesis
                var d = defaultValue.Trim(new[] { '(', ')' });
                //special case casting. What about other single integers?
                if ("money".Equals(column.DbDataType, StringComparison.OrdinalIgnoreCase) && d == "0")
                    d = "((0::text)::money)"; //cast from int to money. Weird.
                defaultValue = defaultConstraint + d;
            }
            return defaultValue;
        }

        private static bool IsStringColumn(DatabaseColumn column)
        {
            var dataType = column.DbDataType.ToUpperInvariant();
            var isString = (dataType == "VARCHAR" || dataType == "TEXT" || dataType == "CHAR");
            var dt = column.DataType;
            if (dt != null && dt.IsString) isString = true;
            return isString;
        }

        private static string FixDefaultValue(string defaultValue)
        {
            //Guid defaults. 
            if (SqlTranslator.IsGuidGenerator(defaultValue))
            {
                return "uuid_generate_v1()"; //use uuid-osp contrib
            }
            return SqlTranslator.Fix(defaultValue);
        }

        protected override string NonNativeAutoIncrementWriter()
        {
            return new AutoIncrementWriter(Table).Write();
        }
    }
}
