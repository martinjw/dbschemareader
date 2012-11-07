using System.Data;
using System.Data.Common;

namespace DatabaseSchemaReader.ProviderSchemaReaders
{
    /// <summary>
    /// Support for VistaDB database
    /// </summary>
    class VistaDbSchemaReader : SchemaExtendedReader
    {
        public VistaDbSchemaReader(string connectionString, string providerName)
            : base(connectionString, providerName)
        {
        }

        public override DataTable ViewColumns(string viewName)
        {
            using (DbConnection connection = Factory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;
                connection.Open();

                var collectionName = ViewColumnsCollectionName;
                return GenericCollection(collectionName, connection, viewName);
            }
        }

        protected override DataTable StoredProcedures(DbConnection connection)
        {
            //it's not reported in the MetaDataCollections, but it is there.
            var collectionName = ProceduresCollectionName;
            return connection.GetSchema(collectionName, new string[] { });
        }

        protected override DataTable StoredProcedureArguments(string storedProcedureName, DbConnection connection)
        {
            //it's not reported in the MetaDataCollections, but it is there.
            var dt = connection.GetSchema("PROCEDUREPARAMETERS", new[] { null, null, storedProcedureName, null });
            dt.TableName = ProcedureParametersCollectionName;
            return dt;
        }

    }
}