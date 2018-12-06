using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;
using System.Collections.Generic;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.SQLite
{
    internal class ViewColumns
    {
        private readonly string _viewName;

        public ViewColumns(int? commandTimeout, string viewName)
        {
            CommandTimeout = commandTimeout;
            _viewName = viewName;
            PragmaSql = @"PRAGMA table_info('{0}')";
        }

        protected List<DatabaseColumn> Result { get; } = new List<DatabaseColumn>();
        public string PragmaSql { get; set; }
        public int? CommandTimeout { get; set; }

        public IList<DatabaseColumn> Execute(IConnectionAdapter connectionAdapter)
        {
            var views = new Views(CommandTimeout, _viewName).Execute(connectionAdapter);

            foreach (var view in views)
            {
                var viewName = view.Name;
                using (var cmd = connectionAdapter.DbConnection.CreateCommand())
                {
                    cmd.CommandText = string.Format(PragmaSql, viewName);
                    if (CommandTimeout.HasValue && CommandTimeout.Value >= 0) cmd.CommandTimeout = CommandTimeout.Value;
                    int ordinal = 0;
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            var colName = dr.GetString("name");
                            var col = new DatabaseColumn
                            {
                                TableName = viewName,
                                SchemaOwner = "",
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
