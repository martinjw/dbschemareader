﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using DatabaseSchemaReader.Conversion;
using DatabaseSchemaReader.Conversion.Loaders;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.Filters;
using DatabaseSchemaReader.ProviderSchemaReaders;

namespace DatabaseSchemaReader
{
    /// <summary>
    /// Uses <see cref="DatabaseSchemaReader.SchemaReader"/> to read database schema into schema objects (rather than DataTables). 
    /// </summary>
    /// <remarks>
    /// Either load independent objects (list of Tables, StoredProcedures), fuller information (a Table with all Columns, constraints...), or full database schemas (<see cref="ReadAll"/>: all tables, views, stored procedures with all information; the DatabaseSchema object will hook up the relationships). Obviously the fuller versions will be slow on moderate to large databases.
    /// </remarks>
    public class DatabaseReader : IDatabaseReader
    {
        private readonly Exclusions _exclusions = new Exclusions();
        private readonly SchemaExtendedReader _schemaReader;
        private readonly DatabaseSchema _db;
        private bool _fixUp = true;

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
            _schemaReader = SchemaReaderFactory.Create(connectionString, sqlType);
            _db = new DatabaseSchema(connectionString, _schemaReader.ProviderName);
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
            _schemaReader = SchemaReaderFactory.Create(databaseSchema.ConnectionString, databaseSchema.Provider);
            _schemaReader.Owner = databaseSchema.Owner;
            _db = databaseSchema;
        }

        /// <summary>
        /// Exclude specified items when reading schema
        /// </summary>
        /// <value>
        /// The exclusions.
        /// </value>
        public Exclusions Exclusions { get { return _exclusions; } }

