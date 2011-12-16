using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using DatabaseSchemaReader.Conversion.KeyMaps;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Conversion
{
    class IndexConverter
    {
        private readonly IList<DatabaseIndex> _indexes = new List<DatabaseIndex>();

        public IndexConverter(DataTable indexes, DataTable indexColumns)
        {
            ConvertIndexes(indexes, _indexes);
            ConvertIndexes(indexColumns, _indexes);
        }
        public void AddIndexColumns(ICollection<DatabaseIndex> indexes, DataTable indexColumns)
        {
            ConvertIndexes(indexColumns, indexes);
        }

        public IEnumerable<DatabaseIndex> Indexes(string tableName)
        {
            return _indexes.Where(i => i.TableName == tableName);
        }

        private static void ConvertIndexes(DataTable dt, ICollection<DatabaseIndex> indexes)
        {
            if (dt == null) return;
            //Npgsql
            if (dt.Columns.Count == 0) return;

            var indexKeyMap = new IndexKeyMap(dt);

            foreach (DataRowView row in dt.DefaultView)
            {
                string name = row[indexKeyMap.Key].ToString();
                if (string.IsNullOrEmpty(name)) continue; //all indexes should have a name
                string schema = !String.IsNullOrEmpty(indexKeyMap.SchemaKey) ? row[indexKeyMap.SchemaKey].ToString() : String.Empty;
                var tableName = row[indexKeyMap.TableKey].ToString();
                var c = indexes.FirstOrDefault(f => f.Name == name && f.SchemaOwner == schema && f.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase));
                if (c == null)
                {
                    c = new DatabaseIndex();
                    c.Name = name;
                    c.SchemaOwner = schema;
                    c.TableName = tableName;
                    if (indexKeyMap.Typekey != null)
                        c.IndexType = row[indexKeyMap.Typekey].ToString();
                    if (FindBoolean(row, indexKeyMap.UniqueKey, "UNIQUE"))
                    {
                        c.IsUnique = true;
                        c.IndexType = "UNIQUE";
                    }
                    if (FindBoolean(row, indexKeyMap.PrimaryKey, String.Empty))
                        c.IndexType = "PRIMARY"; //primary keys should be unique too
                    indexes.Add(c);
                }
                if (string.IsNullOrEmpty(indexKeyMap.ColumnKey)) continue;

                string colName = row[indexKeyMap.ColumnKey].ToString();
                if (string.IsNullOrEmpty(colName)) continue;
                var column = new DatabaseColumn();
                column.Name = colName;
                if (!string.IsNullOrEmpty(indexKeyMap.OrdinalKey))
                {
                    int ordinal = Convert.ToInt32(row[indexKeyMap.OrdinalKey], CultureInfo.CurrentCulture);
                    column.Ordinal = ordinal;
                }
                if (ContainsColumn(c.Columns, colName)) continue;
                c.Columns.Add(column);
                if (c.Columns.Count > 1 && column.Ordinal != 0)
                {
                    //the order of the datatable may be wrong
                    c.Columns.Sort((x, y) => x.Ordinal.CompareTo(y.Ordinal));
                }
            }
        }


        private static bool ContainsColumn(ICollection<DatabaseColumn> columns, string columnName)
        {
            if (columns.Count == 0) return false;
            foreach (var column in columns)
            {
                if (column.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }


        private static bool FindBoolean(DataRowView row, string key, string trueText)
        {
            if (key == null) return false;
            var o = row[key];
            if (o == DBNull.Value) return false;
            if (o is string)
            {
                return (o.Equals(trueText));
            }
            return (bool)o;
        }
    }
}
