using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// The parent of all schema objects.
    /// </summary>
    /// <remarks>
    /// When initially populated, many of the objects (tables, stored procedures) are not linked.
    /// Use <see cref="DatabaseSchemaFixer.UpdateReferences" /> to link things up
    /// </remarks>
    [Serializable]
    public partial class DatabaseSchema
    {
        #region Fields
        //backing fields
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly List<DatabaseTable> _tables;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly List<DatabaseView> _views;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly List<DataType> _dataTypes;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly List<DatabaseStoredProcedure> _storedProcedures;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly List<DatabasePackage> _packages;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly List<DatabaseSequence> _sequences;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly List<DatabaseFunction> _functions;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly List<DatabaseUser> _users;
        #endregion


        private DatabaseSchema()
            : this(null, null)
        {
            //private constructor used for xmlserialization
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseSchema"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="sqlType">Type of the provider</param>
        public DatabaseSchema(string connectionString, SqlType sqlType)
            :this(connectionString, null)
        {
            Provider = ProviderToSqlType.Convert(sqlType);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseSchema"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="providerName">Name of the provider.</param>
        public DatabaseSchema(string connectionString, string providerName)
        {
            ConnectionString = connectionString;
            Provider = providerName;

            _packages = new List<DatabasePackage>();
            _views = new List<DatabaseView>();
            _users = new List<DatabaseUser>();
            _sequences = new List<DatabaseSequence>();
            _functions = new List<DatabaseFunction>();
            _tables = new List<DatabaseTable>();
            _storedProcedures = new List<DatabaseStoredProcedure>();
            _dataTypes = new List<DataType>();
        }

        /// <summary>
        /// Gets the data types.
        /// </summary>
        public List<DataType> DataTypes => _dataTypes;

        /// <summary>
        /// Gets the stored procedures.
        /// </summary>
        public List<DatabaseStoredProcedure> StoredProcedures => _storedProcedures;

        /// <summary>
        /// Gets the packages.
        /// </summary>
        public List<DatabasePackage> Packages => _packages;

        /// <summary>
        /// Gets the tables.
        /// </summary>
        public List<DatabaseTable> Tables => _tables;

        /// <summary>
        /// Gets the views.
        /// </summary>
        public List<DatabaseView> Views => _views;

        /// <summary>
        /// Gets the users.
        /// </summary>
        public List<DatabaseUser> Users => _users;

        /// <summary>
        /// Gets the functions.
        /// </summary>
        public List<DatabaseFunction> Functions => _functions;

        /// <summary>
        /// Gets the sequences.
        /// </summary>
        public List<DatabaseSequence> Sequences => _sequences;

        /// <summary>
        /// Gets or sets the provider.
        /// </summary>
        /// <value>
        /// The provider.
        /// </value>
        public string Provider { get; set; }
        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        /// <value>
        /// The connection string.
        /// </value>
        public string ConnectionString { get; set; }
        /// <summary>
        /// Gets or sets the owner.
        /// </summary>
        /// <value>
        /// The owner.
        /// </value>
        public string Owner { get; set; }

        /// <summary>
        /// Finds a table by name
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public DatabaseTable FindTableByName(string name)
        {
            return Tables.Find(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Finds a table by name and schema
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="schema">The schema.</param>
        /// <returns></returns>
        public DatabaseTable FindTableByName(string name, string schema)
        {
            return Tables.Find(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(t.SchemaOwner, schema, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "Tables: {0}, Views: {1}, StoredProcedures: {2}", Tables.Count, Views.Count, StoredProcedures.Count);
        }
    }
}
