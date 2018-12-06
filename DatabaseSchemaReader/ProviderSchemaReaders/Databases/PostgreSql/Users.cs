using DatabaseSchemaReader.DataSchema;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.PostgreSql
{
    internal class Users : SqlExecuter<DatabaseUser>
    {
        public Users(int? commandTimeout) : base(commandTimeout, null)
        {
            Sql = @"SELECT usename as user_name FROM pg_catalog.pg_user";
        }

        public IList<DatabaseUser> Execute(IConnectionAdapter connectionAdapter)
        {
            ExecuteDbReader(connectionAdapter);
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