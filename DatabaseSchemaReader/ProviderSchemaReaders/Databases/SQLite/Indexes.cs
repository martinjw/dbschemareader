using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.SQLite
{
    class Indexes : SqlExecuter<DatabaseIndex>
    {
        private readonly string _tableName;

        public Indexes(string tableName)
        {
            _tableName = tableName;
            Sql = @"SELECT
  name,
  tbl_name,
  sql
FROM sqlite_master
WHERE type = 'index'
AND (tbl_name = @TABLE_NAME OR (@TABLE_NAME IS NULL))
ORDER BY tbl_name, name";
            PragmaSql = @"PRAGMA index_info('{0}')";
        }

        public string PragmaSql { get; set; }

        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "TABLE_NAME", _tableName);
        }

        protected override void Mapper(IDataRecord record)
        {
            var tableName = record.GetString("tbl_name");
            var name = record.GetString("name");
            var index = Result.FirstOrDefault(f => f.Name == name && f.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase));
            if (index == null)
            {
                index = new DatabaseIndex
                {
                    SchemaOwner = null,
                    TableName = tableName,
                    Name = name,
                };
                Result.Add(index);
            }
        }

        public IList<DatabaseIndex> Execute(DbConnection dbConnection, DbTransaction transaction)
        {
            ExecuteDbReader(dbConnection, transaction);

            foreach (var index in Result)
            {
                var name = index.Name;
                using (var cmd = dbConnection.CreateCommand())
                {
                    cmd.CommandText = string.Format(PragmaSql, name);
                    int ordinal = 0;
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            var colName = dr.GetString("name");
                            var col = new DatabaseColumn
                            {
                                Name = colName,
                                Ordinal = ordinal,
                            };
                            index.Columns.Add(col);
                            ordinal++;
                        }
                    }
                }
            }

            return Result;
        }
    }
}