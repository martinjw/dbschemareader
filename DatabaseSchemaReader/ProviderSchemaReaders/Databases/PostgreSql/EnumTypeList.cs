using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.PostgreSql
{
    class EnumTypeList : SqlExecuter<DataType>
    {
        public EnumTypeList()
        {
            Sql = @"select * from pg_type pgt join pg_enum pge on pgt.oid = pge.enumtypid;";
        }

        protected override void AddParameters(DbCommand command)
        {
            //AddDbParameter(command, "OWNER", Owner);
            //AddDbParameter(command, "TABLENAME", _tableName);
        }

        protected override void Mapper(IDataRecord record)
        {
            //var schema = record["table_schema"].ToString();
            //var name = record["table_name"].ToString();
            //var table = new DatabaseTable
            //{
            //    Name = name,
            //    SchemaOwner = schema
            //};

            //Result.Add(table);

        }

        public IList<DataType> Execute(IConnectionAdapter connectionAdapter)
        {
            ExecuteDbReader(connectionAdapter);
            return Result;            
        }
    }
}
