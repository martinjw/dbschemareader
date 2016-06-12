using System.Collections.Generic;
using System.Data;
using System.Linq;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Conversion
{
    class TableDescriptionConverter
    {
        private readonly IList<DatabaseTable> _list;

        public TableDescriptionConverter(DataTable dataTable)
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
            const string descKey = "TableDescription";

            foreach (DataRow row in dataTable.Rows)
            {
                var schema = row[schemaKey].ToString();
                var name = row[tableKey].ToString();
                var desc = row[descKey].ToString();
                var table = new DatabaseTable();
                table.Name = name;
                table.SchemaOwner = schema;
                table.Description = desc;
                list.Add(table);
            }
            return list;
        }


        public string FindDescription(string schema, string tableName)
        {
            var find = _list.FirstOrDefault(t => t.SchemaOwner == schema && t.Name == tableName);
            return find == null ? null : find.Description;
        }
    }
}
