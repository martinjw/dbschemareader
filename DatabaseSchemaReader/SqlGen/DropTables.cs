using System;
using System.Text;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen
{
    class DropTables
    {
        public static string Write(DatabaseSchema schema, ISqlFormatProvider formatter)
        {
            var sb = new StringBuilder();
            var lineEnding = formatter.LineEnding();
            //if this is a GO, comment it out too
            if (lineEnding.IndexOf(Environment.NewLine, StringComparison.Ordinal) != -1)
            {
                lineEnding = lineEnding.Replace(Environment.NewLine, Environment.NewLine + "--");
            }
            foreach (var table in schema.Tables)
            {
                foreach (var foreignKey in table.ForeignKeys)
                {
                    sb.AppendLine("-- ALTER TABLE " + formatter.Escape(table.Name) + " DROP CONSTRAINT " + foreignKey.Name + lineEnding);

                }
            }
            foreach (var table in schema.Tables)
            {
                sb.AppendLine("-- DROP TABLE " + formatter.Escape(table.Name) + lineEnding);
            }
            return sb.ToString();
        }
    }
}
