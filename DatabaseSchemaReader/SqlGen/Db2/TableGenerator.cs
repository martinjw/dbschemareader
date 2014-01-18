﻿using System.Linq;
using System.Text;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.Db2
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
            var type = new DataTypeWriter().WriteDataType(column);
            type += (!column.Nullable ? " NOT NULL" : string.Empty);

            var defaultValue = column.DefaultValue;
            if (!string.IsNullOrEmpty(defaultValue))
            {
                defaultValue = FixDefaultValue(defaultValue);
                const string defaultConstraint = " DEFAULT ";

                var dbDataType = column.DbDataType.ToUpperInvariant();
                if (DataTypeConverter.IsVariableString(dbDataType))
                {
                    type += defaultConstraint + "'" + defaultValue + "'";
                }
                else //numeric default
                {
                    type += defaultConstraint + defaultValue;
                }
            }

            //cannot detect the "GENERATED BY DEFAULT" vs "GENERATED ALWAYS" version?
            if (column.IsAutoNumber) type += " GENERATED BY DEFAULT AS IDENTITY";
            if (column.IsPrimaryKey && Table.PrimaryKey.Columns.Count == 1)
                type += " PRIMARY KEY";

            return type;
        }

        private static string FixDefaultValue(string defaultValue)
        {
            //Guid defaults. 
            if (SqlTranslator.IsGuidGenerator(defaultValue))
            {
                return null; //there is no native guid (uuid) generator in DB2
            }
            return SqlTranslator.Fix(defaultValue);
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
            return new Db2MigrationGenerator { IncludeSchema = IncludeSchema };
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


        private static string TranslateCheckExpression(string expression)
        {
            expression = SqlTranslator.Fix(expression);

            return expression
                //column escaping
                .Replace("[", "\"")
                .Replace("]", "\"")
                //MySql column escaping
                .Replace("`", "\"")
                .Replace("`", "\"");
        }
        #endregion
    }
}
