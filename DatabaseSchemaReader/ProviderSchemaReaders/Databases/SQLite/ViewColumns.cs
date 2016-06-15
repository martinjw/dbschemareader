using System.Collections.Generic;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.SQLite
{
    internal class ViewColumns
    {
        private readonly string _viewName;

        public ViewColumns(string viewName)
        {
            _viewName = viewName;
            PragmaSql = @"PRAGMA table_info('{0}')";
        }

        protected List<DatabaseColumn> Result { get; } = new List<DatabaseColumn>();
        public string PragmaSql { get; set; }

        public IList<DatabaseColumn> Execute(DbConnection connection)
        {
            var views = new Views(_viewName).Execute(connection);

            foreach (var view in views)
            {
                var viewName = view.Name;
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = string.Format(PragmaSql, viewName);
                    int ordinal = 0;
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            var colName = dr.GetString("name");
                            var col = new DatabaseColumn
                                      {
                                          TableName = viewName,
                                          Name = colName,
                                          Ordinal = ordinal,
                                          //type will be like "nvarchar(32)".
                                          //Lengths /precisions could be parsed out (nb remember this is Sqlite)
                                          DbDataType = dr.GetString("type"),
                                          Nullable = dr.GetBoolean("notnull"),
                                          DefaultValue = dr.GetString("dflt_value"),
                                          IsPrimaryKey = dr.GetBoolean("pk"),
                                      };
                            Result.Add(col);
                            ordinal++;
                        }
                    }
                }
            }

            return Result;
        }
    }
}
