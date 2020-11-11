using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.Filters;
using DatabaseSchemaReader.ProviderSchemaReaders;
using DatabaseSchemaReader.ProviderSchemaReaders.Adapters;
using DatabaseSchemaReader.ProviderSchemaReaders.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable once RedundantUsingDirective
using System.Threading;

namespace DatabaseSchemaReader
{
    /// <summary>
    /// Reads database schema into schema model.
    /// </summary>
    /// <remarks>
    /// Either load independent objects (list of Tables, StoredProcedures), fuller information (a Tables with all Columns, constraints...), or full database schemas (<see cref="ReadAll()"/>: all tables, views, stored procedures with all information; the DatabaseSchema object will hook up the relationships). Obviously the fuller versions will be slow on moderate to large databases.
    /// </remarks>
    public class DatabaseReader : IDatabaseReader
    {
        private readonly SchemaParameters _schemaParameters;
        private readonly ReaderAdapter _readerAdapter;

        //private readonly SchemaExtendedReader _schemaReader;
        private readonly DatabaseSchema _db;

        private bool _fixUp = true;

        /// <summary>
        /// ReaderProgress Event
        /// </summary>
        public event EventHandler<ReaderEventArgs> ReaderProgress;

//#if COREFX
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseReader"/> class from a DbConnection.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public DatabaseReader(System.Data.Common.DbConnection connection) :
            this(connection.GetType().Namespace, connection.ConnectionString, new SchemaParameters(connection))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseReader"/> class from a DbTransaction.
        /// </summary>
        /// <param name="transaction">The transaction</param>
        public DatabaseReader(System.Data.Common.DbTransaction transaction) :
            this(transaction.Connection.GetType().Namespace, transaction.Connection.ConnectionString, new SchemaParameters(transaction))
        {   
        }

