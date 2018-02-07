using System.Collections.Generic;
using System.Linq;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.SQLite
{
    class Constraints
    {
        private readonly string _tableName;

        public Constraints(string tableName)
        {
            _tableName = tableName;
            PragmaSql = @"PRAGMA foreign_key_list('{0}')";
        }

        protected List<DatabaseConstraint> Result { get; } = new List<DatabaseConstraint>();
        public string PragmaSql { get; set; }

        public IList<DatabaseConstraint> Execute(IConnectionAdapter connectionAdapter)
        {
            var tables = new Tables(_tableName, new SchemaFactory()).Execute(connectionAdapter);

            foreach (var table in tables)
            {
                var tableName = table.Name;
                using (var cmd = connectionAdapter.DbConnection.CreateCommand())
                {
                    cmd.CommandText = string.Format(PragmaSql, tableName);
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            var refersToTable = dr.GetString("table");
                            var con =
                                Result.FirstOrDefault(x => x.TableName == tableName && x.RefersToTable == refersToTable);
                            if (con == null)
                            {
                                con = new DatabaseConstraint
                                {
                                    TableName = tableName,
                                    SchemaOwner = "",
                                    ConstraintType = ConstraintType.ForeignKey,
                                    RefersToTable = refersToTable,
                                    UpdateRule = dr.GetString("on_update"),
                                    DeleteRule = dr.GetString("on_delete"),
                                };
                                Result.Add(con);
                            }
                            con.Columns.Add(dr.GetString("from"));

                        }
                    }
                }
            }

            return Result;
        }
    }


    class PkConstraints
    {
        private readonly string _tableName;

        public PkConstraints(string tableName)
        {
            _tableName = tableName;
            PragmaSql = @"PRAGMA table_info('{0}')";
        }

        protected List<DatabaseConstraint> Result { get; } = new List<DatabaseConstraint>();
        public string PragmaSql { get; set; }

        public IList<DatabaseConstraint> Execute(IConnectionAdapter connectionAdapter)
        {
            var tables = new Tables(_tableName, new SchemaFactory()).Execute(connectionAdapter);

            foreach (var table in tables)
            {
                var tableName = table.Name;
                using (var cmd = connectionAdapter.DbConnection.CreateCommand())
                {
                    cmd.CommandText = string.Format(PragmaSql, tableName);
                    using (var dr = cmd.ExecuteReader())
                    {
                        var columns = new List<string>();
                        while (dr.Read())
                        {
                            var colName = dr.GetString("name");
                            bool isPrimaryKey = dr.GetBoolean("pk");
                            if (isPrimaryKey == false)
                                continue;

                            columns.Add(colName);
                        }

                        var con = new DatabaseConstraint
                        {
                            TableName = tableName,
                            SchemaOwner = "",
                            ConstraintType = ConstraintType.PrimaryKey,
                        };
                        con.Columns.AddRange(columns);
                        Result.Add(con);
                    }
                }
            }

            return Result;
        }
    }
}