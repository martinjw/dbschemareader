using System.Collections.Generic;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.Databases.PostgreSql;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Adapters
{
    class PostgreSqlAdapter : ReaderAdapter
    {
        public PostgreSqlAdapter(SchemaParameters schemaParameters) : base(schemaParameters)
        {
        }
        public override IList<DataType> DataTypes()
        {
            return new DataTypeList().Execute();
        }

    }
}