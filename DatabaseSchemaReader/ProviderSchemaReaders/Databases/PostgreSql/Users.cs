using DatabaseSchemaReader.DataSchema;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.PostgreSql
{
    internal class Users : SqlExecuter<DatabaseUser>
    {
        public Users()
        {
            Sql = @"SELECT usename as user_name FROM pg_catalog.pg_user";
        }

        public IList<DatabaseUser> Execute(DbConnection connection, DbTransaction transaction)
        {
            ExecuteDbReader(connection, transaction);
            return Result;
        }

        protected override void AddParameters(DbCommand command)
        {
        }

        protected override void Mapper(IDataRecord record)
        {
            var name = record["user_name"].ToString();
            var table = new DatabaseUser
            {
                Name = name,
            };

            Result.Add(table);
        }
    }
}