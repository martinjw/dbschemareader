using System.Data.Common;

namespace CopyToSQLite
{
    internal interface IDatabaseCreator
    {
        DbConnection CreateConnection();
        void CreateTables(string ddl);
    }
}