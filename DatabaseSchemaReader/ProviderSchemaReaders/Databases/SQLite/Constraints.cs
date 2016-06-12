using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using DatabaseSchemaReader.DataSchema;

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

        public IList<DatabaseConstraint> Execute(DbConnection connection)
        {
            var tables = new Tables(_tableName).Execute(connection);

            foreach (var table in tables)
            {
                var tableName = table.Name;
                using (var cmd = connection.CreateCommand())
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
}