        /// <summary>
        /// Gets or sets the owner user. Always set it with Oracle (otherwise you'll get SYS, MDSYS etc...)
        /// </summary>
        /// <value>The user.</value>
        public string Owner
        {
            get { return _schemaReader.Owner; }
            set
            {
                _schemaReader.Owner = value;
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

            AllStoredProcedures();
            //oracle extra
            DatabaseSchema.Sequences.Clear();
            var sequences = _schemaReader.Sequences();
            DatabaseSchema.Sequences.AddRange(SchemaProcedureConverter.Sequences(sequences));

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
            DataTable dt = _schemaReader.Users();
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
            DataTable dt = _schemaReader.Tables();
            return SchemaConverter.Tables(dt);
        }

        /// <summary>
        /// Gets all tables (plus constraints, indexes and triggers).
        /// </summary>
        public IList<DatabaseTable> AllTables()
        {
            DataTable tabs = _schemaReader.Tables();
            //get full datatables for all tables, to minimize database calls

            //we either use the converters directly (DataTable to our db model)
            //or loaders, which wrap the schema loader calls and converters 
            //loaders hide the switch between calling for all tables, or a specific table
            var columnLoader = new ColumnLoader(_schemaReader);
            var constraintLoader = new SchemaConstraintLoader(_schemaReader);
            var indexLoader = new IndexLoader(_schemaReader);

            DataTable ids = _schemaReader.IdentityColumns(null);
            DataTable computeds = _schemaReader.ComputedColumns(null);

            var tableDescriptions = new TableDescriptionConverter(_schemaReader.TableDescription(null));
            var columnDescriptions = new ColumnDescriptionConverter(_schemaReader.ColumnDescription(null));

            DataTable triggers = _schemaReader.Triggers(null);
            var triggerConverter = new TriggerConverter(triggers);

            var tables = SchemaConverter.Tables(tabs);
            var tableFilter = Exclusions.TableFilter;
            if (tableFilter != null)
            {
                tables.RemoveAll(t => tableFilter.Exclude(t.Name));
            }
            tables.Sort(delegate(DatabaseTable t1, DatabaseTable t2)
            {
                //doesn't account for mixed schemas
                return string.Compare(t1.Name, t2.Name, StringComparison.OrdinalIgnoreCase);
            });


            foreach (DatabaseTable table in tables)
            {
                var tableName = table.Name;
                var schemaName = table.SchemaOwner;
                table.Description = tableDescriptions.FindDescription(table.SchemaOwner, tableName);

                var databaseColumns = columnLoader.Load(tableName, schemaName);
                table.Columns.AddRange(databaseColumns);

                columnDescriptions.AddDescriptions(table);

                var pkConstraints = constraintLoader.Load(tableName, schemaName, ConstraintType.PrimaryKey);
                PrimaryKeyLogic.AddPrimaryKey(table, pkConstraints);

                var fks = constraintLoader.Load(tableName, schemaName, ConstraintType.ForeignKey);
                table.AddConstraints(fks);

                table.AddConstraints(constraintLoader.Load(tableName, schemaName, ConstraintType.UniqueKey));
                table.AddConstraints(constraintLoader.Load(tableName, schemaName, ConstraintType.Check));
                table.AddConstraints(constraintLoader.Load(tableName, schemaName, ConstraintType.Default));

                indexLoader.AddIndexes(table);

                SchemaConstraintConverter.AddIdentity(ids, table);
                SchemaConstraintConverter.AddComputed(computeds, table);

                table.Triggers.Clear();
                table.Triggers.AddRange(triggerConverter.Triggers(tableName));
                _schemaReader.PostProcessing(table);
            }
            DatabaseSchema.Tables.Clear();
            DatabaseSchema.Tables.AddRange(tables);
            UpdateReferences();

            if (DatabaseSchema.DataTypes.Count > 0)
                DatabaseSchemaFixer.UpdateDataTypes(DatabaseSchema);

            _schemaReader.PostProcessing(DatabaseSchema);

            return tables;
        }

        /// <summary>
        /// Gets all views.
        /// </summary>
        public IList<DatabaseView> AllViews()
        {
            DataTable dt = _schemaReader.Views();
            List<DatabaseView> views = SchemaConverter.Views(dt);
            var viewFilter = Exclusions.ViewFilter;
            if (viewFilter != null)
            {
                views.RemoveAll(v => viewFilter.Exclude(v.Name));
            }
            //get full datatables for all tables, to minimize database calls
            var columnLoader = new ViewColumnLoader(_schemaReader);
            foreach (DatabaseView v in views)
            {
                v.Columns.AddRange(columnLoader.Load(v.Name, v.SchemaOwner));
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

            var schemaOwner = _schemaReader.Owner;
            DatabaseTable table;
            using (DataSet ds = _schemaReader.Table(tableName))
            {
                if (ds == null) return null;
                if (ds.Tables.Count == 0) return null;

                table = DatabaseSchema.FindTableByName(tableName, schemaOwner);
                if (table == null)
                {
                    table = new DatabaseTable();
                    DatabaseSchema.Tables.Add(table);
                }
                table.Name = tableName;
                table.SchemaOwner = schemaOwner;
                //columns must be done first as it is updated by the others
                table.Columns.Clear();
                var columnConverter = new ColumnConverter(ds.Tables[_schemaReader.ColumnsCollectionName]);
                var databaseColumns = columnConverter.Columns(tableName, schemaOwner).ToList();
                if (!databaseColumns.Any())
                {
                    //need to define the schema
                    databaseColumns = columnConverter.Columns().ToList();
                    var first = databaseColumns.FirstOrDefault();
                    if (first != null)
                    {
                        //take the schema of the first we find
                        table.SchemaOwner = schemaOwner = first.SchemaOwner;
                    }
                    databaseColumns = columnConverter.Columns(tableName, schemaOwner).ToList();
                }
                table.Columns.AddRange(databaseColumns);
                if (ds.Tables.Contains(_schemaReader.PrimaryKeysCollectionName))
                {
                    var converter = new SchemaConstraintConverter(ds.Tables[_schemaReader.PrimaryKeysCollectionName], ConstraintType.PrimaryKey);
                    var pkConstraints = converter.Constraints();
                    PrimaryKeyLogic.AddPrimaryKey(table, pkConstraints);
                }
                if (ds.Tables.Contains(_schemaReader.ForeignKeysCollectionName))
                {
                    var converter = new SchemaConstraintConverter(ds.Tables[_schemaReader.ForeignKeysCollectionName], ConstraintType.ForeignKey);
                    table.AddConstraints(converter.Constraints());
                }
                if (ds.Tables.Contains(_schemaReader.ForeignKeyColumnsCollectionName))
                {
                    var fkConverter = new ForeignKeyColumnConverter(ds.Tables[_schemaReader.ForeignKeyColumnsCollectionName]);
                    fkConverter.AddForeignKeyColumns(table.ForeignKeys);
                }

                if (ds.Tables.Contains(_schemaReader.UniqueKeysCollectionName))
                {
                    var converter = new SchemaConstraintConverter(ds.Tables[_schemaReader.UniqueKeysCollectionName], ConstraintType.UniqueKey);
                    table.AddConstraints(converter.Constraints());
                }
                if (ds.Tables.Contains(_schemaReader.ComputedColumnsCollectionName))
                {
                    SchemaConstraintConverter.AddComputed(ds.Tables[_schemaReader.ComputedColumnsCollectionName], table);
                }

                var indexConverter = new IndexConverter(ds.Tables[_schemaReader.IndexColumnsCollectionName], null);
                table.Indexes.AddRange(indexConverter.Indexes(tableName, schemaOwner));

                if (ds.Tables.Contains(_schemaReader.IdentityColumnsCollectionName))
                    SchemaConstraintConverter.AddIdentity(ds.Tables[_schemaReader.IdentityColumnsCollectionName], table);

                _schemaReader.PostProcessing(table);

            }

            if (DatabaseSchema.DataTypes.Count > 0)
                DatabaseSchemaFixer.UpdateDataTypes(DatabaseSchema);
            _schemaReader.PostProcessing(DatabaseSchema);

            return table;
        }

        /// <summary>
        /// Gets all stored procedures (no arguments, for Oracle no packages)
        /// </summary>
        public IList<DatabaseStoredProcedure> StoredProcedureList()
        {
            DataTable dt = _schemaReader.StoredProcedures();
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
            try
            {
                DataTable functions = _schemaReader.Functions();
                DatabaseSchema.Functions.Clear();
                DatabaseSchema.Functions.AddRange(SchemaProcedureConverter.Functions(functions));
            }
            catch (DbException ex)
            {
                Debug.WriteLine("Cannot read functions - database security may prevent access to DDL\n" + ex.Message);
                throw; //or suppress if not applicable
            }


            DataTable dt = _schemaReader.StoredProcedures();
            SchemaProcedureConverter.StoredProcedures(DatabaseSchema, dt);
            var procFilter = Exclusions.StoredProcedureFilter;
            if (procFilter != null)
            {
                DatabaseSchema.StoredProcedures.RemoveAll(p => procFilter.Exclude(p.Name));
            }

            DatabaseSchema.Packages.Clear();
            DatabaseSchema.Packages.AddRange(SchemaProcedureConverter.Packages(_schemaReader.Packages()));
            var packFilter = Exclusions.PackageFilter;
            if (packFilter != null)
            {
                DatabaseSchema.Packages.RemoveAll(p => packFilter.Exclude(p.Name));
            }
            //do all the arguments as one call and sort them out. 
            //NB: This is often slow on Oracle
            DataTable args = _schemaReader.StoredProcedureArguments(null);

            var converter = new SchemaProcedureConverter();
            converter.PackageFilter = Exclusions.PackageFilter;
            converter.StoredProcedureFilter = Exclusions.StoredProcedureFilter;
            if (args.Rows.Count == 0)
            {
                //MySql v6 won't do all stored procedures. So we have to do them individually.
                foreach (var sproc in DatabaseSchema.StoredProcedures)
                {
                    args = _schemaReader.StoredProcedureArguments(sproc.Name);
                    converter.UpdateArguments(DatabaseSchema, args);
                }

                foreach (var function in DatabaseSchema.Functions)
                {
                    args = _schemaReader.StoredProcedureArguments(function.Name);
                    converter.UpdateArguments(DatabaseSchema, args);
                }
            }
            //arguments could be for functions too
            converter.UpdateArguments(DatabaseSchema, args);
            foreach (var function in DatabaseSchema.Functions)
            {
                //return types are assigned as arguments (in most platforms). Move them to return type.
                function.CheckArgumentsForReturnType();
            }

            //procedure, function and view source sql
            DataTable srcs = _schemaReader.ProcedureSource(null);
            SchemaSourceConverter.AddSources(DatabaseSchema, srcs);

            UpdateReferences();

            return DatabaseSchema.StoredProcedures;
        }

        /// <summary>
        /// Gets all datatypes (and updates columns/arguments if already loaded)
        /// </summary>
        public IList<DataType> DataTypes()
        {
            List<DataType> list = SchemaConverter.DataTypes(_schemaReader.DataTypes());
            if (list.Count == 0) list = _schemaReader.SchemaDataTypes();
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

            //last, do custom post processing if implemented
            _schemaReader.PostProcessing(DatabaseSchema);
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
                // free managed resources
                if (_schemaReader != null)
                {
                    _schemaReader.Dispose();
                }
            }
        }
        #endregion
    }
}