        private DatabaseReader(string name, string connectionString, SchemaParameters schemaParameters)
        {
            _db = new DatabaseSchema(connectionString, name);
            _schemaParameters = schemaParameters;
            _schemaParameters.DatabaseSchema = _db;
            _readerAdapter = ReaderAdapterFactory.Create(_schemaParameters);
        }
//#endif
#if !COREFX

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseReader"/> class. For Oracle, use the overload.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="providerName">Name of the provider.</param>
        public DatabaseReader(string connectionString, string providerName)
            : this(new DatabaseSchema(connectionString, providerName))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseReader"/> class for one of the standard providers.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="sqlType">Type of the SQL.</param>
        public DatabaseReader(string connectionString, SqlType sqlType)
        {
            if (connectionString == null) throw new ArgumentNullException("connectionString");
            _schemaParameters = new SchemaParameters(connectionString, sqlType);
            _readerAdapter = ReaderAdapterFactory.Create(_schemaParameters);
            //_schemaReader = SchemaReaderFactory.Create(connectionString, sqlType);
            _db = new DatabaseSchema(connectionString, _schemaParameters.ProviderName);
            _schemaParameters.DatabaseSchema = _db;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseReader"/> class. For Oracle, use this overload.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="providerName">Name of the provider.</param>
        /// <param name="owner">The schema owner.</param>
        public DatabaseReader(string connectionString, string providerName, string owner)
            : this(new DatabaseSchema(connectionString, providerName) { Owner = owner })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseReader"/> class using an existing <see cref="DatabaseSchema"/>.
        /// </summary>
        /// <param name="databaseSchema">The database schema. Can be a subclassed version.</param>
        public DatabaseReader(DatabaseSchema databaseSchema)
        {
            if (databaseSchema == null) throw new ArgumentNullException("databaseSchema");
            if (databaseSchema.ConnectionString == null) throw new ArgumentException("No connectionString");

            _schemaParameters = new SchemaParameters(databaseSchema.ConnectionString, databaseSchema.Provider);
            _schemaParameters.DatabaseSchema = databaseSchema;
            _schemaParameters.Owner = databaseSchema.Owner;
            _readerAdapter = ReaderAdapterFactory.Create(_schemaParameters);
            _db = databaseSchema;
        }

#endif

        private void RaiseReadingProgress(SchemaObjectType schemaObjectType)
        {
            ReaderEventArgs.RaiseEvent(ReaderProgress, this, ProgressType.ReadingSchema, schemaObjectType, null, null, null);
        }

        private void RaiseReadingProgress(object sender, ReaderEventArgs e)
        {
            ReaderEventArgs.RaiseEvent(ReaderProgress, sender, e.ProgressType, e.SchemaObjectType, e.Name, e.Index, e.Count);
        }

        /// <summary>
        /// Exclude specified items when reading schema
        /// </summary>
        /// <value>
        /// The exclusions.
        /// </value>
        public Exclusions Exclusions { get { return _schemaParameters.Exclusions; } }

        /// <summary>
        /// Gets or sets the owner user. Always set it with Oracle (otherwise you'll get SYS, MDSYS etc...)
        /// </summary>
        /// <value>The user.</value>
        public string Owner
        {
            get { return _readerAdapter.Owner; }
            set
            {
                _readerAdapter.Owner = value;
                _db.Owner = value;
            }
        }

        /// <summary>
        /// Set the ADO command timeout. If not set, default is normally 30 seconds
        /// </summary>
        public int? CommandTimeout
        {
            get { return _readerAdapter.CommandTimeout; }
            set { _readerAdapter.CommandTimeout = value; }
        }

        /// <summary>
        /// Gets the database schema. Only call AFTER calling <see cref="ReadAll()"/> or one or more other methods such as <see cref="AllTables()"/>. A collection of Tables, Views and StoredProcedures. Use <see cref="DataSchema.DatabaseSchemaFixer.UpdateReferences(DataSchema.DatabaseSchema)"/> to update object references after loaded. Use <see cref="DataSchema.DatabaseSchemaFixer.UpdateDataTypes(DataSchema.DatabaseSchema)"/> to add datatypes from DbDataType string after loaded.
        /// </summary>
        public DatabaseSchema DatabaseSchema
        {
            get { return _db; }
        }

        /// <summary>
        /// Gets all of the schema in one call.
        /// </summary>
        /// <returns></returns>
        public DatabaseSchema ReadAll()
        {
            return ReadAll(CancellationToken.None);
        }

        /// <summary>
        /// Gets all of the schema in one call.
        /// </summary>
        public DatabaseSchema ReadAll(CancellationToken ct)
        {
            _fixUp = false;
            using (_readerAdapter.CreateConnection())
            {
                if (ct.IsCancellationRequested) return _db;
                DataTypes();

                if (ct.IsCancellationRequested) return _db;
                AllUsers();

                if (ct.IsCancellationRequested) return _db;
                AllSchemas();

                if (ct.IsCancellationRequested) return _db;
                AllTables(ct);

                if (ct.IsCancellationRequested) return _db;
                AllViews(ct);

                if (ct.IsCancellationRequested) return _db;
                AllStoredProcedures(ct);

                if (ct.IsCancellationRequested) return _db;
                AllSequences();
            }
            _fixUp = true;
            UpdateReferences();

            return _db;
        }

        private void AllSequences()
        {
            RaiseReadingProgress(SchemaObjectType.Sequences);
            var sequences = _readerAdapter.Sequences(null);
            DatabaseSchema.Sequences.Clear();
            DatabaseSchema.Sequences.AddRange(sequences);
        }

        /// <summary>
        /// Gets the users (specifically for Oracle)
        /// </summary>
        public IList<DatabaseUser> AllUsers()
        {
            RaiseReadingProgress(SchemaObjectType.Users);
            IList<DatabaseUser> users;
            using (_readerAdapter.CreateConnection())
            {
                users = _readerAdapter.Users();
            }
            DatabaseSchema.Users.Clear();
            DatabaseSchema.Users.AddRange(users);
            return users;
        }

        /// <summary>
        /// Gets the schemas (of supported db engines)
        /// </summary>
        public IList<DatabaseDbSchema> AllSchemas()
        {
            RaiseReadingProgress(SchemaObjectType.Schemas);
            IList<DatabaseDbSchema> schemas;
            using (_readerAdapter.CreateConnection())
            {
                schemas = _readerAdapter.Schemas();
            }
            DatabaseSchema.Schemas.Clear();
            DatabaseSchema.Schemas.AddRange(schemas);
            return schemas;
        }

        /// <summary>
        /// Gets all tables (just names, no columns).
        /// </summary>
        public IList<DatabaseTable> TableList()
        {
            RaiseReadingProgress(SchemaObjectType.Tables);
            using (_readerAdapter.CreateConnection())
            {
                return _readerAdapter.Tables(null);
            }
        }

        /// <summary>
        /// Gets all tables with columns and datatypes but nothing else (no constraints, indexes or triggers).
        /// </summary>
        public IList<DatabaseTable> TablesQuickView()
        {
            RaiseReadingProgress(SchemaObjectType.Tables);
            IList<DatabaseTable> tables;
            using (_readerAdapter.CreateConnection())
            {
                tables = _readerAdapter.Tables(null);
            }
            DatabaseSchema.Tables.Clear();
            DatabaseSchema.Tables.AddRange(tables);

            RaiseReadingProgress(SchemaObjectType.Columns);
            IList<DatabaseColumn> columns;
            using (_readerAdapter.CreateConnection())
            {
                columns = _readerAdapter.Columns(null);
            }

            foreach (var table in DatabaseSchema.Tables)
            {
                table.Columns.Clear();
                table.Columns.AddRange(
                    columns.Where(x => string.Equals(x.TableName, table.Name, StringComparison.OrdinalIgnoreCase)
                                       && string.Equals(x.SchemaOwner, table.SchemaOwner, StringComparison.OrdinalIgnoreCase)));
            }

            DataTypes();

            return DatabaseSchema.Tables;
        }

        /// <summary>
        /// Gets all tables (plus constraints, indexes and triggers).
        /// </summary>
        public IList<DatabaseTable> AllTables()
        {
            return AllTables(CancellationToken.None);
        }

        /// <summary>
        /// Gets all tables (plus constraints, indexes and triggers).
        /// </summary>
        public IList<DatabaseTable> AllTables(CancellationToken ct)
        {
            if (ct.IsCancellationRequested) return new List<DatabaseTable>();
            RaiseReadingProgress(SchemaObjectType.Tables);

            IList<DatabaseTable> tables;
            using (_readerAdapter.CreateConnection())
            {
                var builder = new TableBuilder(_readerAdapter);
                tables = builder.Execute(ct);
            }
            if (ct.IsCancellationRequested) return tables;

            DatabaseSchema.Tables.Clear();
            DatabaseSchema.Tables.AddRange(tables);

            UpdateReferences();
            if (DatabaseSchema.DataTypes.Count == 0)
            {
                DataTypes(); //force load of datatypes
            }
            else
            {
                DatabaseSchemaFixer.UpdateDataTypes(DatabaseSchema);
            }

            return tables;
        }

        /// <summary>
        /// Gets all views.
        /// </summary>
        public IList<DatabaseView> AllViews()
        {
            return AllViews(CancellationToken.None);
        }

        /// <summary>
        /// Gets all views.
        /// </summary>
        public IList<DatabaseView> AllViews(CancellationToken ct)
        {
            if (ct.IsCancellationRequested) return DatabaseSchema.Views;

            IList<DatabaseView> views;
            using (_readerAdapter.CreateConnection())
            {
                var builder = new ViewBuilder(_readerAdapter, Exclusions);
                var handler = ReaderProgress;
                if (handler != null) builder.ReaderProgress += RaiseReadingProgress;
                views = builder.Execute(ct);
            }

            DatabaseSchema.Views.Clear();
            DatabaseSchema.Views.AddRange(views);

            UpdateReferences();
            if (DatabaseSchema.DataTypes.Count > 0)
                DatabaseSchemaFixer.UpdateDataTypes(DatabaseSchema);

            return views;
        }

        /// <summary>
        /// Does table exist?
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">tableName</exception>
        public bool TableExists(string tableName)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException("tableName");
            using (_readerAdapter.CreateConnection())
            {
                var tables = _readerAdapter.Tables(tableName);
                return tables.Count > 0;
            }
        }

