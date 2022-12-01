using DatabaseSchemaReader.DataSchema;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Builders
{
    internal static class TableIndexMerger
    {
        public static void UpdateIndexes(DatabaseTable table, IList<DatabaseIndex> indexes)
        {
            if (indexes == null || indexes.Count == 0) return;
            var tableIndexes = indexes.Where(x => x.SchemaOwner == table.SchemaOwner &&
                                                  x.TableName == table.Name);
            foreach (var index in tableIndexes)
            {
                //we don't need the index columns to be the actual table columns- the ordinals are different
                //var list = new List<DatabaseColumn>();
                foreach (var indexColumn in index.Columns)
                {
                    var tableColumn = table.Columns.FirstOrDefault(c => c.Name == indexColumn.Name);
                    if (tableColumn != null)
                    {
                        //list.Add(tableColumn);
                        //copy a few properties that might be useful instead of cross referencing manually
                        indexColumn.DbDataType = tableColumn.DbDataType;
                        indexColumn.Length = tableColumn.Length;
                        indexColumn.Scale = tableColumn.Scale;
                        indexColumn.Precision = tableColumn.Precision;
                        indexColumn.Nullable = tableColumn.Nullable;
                        indexColumn.TableName = tableColumn.TableName;
                    }
                }
                //index.Columns.Clear();
                //index.Columns.AddRange(list);
                table.AddIndex(index);
            }
        }
    }
}