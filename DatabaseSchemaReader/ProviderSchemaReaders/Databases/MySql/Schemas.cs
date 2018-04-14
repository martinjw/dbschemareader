﻿using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.MySql
{
    class Schemas : SqlExecuter<DatabaseDbSchema>
    {
        public Schemas()
        {
            Sql = @"SELECT DATABASE() as name";
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