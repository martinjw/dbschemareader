using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Firebird
{
    class Users : SqlExecuter<DatabaseUser>
    {
        public Users(int? commandTimeout) : base(commandTimeout, null)
        {
            Sql = @"SELECT DISTINCT RDB$USER FROM RDB$USER_PRIVILEGES";
        }

        protected override void AddParameters(DbCommand command)
        {
        }

        protected override void Mapper(IDataRecord record)
        {
            var name = record.GetString("RDB$USER").Trim();
            var constraint = new DatabaseUser
            {
                Name = name,
            };
            Result.Add(constraint);
        }

        public IList<DatabaseUser> Execute(IConnectionAdapter connectionAdapter)
        {
            ExecuteDbReader(connectionAdapter);
            return Result;
        }
    }
}