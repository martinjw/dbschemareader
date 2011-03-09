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
    /// Use <see cref="DatabaseSchemaFixer.UpdateReferences"/> to link things up
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

        public List<DataType> DataTypes { get { return _dataTypes; } }

        public List<DatabaseStoredProcedure> StoredProcedures { get { return _storedProcedures; } }

        public List<DatabasePackage> Packages { get { return _packages; } }

        public List<DatabaseTable> Tables { get { return _tables; } }

        public List<DatabaseView> Views { get { return _views; } }

        public List<DatabaseUser> Users { get { return _users; } }

        public List<DatabaseFunction> Functions { get { return _functions; } }

        public List<DatabaseSequence> Sequences { get { return _sequences; } }

        public string Provider { get; set; }
        public string ConnectionString { get; set; }
        public string Owner { get; set; }

        public DatabaseTable FindTableByName(string name)
        {
            return Tables.Find(delegate(DatabaseTable t2) { return t2.Name.Equals(name, StringComparison.OrdinalIgnoreCase); });
        }


        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "Tables: {0}, Views: {1}, StoredProcedures: {2}", Tables.Count, Views.Count, StoredProcedures.Count);
        }
    }
}
