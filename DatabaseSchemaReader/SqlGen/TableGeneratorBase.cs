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
            IncludeDefaultValues = true;
        }

        protected abstract ISqlFormatProvider SqlFormatProvider();

        private string EscapeName(string name)
        {
            return SqlFormatProvider().Escape(name);
        }
        public abstract string WriteDataType(DatabaseColumn column);
        protected abstract string NonNativeAutoIncrementWriter();

        protected abstract string ConstraintWriter();

        protected virtual void AddTableConstraints(IList<string> columnList)
        {
            //override this to add constraints after columns
        }

        public bool IncludeSchema { get; set; }

        public bool IncludeDefaultValues { get; set; }

        public virtual string Write()
        {
            var sb = new StringBuilder();

            sb.AppendLine("CREATE TABLE " + SchemaTableName(Table));
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

            if (Table.HasAutoNumberColumn)
            {
                //If no native identity/autoincrement, add it
                sb.AppendLine(NonNativeAutoIncrementWriter());
            }

            return sb.ToString();
        }

        protected string SchemaTableName(DatabaseTable databaseTable)
        {
            return SchemaName(databaseTable.SchemaOwner) + EscapeName(databaseTable.Name);
        }

        /// <summary>
        /// If there is a schema (eg "dbo") returns it escaped with trailing dot ("[dbo].")
        /// </summary>
        protected string SchemaName(string schema)
        {
            if (IncludeSchema && !string.IsNullOrEmpty(schema))
            {
                return EscapeName(schema) + ".";
            }
            return string.Empty;
        }

        public string WriteColumn(DatabaseColumn column)
        {
            return "  " + EscapeName(column.Name) + " " + WriteDataType(column).TrimEnd();
        }

    }
}
