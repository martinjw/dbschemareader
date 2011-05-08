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
            return false;
        }

        private static string TranslateCheckExpression(string expression)
        {
            //translate SqlServer-isms into Oracle
            return expression
                //column escaping
                .Replace("[", "\"")
                .Replace("]", "\"");
        }

        protected override ISqlFormatProvider SqlFormatProvider()
        {
            return new SqlFormatProvider();
        }

        protected override string WriteDataType(DatabaseColumn column)
        {
            var sql = string.Empty;
            var defaultValue = string.Empty;
            var dataType = column.DbDataType.ToUpperInvariant();
            var precision = column.Precision;
            var scale = column.Scale;
            var length = column.Length;

            if (dataType == "BOOLEAN")
            {
                dataType = "NUMBER";
                precision = 1;
                scale = 0;
            }
            //sql server to oracle translation
            if (dataType == "VARBINARY" || dataType == "IMAGE") dataType = "BLOB";
            if (dataType == "NVARCHAR" && length > 4000) dataType = "CLOB";
            if (dataType == "NVARCHAR") dataType = "NVARCHAR2";
            if (dataType == "VARCHAR") dataType = "VARCHAR2";
            if (dataType == "NTEXT" || dataType == "TEXT") dataType = "CLOB";
            if (dataType == "UNIQUEIDENTIFIER")
            {
                dataType = "RAW";
                length = 16;
            }
            if (dataType == "XML") dataType = "XMLTYPE";
            //Dates in SQL Server range from 1753 A.D. to 9999 A.D., whereas dates in Oracle range from 4712 B.C. to 4712 A.D.
            if (dataType == "DATETIME") dataType = "DATE";
            if (dataType == "NUMERIC") dataType = "NUMBER";
            if (dataType == "INT")
            {
                dataType = "NUMBER";
                precision = 9;
                scale = 0;
            }
            if (dataType == "SMALLINT")
            {
                dataType = "NUMBER";
                precision = 5;
                scale = 0;
            }
            if (dataType == "BIT")
            {
                dataType = "NUMBER";
                precision = 1;
                scale = 0;
            }
            if (dataType == "DECIMAL")
            {
                dataType = "NUMBER";
                precision = 18;
                scale = 0;
            }
            if (dataType == "MONEY")
            {
                dataType = "NUMBER";
                precision = 15;
                scale = 4;
            }

            //write out Oracle datatype definition
            if (dataType == "NVARCHAR2")
            {
                //don't specify "CHAR" for NVARCHAR2
                sql = dataType + " (" + length + ")";
                if (!string.IsNullOrEmpty(column.DefaultValue))
                    defaultValue = AddQuotedDefault(column);
            }
            if (dataType == "VARCHAR2")
            {
                //assume it's CHAR rather than bytes
                sql = dataType + " (" + length + " CHAR)";
                if (!string.IsNullOrEmpty(column.DefaultValue))
                    defaultValue = AddQuotedDefault(column);
            }
            if (dataType == "CHAR" || dataType == "NCHAR")
            {
                sql = dataType + " (" + length + ")";
                if (!string.IsNullOrEmpty(column.DefaultValue))
                    defaultValue = AddQuotedDefault(column);
            }
            if (dataType == "NUMBER")
            {
                var writeScale = ((scale != null) && (scale > 0) ? "," + scale : "");
                sql = "NUMBER (" + precision + writeScale + ")";
                if (!string.IsNullOrEmpty(column.DefaultValue))
                    defaultValue = " DEFAULT " + column.DefaultValue;
            }
            if (dataType == "REAL")
            {
                sql = "REAL";
                if (!string.IsNullOrEmpty(column.DefaultValue))
                    defaultValue = " DEFAULT " + column.DefaultValue;
            }
            if (dataType == "RAW")
            {
                sql = "RAW(" + length + ")";
            }


            if (dataType == "DATE")
            {
                sql = "DATE";
                if (!string.IsNullOrEmpty(column.DefaultValue))
                    defaultValue = " DEFAULT DATE '" + column.DefaultValue + "'";
            }

            if (dataType == "TIMESTAMP")
            {
                sql = "TIMESTAMP" + (precision.HasValue ? " (" + precision + ")" : " (6)");
                if (!string.IsNullOrEmpty(column.DefaultValue))
                    defaultValue = " DEFAULT TIMESTAMP '" + column.DefaultValue + "'";
            }

            if (dataType == "CLOB")
            {
                sql = "CLOB ";
                if (!string.IsNullOrEmpty(column.DefaultValue))
                    defaultValue = AddQuotedDefault(column);
            }

            if (dataType == "BLOB")
            {
                sql = "BLOB ";
                if (!string.IsNullOrEmpty(column.DefaultValue))
                    defaultValue = AddQuotedDefault(column);
            }

            if (string.IsNullOrEmpty(sql))
            {
                sql = column.DbDataType;
                if (!string.IsNullOrEmpty(column.DefaultValue))
                    defaultValue = AddQuotedDefault(column);
            }

            return sql + defaultValue + (!column.Nullable ? " NOT NULL" : string.Empty);
        }

        protected override string NonNativeAutoIncrementWriter()
        {
            //SQLServer table with IDENTITY- let's create the Oracle equivalent
            return new AutoIncrementWriter(Table).Write();
        }

        private static string AddQuotedDefault(DatabaseColumn column)
        {
            return " DEFAULT '" + column.DefaultValue + "'";
        }
    }
}
