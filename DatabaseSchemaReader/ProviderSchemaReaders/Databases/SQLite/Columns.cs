using System.Collections.Generic;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.SQLite
{
    internal class Columns
    {
        private readonly string _tableName;

        public Columns(string tableName)
        {
            _tableName = tableName;
            PragmaSql = @"PRAGMA table_info('{0}')";
        }

        protected List<DatabaseColumn> Result { get; } = new List<DatabaseColumn>();
        public string PragmaSql { get; set; }

        public IList<DatabaseColumn> Execute(DbConnection connection)
        {
            var tables = new Tables(_tableName).Execute(connection);

            foreach (var table in tables)
            {
                var tableName = table.Name;
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = string.Format(PragmaSql, tableName);
                    int ordinal = 0;
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            var colName = dr.GetString("name");
                            var col = new DatabaseColumn
                            {
                                TableName = tableName,
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