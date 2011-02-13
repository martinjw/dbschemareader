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

        protected abstract string LineEnding();
        protected abstract string EscapeName(string name);
        protected abstract string WriteDataType(DatabaseColumn column);
        protected abstract string NonNativeAutoIncrementWriter();

        protected abstract string ConstraintWriter();

        public bool IncludeSchema { get; set; }

        public string Write()
        {
            var sb = new StringBuilder();

            sb.AppendLine("CREATE TABLE " + (IncludeSchema ? EscapeName(Table.SchemaOwner) + "." : string.Empty) + EscapeName(TableName));
            sb.AppendLine("(");
            var columnList = new List<string>();
            foreach (var column in Table.Columns)
            {
                columnList.Add("  " + EscapeName(column.Name) + " " + WriteDataType(column));
            }
            sb.AppendLine(string.Join("," + Environment.NewLine, columnList.ToArray()));

            sb.AppendLine(")" + LineEnding());

            sb.AppendLine(ConstraintWriter());

            if (Table.HasIdentityColumn)
            {
                //If no native identity/autoincrement, add it
                sb.AppendLine(NonNativeAutoIncrementWriter());
            }

            return sb.ToString();

        }
    }
}
