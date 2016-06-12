using System.Collections.Generic;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.Databases.MySql;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Adapters
{
    class MySqlAdapter : ReaderAdapter
    {
        public MySqlAdapter(SchemaParameters schemaParameters) : base(schemaParameters)
        {
        }
        public override IList<DataType> DataTypes()
        {
            return new DataTypeList().Execute();
        }

    }
}