        /// <summary>
        /// Gets the table. If <see cref="Owner"/> is specified, it is used.
        /// </summary>
        /// <param name="tableName">Name of the table. Oracle names can be case sensitive.</param>
        public DatabaseTable Table(string tableName)
        {
            return Table(tableName, CancellationToken.None);
        }

        /// <summary>
        /// Gets the table. If <see cref="Owner" /> is specified, it is used.
        /// </summary>
        /// <param name="tableName">Name of the table. Oracle names can be case sensitive.</param>
        /// <param name="ct">The ct.</param>
        public DatabaseTable Table(string tableName, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException("tableName");

            DatabaseTable table;
            using (_readerAdapter.CreateConnection())
            {
                var builder = new TableBuilder(_readerAdapter);
                var handler = ReaderProgress;
                if (handler != null) builder.ReaderProgress += RaiseReadingProgress;
                table = builder.Execute(ct, tableName);
            }

            var existingTable = DatabaseSchema.FindTableByName(tableName, _schemaParameters.Owner);
            if (existingTable != null)
            {
                DatabaseSchema.Tables.Remove(existingTable);
            }
            DatabaseSchema.Tables.Add(table);

            if (ct.IsCancellationRequested) return table;

            if (DatabaseSchema.DataTypes.Count > 0)
                DatabaseSchemaFixer.UpdateDataTypes(DatabaseSchema);

            return table;
        }

