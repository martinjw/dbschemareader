using System.Collections.Generic;
using System.Data;
using System.Linq;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Conversion
{
    class ColumnDescriptionConverter
    {
        private readonly IList<DatabaseTable> _list;

        public ColumnDescriptionConverter(DataTable dataTable)
        {
            _list = Convert(dataTable);
        }

        public IList<DatabaseTable> Result()
        {
            return _list;
        }

        private static IList<DatabaseTable> Convert(DataTable dataTable)
        {
            var list = new List<DatabaseTable>();
            if ((dataTable == null) || (dataTable.Columns.Count == 0) || (dataTable.Rows.Count == 0))
            {
                return list;
            }

            const string schemaKey = "SchemaOwner";
            const string tableKey = "TableName";
            const string descKey = "ColumnDescription";
            const string columnKey = "ColumnName";

            foreach (DataRow row in dataTable.Rows)
            {
                var schema = row[schemaKey].ToString();
                var name = row[tableKey].ToString();
                var col = row[columnKey].ToString();
                var desc = row[descKey].ToString();
                var table = list.FirstOrDefault(t => t.SchemaOwner == schema && t.Name == name);
                if (table == null)
                {
                    table = new DatabaseTable();
                    table.Name = name;
                    table.SchemaOwner = schema;
                    list.Add(table);
                }
                table.AddColumn(col).Description = desc;
            }
            return list;
        }


        public void AddDescriptions(DatabaseTable table)
        {
            var find = _list.FirstOrDefault(t => t.SchemaOwner == table.SchemaOwner && t.Name == table.Name);
            if (find == null) return;
            foreach (var col in find.Columns)
            {
                var match = table.FindColumn(col.Name);
                if (match != null) match.Description = col.Description;
            }
        }
    }
}
