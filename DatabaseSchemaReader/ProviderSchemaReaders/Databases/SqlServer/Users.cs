using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.SqlServer
{
    class Users : SqlExecuter<DatabaseUser>
    {
        public Users()
        {
            Sql = @"select name from sysusers";
        }

        protected override void AddParameters(DbCommand command)
        {
        }

        protected override void Mapper(IDataRecord record)
        {
            var name = record.GetString("name");
            var constraint = new DatabaseUser
            {
                Name = name,
            };
            Result.Add(constraint);
        }

        public IList<DatabaseUser> Execute(DbConnection dbConnection, DbTransaction transaction)
        {
            ExecuteDbReader(dbConnection, transaction);
            return Result;
        }
    }
}