        /// <summary>
        /// Gets all stored procedures (no arguments, for Oracle no packages)
        /// </summary>
        public IList<DatabaseStoredProcedure> StoredProcedureList()
        {
            using (_readerAdapter.CreateConnection())
            {
                return _readerAdapter.StoredProcedures(null);
            }
        }

        /// <summary>
        /// Gets all stored procedures (and functions) with their arguments
        /// </summary>
        /// <remarks>
        /// <para>We also get the source (if available)</para>
        /// <para>We don't get functions here.</para>
        /// <para>In Oracle stored procedures are often in packages. We read the non-packaged stored procedures, then add packaged stored procedures if they have arguments. If they don't have arguments, they are not found.</para>
        /// </remarks>
        public IList<DatabaseStoredProcedure> AllStoredProcedures()
        {
            return AllStoredProcedures(CancellationToken.None);
        }

        /// <summary>
        /// Gets all stored procedures (and functions) with their arguments
        /// </summary>
        public IList<DatabaseStoredProcedure> AllStoredProcedures(CancellationToken ct)
        {
            using (_readerAdapter.CreateConnection())
            {
                var builder = new ProcedureBuilder(_readerAdapter, DatabaseSchema, Exclusions);
                var handler = ReaderProgress;
                if (handler != null) builder.ReaderProgress += RaiseReadingProgress;
                builder.Execute(ct);
            }

            UpdateReferences();

            return DatabaseSchema.StoredProcedures;
        }

        /// <summary>
        /// Gets all datatypes (and updates columns/arguments if already loaded)
        /// </summary>
        public IList<DataType> DataTypes()
        {
            var list = _readerAdapter.DataTypes();
            DatabaseSchema.DataTypes.Clear();
            DatabaseSchema.DataTypes.AddRange(list);
            DatabaseSchemaFixer.UpdateDataTypes(DatabaseSchema); //if columns/arguments loaded later, run this method again.
            return list;
        }

        private void UpdateReferences()
        {
            //a simple latch so ReadAll will only call this at the end
            if (!_fixUp) return;

            DatabaseSchemaFixer.UpdateReferences(DatabaseSchema); //updates all references
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                //nothing to dispose?
            }
        }

        #endregion Implementation of IDisposable
    }
}
