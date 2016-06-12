using System.Collections.Generic;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.Databases.SqlServerCe;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Adapters
{
    class SqlServerCeAdapter : ReaderAdapter
    {
        public SqlServerCeAdapter(SchemaParameters schemaParameters) : base(schemaParameters)
        {
        }
        public override IList<DataType> DataTypes()
        {
            return new DataTypeList().Execute();
        }

    }
}