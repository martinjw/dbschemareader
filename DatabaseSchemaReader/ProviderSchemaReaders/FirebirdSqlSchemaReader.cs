using System.Data;
using System.Data.Common;

namespace DatabaseSchemaReader.ProviderSchemaReaders
{
    class FirebirdSqlSchemaReader : SchemaExtendedReader
    {
        public FirebirdSqlSchemaReader(string connectionString, string providerName) : base(connectionString, providerName)
        {
        }

        internal override string CheckConstraintsCollectionName
        {
            get { return "CheckConstraintsByTable"; }
        }
    }
}