#if !COREFX
using System.Collections.Generic;
using System.Linq;
using DatabaseSchemaReader.Conversion;
using DatabaseSchemaReader.Conversion.Loaders;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ResultModels;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Adapters
{
    class DbProviderReaderAdapter : ReaderAdapter
    {
        private readonly SchemaExtendedReader _schemaReader;

        public DbProviderReaderAdapter(SchemaParameters schemaParameters)
            : base(schemaParameters)
        {
            _schemaReader = SchemaReaderFactory.Create(schemaParameters.ConnectionString, schemaParameters.ProviderName);
            _schemaReader.Owner = schemaParameters.Owner;
            _schemaReader.CommandTimeout = CommandTimeout;
        }

        public override string Owner
        {
            get { return base.Owner; }
            set
            {
                base.Owner = value;
                _schemaReader.Owner = value;
            }
        }

        public override IList<DatabaseSequence> Sequences(string name)
        {
            var sequences = _schemaReader.Sequences();
            return SchemaProcedureConverter.Sequences(sequences);
        }

        public override IList<DatabaseUser> Users()
        {
            var dt = _schemaReader.Users();
            var list = UserConverter.Convert(dt);
            return list;
        }

        public override IList<DatabaseTable> Tables(string tableName)
        {
            var dt = _schemaReader.Tables(tableName);
            return SchemaConverter.Tables(dt);
        }

        public override IList<DatabaseColumn> Columns(string tableName)
        {
            var dt = _schemaReader.Columns(tableName);
            var columnConverter = new ColumnConverter(dt);
            var cols = columnConverter.Columns().ToList();
            return cols;
        }

        public override IList<DatabaseStoredProcedure> StoredProcedures(string name)
        {
            var dt = _schemaReader.StoredProcedures();
            SchemaProcedureConverter.StoredProcedures(Parameters.DatabaseSchema, dt);
            if (Parameters.SqlType == SqlType.Oracle && !string.IsNullOrEmpty(Owner))
            {
                Parameters.DatabaseSchema.StoredProcedures.RemoveAll(x => x.SchemaOwner != Owner);
            }
            return Parameters.DatabaseSchema.StoredProcedures;
        }

        public override IList<DataType> DataTypes()
        {
            var list = SchemaConverter.DataTypes(_schemaReader.DataTypes());
            if (list.Count == 0) list = _schemaReader.SchemaDataTypes();
            return list;
        }

        public override IList<DatabaseView> Views(string viewName)
        {
            var dt = _schemaReader.Views();
            var views = SchemaConverter.Views(dt);
            return views;
        }

        public override IList<DatabaseColumn> ViewColumns(string viewName)
        {
            var columnLoader = new ViewColumnLoader(_schemaReader);
            if (string.IsNullOrEmpty(viewName))
            {
                return columnLoader.Load();
            }
            return columnLoader.Load(viewName, Owner).ToList();
        }

        public override IList<DatabaseArgument> ProcedureArguments(string name)
        {
            var dt = _schemaReader.StoredProcedureArguments(name);
            var converter = new SchemaProcedureConverter
            {
                PackageFilter = Parameters.Exclusions.PackageFilter,
                StoredProcedureFilter = Parameters.Exclusions.StoredProcedureFilter
            };
            return converter.Arguments(dt);
        }

        public override IList<DatabaseConstraint> CheckConstraints(string tableName)
        {
            var dt = _schemaReader.CheckConstraints(tableName);
            var converter = new SchemaConstraintConverter(dt, ConstraintType.Check);
            return converter.Constraints();
        }

        public override IList<DatabaseConstraint> DefaultConstraints(string tableName)
        {
            var dt = _schemaReader.DefaultConstraints(tableName);
            var converter = new SchemaConstraintConverter(dt, ConstraintType.Default);
            return converter.Constraints();
        }

        public override IList<DatabaseConstraint> PrimaryKeys(string tableName)
        {
            var dt = _schemaReader.PrimaryKeys(tableName);
            var converter = new SchemaConstraintConverter(dt, ConstraintType.PrimaryKey);
            return converter.Constraints();
        }

        public override IList<DatabaseConstraint> ForeignKeys(string tableName)
        {
            var dt = _schemaReader.ForeignKeys(tableName);
            var fkcols = _schemaReader.ForeignKeyColumns(tableName);
            var converter = new SchemaConstraintConverter(dt, ConstraintType.ForeignKey);
            var fks = converter.Constraints();
            var fkColumnConverter = new ForeignKeyColumnConverter(fkcols);
            fkColumnConverter.AddForeignKeyColumns(fks);
            return fks;
        }

        public override IList<DatabaseConstraint> UniqueKeys(string tableName)
        {
            var dt = _schemaReader.UniqueKeys(tableName);
            var converter = new SchemaConstraintConverter(dt, ConstraintType.UniqueKey);
            return converter.Constraints();
        }


        public override IList<DatabaseColumn> IdentityColumns(string tableName)
        {
            var dt = _schemaReader.IdentityColumns(tableName);
            return SchemaConstraintConverter.ConvertIdentity(dt);
        }

        public override IList<DatabaseColumn> ComputedColumns(string tableName)
        {
            var dt = _schemaReader.ComputedColumns(tableName);
            return SchemaConstraintConverter.ConvertComputed(dt);
        }

        public override IList<DatabaseIndex> Indexes(string tableName)
        {
            var converter = new IndexConverter(_schemaReader.Indexes(tableName), _schemaReader.IndexColumns(tableName));
            var indices = converter.Indexes();
            return indices;
        }

        public override IList<DatabaseTable> ColumnDescriptions(string tableName)
        {
            var columnDescriptions = new ColumnDescriptionConverter(_schemaReader.ColumnDescription(tableName));
            return columnDescriptions.Result();
        }

        public override IList<DatabaseTable> TableDescriptions(string tableName)
        {
            var tableDescriptions = new TableDescriptionConverter(_schemaReader.TableDescription(tableName));
            return tableDescriptions.Result();
        }

        public override IList<DatabasePackage> Packages(string name)
        {
            var packages = SchemaProcedureConverter.Packages(_schemaReader.Packages());
            if (Parameters.SqlType == SqlType.Oracle && !string.IsNullOrEmpty(Owner))
            {
                packages.RemoveAll(x => x.SchemaOwner != Owner);
            }
            return packages;
        }

        public override IList<DatabaseTrigger> Triggers(string tableName)
        {
            var dt = _schemaReader.Triggers(tableName);
            var triggerConverter = new TriggerConverter(dt);
            return triggerConverter.Result().ToList();
        }

        public override IList<ProcedureSource> ProcedureSources(string name)
        {
            var dt = _schemaReader.ProcedureSource(name);
            return SchemaSourceConverter.AddSources(dt);
        }

        public override void PostProcessing(DatabaseTable databaseTable)
        {
            _schemaReader.PostProcessing(databaseTable);
        }

        public override IList<DatabaseFunction> Functions(string name)
        {
            var dt = _schemaReader.Functions();
            return SchemaProcedureConverter.Functions(dt);
        }

        public override IList<DatabaseDbSchema> Schemas()
        {
            return _schemaReader.Schemas();
        }
    }
}
#endif