using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using DatabaseSchemaReader.Conversion;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader
{
    /// <summary>
    /// Uses <see cref="SchemaReader"/> to read database schema into schema objects (rather than DataTables). 
    /// </summary>
    /// <remarks>
    /// Either load independent objects (list of Tables, StoredProcedures), fuller informarion (a Table with all Columns, constraints...), or full database schemas (<see cref="ReadAll"/>: all tables, views, stored procedures with all information; the DatabaseSchema object will hook up the relationships). Obviously the fuller versions will be slow on moderate to large databases.
    /// </remarks>
    public class DatabaseReader : IDisposable
    {
        private readonly SchemaExtendedReader _sr;
        private readonly DatabaseSchema _db;
        private bool _fixUp = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseReader"/> class. For Oracle, use the overload.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="providerName">Name of the provider.</param>
        public DatabaseReader(string connectionString, string providerName)
        {
            _sr = new SchemaExtendedReader(connectionString, providerName);
            _db = new DatabaseSchema(connectionString, providerName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseReader"/> class. For Oracle, use this overload.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="providerName">Name of the provider.</param>
        /// <param name="owner">The schema owner.</param>
        public DatabaseReader(string connectionString, string providerName, string owner)
            : this(connectionString, providerName)
        {
            _sr.Owner = owner;
            _db.Owner = owner;
        }

        /// <summary>
        /// Gets or sets the owner user. Always set it with Oracle (otherwise you'll get SYS, MDSYS etc...)
        /// </summary>
        /// <value>The user.</value>
        public string Owner
        {
            get { return _sr.Owner; }
            set
            {
                _sr.Owner = value;
                _db.Owner = value;
            }
        }

        /// <summary>
        /// Gets the database schema. Only call AFTER calling <see cref="ReadAll"/> or one or more other methods such as <see cref="AllTables"/>. A collection of Tables, Views and StoredProcedures. Use <see cref="DataSchema.DatabaseSchemaFixer.UpdateReferences"/> to update object references after loaded. Use <see cref="DataSchema.DatabaseSchemaFixer.UpdateDataTypes"/> to add datatypes from DbDataType string after loaded.
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
            _fixUp = false;
            DataTypes();
            AllUsers();
            AllTables();
            AllViews();

            try
            {
                DataTable functions = _sr.Functions();
                DatabaseSchema.Functions.AddRange(SchemaProcedureConverter.Functions(functions));
            }
            catch (DbException ex)
            {
                Debug.WriteLine("Cannot read functions - database security may prevent access to DDL\n" + ex.Message);
                throw; //or suppress if not applicable
            }

            AllStoredProcedures();
            //oracle extra
            DatabaseSchema.Sequences.Clear();
            DatabaseSchema.Sequences.AddRange(SchemaProcedureConverter.Sequences(_sr.Sequences()));

            _fixUp = true;
            UpdateReferences();

            return _db;
        }

        /// <summary>
        /// Gets the users (specifically for Oracle)
        /// </summary>
        public IList<DatabaseUser> AllUsers()
        {
            var list = new List<DatabaseUser>();
            DataTable dt = _sr.Users();
            //sql
            string key = "user_name";
            //oracle
            if (!dt.Columns.Contains(key)) key = "name";
            //mysql
            if (!dt.Columns.Contains(key)) key = "username";
            foreach (DataRow row in dt.Rows)
            {
                var u = new DatabaseUser();
                u.Name = row[key].ToString();
                list.Add(u);
            }
            DatabaseSchema.Users.Clear();
            DatabaseSchema.Users.AddRange(list);
            return list;
        }

        /// <summary>
        /// Gets all tables (just names, no columns).
        /// </summary>
        public IList<DatabaseTable> TableList()
        {
            DataTable dt = _sr.Tables();
            return SchemaConverter.Tables(dt);
        }

        /// <summary>
        /// Gets all tables (plus constraints, indexes and triggers).
        /// </summary>
        public IList<DatabaseTable> AllTables()
        {
            DataTable dt = _sr.Tables();
            //get full datatables for all tables, to minimize database calls
            DataTable cols = _sr.Columns(null); //might want to cache this for views
            DataTable pks = _sr.PrimaryKeys(null);
            DataTable fks = _sr.ForeignKeys(null);
            DataTable fkcols = _sr.ForeignKeyColumns(null);
            DataTable uks = _sr.UniqueKeys(null);
            DataTable ids = _sr.IdentityColumns(null);
            DataTable cks = _sr.CheckConstraints(null);
            DataTable indexes = _sr.IndexColumns(null);
            DataTable triggers = _sr.Triggers(null);
            List<DatabaseTable> tables = SchemaConverter.Tables(dt);
            tables.Sort(delegate(DatabaseTable t1, DatabaseTable t2)
            {
                //doesn't account for mixed schemas
                return string.Compare(t1.Name, t2.Name, StringComparison.OrdinalIgnoreCase);
            });
            foreach (DatabaseTable table in tables)
            {
                table.Columns.AddRange(SchemaConverter.Columns(cols, table.Name));
                List<DatabaseConstraint> pkConstraints = SchemaConstraintConverter.Constraints(pks, ConstraintType.PrimaryKey, table.Name);
                if (pkConstraints.Count > 0) table.PrimaryKey = pkConstraints[0];
                table.ForeignKeys = SchemaConstraintConverter.Constraints(fks, ConstraintType.ForeignKey, table.Name);
                SchemaConstraintConverter.AddForeignKeyColumns(fkcols, table);
                table.UniqueKeys = SchemaConstraintConverter.Constraints(uks, ConstraintType.UniqueKey, table.Name);
                table.CheckConstraints = SchemaConstraintConverter.Constraints(cks, ConstraintType.Check, table.Name);
                table.Indexes = SchemaConstraintConverter.Indexes(indexes, table.Name);
                SchemaConstraintConverter.AddIdentity(ids, table);
                table.Triggers.Clear();
                table.Triggers.AddRange(SchemaConstraintConverter.Triggers(triggers, table.Name));
            }
            DatabaseSchema.Tables.Clear();
            DatabaseSchema.Tables.AddRange(tables);
            UpdateReferences();

            if (DatabaseSchema.DataTypes.Count > 0)
                DatabaseSchemaFixer.UpdateDataTypes(DatabaseSchema);

            return tables;
        }

        /// <summary>
        /// Gets all views.
        /// </summary>
        public IList<DatabaseView> AllViews()
        {
            DataTable dt = _sr.Views();
            List<DatabaseView> views = SchemaConverter.Views(dt);
            //get full datatables for all tables, to minimize database calls
            DataTable cols = _sr.Columns(null);
            foreach (DatabaseView v in views)
            {
                v.Columns.AddRange(SchemaConverter.Columns(cols, v.Name));
            }
            DatabaseSchema.Views.Clear();
            DatabaseSchema.Views.AddRange(views);
            return views;
        }

        /// <summary>
        /// Gets the table. If <see cref="Owner"/> is specified, it is used.
        /// </summary>
        /// <param name="tableName">Name of the table. Oracle names can be case sensitive.</param>
        public DatabaseTable Table(string tableName)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException("tableName");

            DataSet ds = _sr.Table(tableName);
            if (ds == null) return null;
            DatabaseTable table = DatabaseSchema.Tables.Find(delegate(DatabaseTable x) { return x.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase); });
            if (table == null)
            {
                table = new DatabaseTable();
                DatabaseSchema.Tables.Add(table);
            }
            table.Name = tableName;
            table.SchemaOwner = _sr.Owner;
            //columns must be done first as it is updated by the others
            table.Columns.Clear();
            table.Columns.AddRange(SchemaConverter.Columns(ds.Tables["Columns"]));
            if (ds.Tables.Contains("Primary_Keys"))
            {
                List<DatabaseConstraint> pkConstraints = SchemaConstraintConverter.Constraints(ds.Tables["Primary_Keys"], ConstraintType.PrimaryKey);
                if (pkConstraints.Count > 0) table.PrimaryKey = pkConstraints[0];
            }
            if (ds.Tables.Contains("Foreign_Keys"))
                table.ForeignKeys = SchemaConstraintConverter.Constraints(ds.Tables["Foreign_Keys"], ConstraintType.ForeignKey);
            if (ds.Tables.Contains("ForeignKeyColumns"))
                SchemaConstraintConverter.AddForeignKeyColumns(ds.Tables["ForeignKeyColumns"], table);

            if (ds.Tables.Contains("Unique_Keys"))
                table.UniqueKeys = SchemaConstraintConverter.Constraints(ds.Tables["Unique_Keys"], ConstraintType.UniqueKey);
            if (ds.Tables.Contains("IndexColumns"))
                table.Indexes = SchemaConstraintConverter.Indexes(ds.Tables["IndexColumns"]);
            if (ds.Tables.Contains("IdentityColumns"))
                SchemaConstraintConverter.AddIdentity(ds.Tables["IdentityColumns"], table);

            if (DatabaseSchema.DataTypes.Count > 0)
                DatabaseSchemaFixer.UpdateDataTypes(DatabaseSchema);

            return table;
        }

        /// <summary>
        /// Gets all stored procedures (no arguments, for Oracle no packages)
        /// </summary>
        public IList<DatabaseStoredProcedure> StoredProcedureList()
        {
            DataTable dt = _sr.StoredProcedures();
            SchemaProcedureConverter.StoredProcedures(DatabaseSchema, dt);
            return DatabaseSchema.StoredProcedures;
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

            DataTable dt = _sr.StoredProcedures();
            SchemaProcedureConverter.StoredProcedures(DatabaseSchema, dt);

            DatabaseSchema.Packages.Clear();
            DatabaseSchema.Packages.AddRange(SchemaProcedureConverter.Packages(_sr.Packages()));
            //do all the arguments as one call and sort them out. 
            //NB: This is often slow on Oracle
            DataTable args = _sr.StoredProcedureArguments(null);
            //arguments could be for functions too
            SchemaProcedureConverter.UpdateArguments(DatabaseSchema, args);

            //procedure, function and view source sql
            DataTable srcs = _sr.ProcedureSource(null);
            SchemaSourceConverter.AddSources(DatabaseSchema, srcs);

            UpdateReferences();

            return DatabaseSchema.StoredProcedures;
        }

        /// <summary>
        /// Gets all datatypes (and updates columns/arguments if already loaded)
        /// </summary>
        public IList<DataType> DataTypes()
        {
            List<DataType> list = SchemaConverter.DataTypes(_sr.DataTypes());
            DatabaseSchema.DataTypes.Clear();
            DatabaseSchema.DataTypes.AddRange(list);
            DatabaseSchemaFixer.UpdateDataTypes(DatabaseSchema); //if columns/arguments loaded later, run this method again.
            return list;
        }

        private void UpdateReferences()
        {
            //a simple latch so ReadAll will only call this at the end
            if (_fixUp)
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
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                if (_sr != null)
                {
                    _sr.Dispose();
                }
            }
        }
        #endregion
    }
}
