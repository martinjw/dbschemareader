using System;
using System.Collections.Generic;
using System.Text;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen
{
    abstract class TableGeneratorBase : ITableGenerator
    {
        protected readonly DatabaseTable Table;
        protected readonly string TableName;

        protected TableGeneratorBase(DatabaseTable table)
        {
            Table = table;
            TableName = table.Name;
            IncludeSchema = true;
        }

        protected abstract ISqlFormatProvider SqlFormatProvider();

        private string EscapeName(string name)
        {
            return SqlFormatProvider().Escape(name);
        }
        protected abstract string WriteDataType(DatabaseColumn column);
        protected abstract string NonNativeAutoIncrementWriter();

        protected abstract string ConstraintWriter();

        protected virtual void AddTableConstraints(IList<string> columnList)
        {
            //override this to add constraints after columns
        }

        public bool IncludeSchema { get; set; }

        public string Write()
        {
            var sb = new StringBuilder();

            var schemaName = (IncludeSchema && !string.IsNullOrEmpty(Table.SchemaOwner)) ? EscapeName(Table.SchemaOwner) + "." : string.Empty;

            sb.AppendLine("CREATE TABLE " + schemaName + EscapeName(TableName));
            sb.AppendLine("(");
            var columnList = new List<string>();
            foreach (var column in Table.Columns)
            {
                columnList.Add(WriteColumn(column));
            }
            AddTableConstraints(columnList);
            sb.AppendLine(string.Join("," + Environment.NewLine, columnList.ToArray()));

            sb.AppendLine(")" + SqlFormatProvider().LineEnding());

            sb.AppendLine(ConstraintWriter());

            if (Table.HasIdentityColumn)
            {
                //If no native identity/autoincrement, add it
                sb.AppendLine(NonNativeAutoIncrementWriter());
            }

            return sb.ToString();
        }

        public string WriteColumn(DatabaseColumn column)
        {
            return "  " + EscapeName(column.Name) + " " + WriteDataType(column).TrimEnd();
        }

    }
}
