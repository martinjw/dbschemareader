using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.SqLite
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
            var type = DataTypeWriter.SqLiteDataType(column);
            if (column.IsPrimaryKey && Table.PrimaryKey.Columns.Count == 1)
            {
                type += " PRIMARY KEY";
                if (column.IsIdentity) type += " AUTOINCREMENT";
            }
            if (!column.Nullable) type += " NOT NULL";
            if (!string.IsNullOrEmpty(column.DefaultValue))
            {
                var value = RemoveParenthesis(column.DefaultValue);
                if (value.IndexOf("GETDATE()", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    value = "CURRENT_TIMESTAMP";
                }
                type += " DEFAULT " + value;
            }

            return type;
        }

        private static string RemoveParenthesis(string value)
        {
            if (value.StartsWith("((", StringComparison.OrdinalIgnoreCase) &&
                value.EndsWith("))", StringComparison.OrdinalIgnoreCase))
            {
                value = value.Substring(2, value.Length - 4);
            }
            return value;
        }

        protected override void AddTableConstraints(IList<string> columnList)
        {
            var formatter = SqlFormatProvider();
            if (Table.PrimaryKey != null && Table.PrimaryKey.Columns.Count > 1)
            {
                columnList.Add("PRIMARY KEY (" + GetColumnList(Table.PrimaryKey.Columns) + ")");
            }
            foreach (var uniqueKey in Table.UniqueKeys)
            {
                columnList.Add("UNIQUE KEY (" + GetColumnList(uniqueKey.Columns) + ")");
            }
            foreach (var checkConstraint in Table.CheckConstraints)
            {
                var expression = CleanCheckExpression(checkConstraint.Expression);
                columnList.Add("CHECK " + expression);
            }

            //http://www.sqlite.org/foreignkeys.html These aren't enabled by default.
            foreach (var foreignKey in Table.ForeignKeys)
            {
                var referencedTable = foreignKey.ReferencedTable(Table.DatabaseSchema);
                //can't find the table. Don't write the fk reference.
                if (referencedTable == null) continue;

                var refColumnList = GetColumnList(referencedTable.PrimaryKey.Columns);

                columnList.Add(string.Format(CultureInfo.InvariantCulture,
                    "FOREIGN KEY ({0}) REFERENCES {1} ({2})",
                    GetColumnList(foreignKey.Columns),
                    formatter.Escape(foreignKey.RefersToTable),
                    refColumnList));
            }
        }

        private static string CleanCheckExpression(string expression)
        {
            if (expression.IndexOf("GETDATE()", StringComparison.OrdinalIgnoreCase) != -1)
            {
                //special case
                expression = Regex.Replace(expression, @"GETDATE\(\)", "CURRENT_TIMESTAMP", RegexOptions.IgnoreCase);
            }
            //remove any braces around numbers
            expression = Regex.Replace(expression, @"\((\d+)\)", "$1");
            return expression;
        }

        protected override string NonNativeAutoIncrementWriter()
        {
            return string.Empty;
        }

        protected override string ConstraintWriter()
        {
            return string.Empty;
        }

        #endregion

        private string GetColumnList(IEnumerable<string> columns)
        {
            var escapedColumnNames = columns.Select(column => SqlFormatProvider().Escape(column)).ToArray();
            return string.Join(", ", escapedColumnNames);
        }
    }
}
