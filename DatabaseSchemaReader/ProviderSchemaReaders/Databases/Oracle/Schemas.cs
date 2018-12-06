using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Oracle
{
    class Schemas : SqlExecuter<DatabaseDbSchema>
    {
        public Schemas(int? commandTimeout) : base(commandTimeout, null)
        {
            Sql = @"SELECT USERNAME AS name FROM ALL_USERS ORDER BY USERNAME";
        }

        protected override void AddParameters(DbCommand command)
        {
        }

        protected override void Mapper(IDataRecord record)
        {
            var name = record.GetString("name");
            var schema = new DatabaseDbSchema
            {
                Name = name,
            };
            Result.Add(schema);
        }

        public IList<DatabaseDbSchema> Execute(IConnectionAdapter connectionAdapter)
        {
            ExecuteDbReader(connectionAdapter);
            return Result;
        }
    }
}