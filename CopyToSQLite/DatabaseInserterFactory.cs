using System.Data.Common;
using DatabaseSchemaReader.DataSchema;

namespace CopyToSQLite
{
    class DatabaseInserterFactory
    {
        private readonly bool _useSqlServerCe;

        public DatabaseInserterFactory(bool useSqlServerCe)
        {
            _useSqlServerCe = useSqlServerCe;
        }

        public DatabaseInserter CreateDatabaseInserter(DbConnection connection, string insertSql, DatabaseTable databaseTable)
        {
            if (_useSqlServerCe)
            {
                return new SqlServerInserter(connection, insertSql, databaseTable);
            }
            return new DatabaseInserter(connection, insertSql);
        }
    }
}
