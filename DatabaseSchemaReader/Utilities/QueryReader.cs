#if !COREFX
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders;
using DatabaseSchemaReader.ProviderSchemaReaders.Adapters;
using DatabaseSchemaReader.Conversion;

namespace DatabaseSchemaReader.Utilities
{
    /// <summary>
    /// Reads schema information from arbitrary sql
    /// </summary>
    public class QueryReader
    {

        /// <summary>
        /// Gets all query columns
        /// </summary>
        public IList<DatabaseColumn> GetQueryColumns(string connectionString, string providerName, string sql)
        {
            var dbFactory = DbProviderFactories.GetFactory(providerName);
            using (var con = dbFactory.CreateConnection())
            {
                con.ConnectionString = connectionString;
                con.Open();
                return GetQueryColumns(con, sql);
            }
        }

        /// <summary>
        /// Gets all query columns
        /// </summary>
        public IList<DatabaseColumn> GetQueryColumns(DbConnection connection, string sql)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));
            var schemaParameters = new SchemaParameters(connection);
            var readerAdapter = ReaderAdapterFactory.Create(schemaParameters);
            var types = readerAdapter.DataTypes();

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = sql;

                return GetQueryColumns(cmd, types);
            }
        }

        /// <summary>
        /// Gets all query columns from a command (assign any parameters if required)
        /// </summary>
        public IList<DatabaseColumn> GetQueryColumns(DbCommand command, IList<DataType> types)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            using (var rdr = command.ExecuteReader(System.Data.CommandBehavior.KeyInfo))
            {
                using (var colTable = rdr.GetSchemaTable())
                {
                    var columnConverter = new ColumnConverter(colTable);
                    var cols = columnConverter.Columns().ToList();

                    // It is necessade to create a new list of DataType using connection.GetSchema(DbMetaDataCollectionNames.DataTypes) 
                    // because the ProviderDbType in the list returned by DataTypes() is sometimes wrong.
                    //var readerAdapter = new DbProviderReaderAdapter(schemaParameters);
                    DatabaseSchemaFixer.UpdateDataTypes(types, cols);
                    return cols;
                }
            }
        }
    }
}
#endif
