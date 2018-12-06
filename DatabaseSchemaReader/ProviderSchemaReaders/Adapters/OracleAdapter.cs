using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.Databases.Oracle;
using DatabaseSchemaReader.ProviderSchemaReaders.ResultModels;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Adapters
{
    class OracleAdapter : ReaderAdapter
    {
        public OracleAdapter(SchemaParameters schemaParameters) : base(schemaParameters)
        {
        }

        public override IList<DataType> DataTypes()
        {
            return new DataTypeList().Execute();
        }

        public override IList<DatabaseTable> Tables(string tableName)
        {
            return new Tables(CommandTimeout, Owner, tableName)
                .Execute(ConnectionAdapter);
        }

        public override IList<DatabaseColumn> Columns(string tableName)
        {
            return new Columns(CommandTimeout, Owner, tableName)
                .Execute(ConnectionAdapter);
        }

        public override IList<DatabaseConstraint> PrimaryKeys(string tableName)
        {
            return new Constraints(CommandTimeout, Owner, tableName, ConstraintType.PrimaryKey)
                .Execute(ConnectionAdapter);
        }

        public override IList<DatabaseConstraint> UniqueKeys(string tableName)
        {
            return new Constraints(CommandTimeout, Owner, tableName, ConstraintType.UniqueKey)
                .Execute(ConnectionAdapter);
        }

        public override IList<DatabaseConstraint> ForeignKeys(string tableName)
        {
            return new Constraints(CommandTimeout, Owner, tableName, ConstraintType.ForeignKey)
                .Execute(ConnectionAdapter);
        }

        public override IList<DatabaseColumn> ComputedColumns(string tableName)
        {
            return new ComputedColumns(CommandTimeout, Owner, tableName)
                .Execute(ConnectionAdapter);
        }

        public override IList<DatabaseColumn> IdentityColumns(string tableName)
        {
            return new IdentityColumns(CommandTimeout, Owner, tableName)
                .Execute(ConnectionAdapter);
        }

        public override IList<DatabaseIndex> Indexes(string tableName)
        {
            return new Indexes(CommandTimeout, Owner, tableName)
                .Execute(ConnectionAdapter);
        }

        public override IList<DatabaseTrigger> Triggers(string tableName)
        {
            return new Triggers(CommandTimeout, Owner, tableName)
                .Execute(ConnectionAdapter);
        }

        public override IList<DatabaseTable> ColumnDescriptions(string tableName)
        {
            return new ColumnDescriptions(CommandTimeout, Owner, tableName)
                .Execute(ConnectionAdapter);
        }

        public override IList<DatabaseTable> TableDescriptions(string tableName)
        {
            return new TableDescriptions(CommandTimeout, Owner, tableName)
                .Execute(ConnectionAdapter);
        }

        public override IList<DatabaseConstraint> CheckConstraints(string tableName)
        {
            return new CheckConstraints(CommandTimeout, Owner, tableName)
                .Execute(ConnectionAdapter);
        }

        public override IList<DatabaseView> Views(string viewName)
        {
            var views = new Views(CommandTimeout, Owner, viewName)
                .Execute(ConnectionAdapter);
            if (string.IsNullOrEmpty(viewName) || !views.Any())
            {
                var mviews = new MaterializedViews(CommandTimeout, Owner, viewName)
                    .Execute(ConnectionAdapter);
                foreach (var mview in mviews)
                {
                    views.Add(mview);
                }
            }
            return views;
        }

        public override IList<DatabaseColumn> ViewColumns(string viewName)
        {
            var columns = new ViewColumns(CommandTimeout, Owner, viewName)
                .Execute(ConnectionAdapter);
            if (string.IsNullOrEmpty(viewName) || !columns.Any())
            {
                var mCols = new MaterializedViewColumns(CommandTimeout, Owner, viewName)
                    .Execute(ConnectionAdapter);
                foreach (var mcol in mCols)
                {
                    columns.Add(mcol);
                }
            }
            return columns;
        }

        public override IList<DatabaseIndex> ViewIndexes(string tableName)
        {
            return new ViewIndexes(CommandTimeout, Owner, tableName)
                .Execute(ConnectionAdapter);
        }

        public override IList<DatabasePackage> Packages(string name)
        {
            return new Packages(CommandTimeout, Owner, name)
                .Execute(ConnectionAdapter);
        }

        public override IList<DatabaseStoredProcedure> StoredProcedures(string name)
        {
            return new StoredProcedures(CommandTimeout, Owner, name)
                .Execute(ConnectionAdapter);
        }

        public override IList<DatabaseFunction> Functions(string name)
        {
            return new Functions(CommandTimeout, Owner)
                .Execute(ConnectionAdapter);
        }

        public override IList<DatabaseArgument> ProcedureArguments(string name)
        {
            return new ProcedureArguments(CommandTimeout, Owner, name)
                .Execute(ConnectionAdapter);
        }

        public override IList<ProcedureSource> ProcedureSources(string name)
        {
            return new ProcedureSources(CommandTimeout, Owner, name)
                .Execute(ConnectionAdapter);
        }

        public override IList<DatabaseSequence> Sequences(string name)
        {
            return new Sequences(CommandTimeout, Owner).Execute(ConnectionAdapter);
        }

        public override IList<DatabaseUser> Users()
        {
            return new Users(CommandTimeout).Execute(ConnectionAdapter);
        }

        public override IList<DatabaseDbSchema> Schemas()
        {
            return new Schemas(CommandTimeout).Execute(ConnectionAdapter);
        }

        public override void PostProcessing(DatabaseTable databaseTable)
        {
            if (databaseTable == null) return;
            //look at Oracle triggers which suggest the primary key id is autogenerated (in SQLServer terms, Identity)
            LookForAutoGeneratedId(databaseTable);
        }

        private static void LookForAutoGeneratedId(DatabaseTable databaseTable)
        {
            var pk = databaseTable.PrimaryKeyColumn;
            if (pk == null) return;
            if (Databases.Oracle.Conversion.LooksLikeAutoNumberDefaults(pk.DefaultValue))
            {
                //Oracle 12c default values from sequence
                pk.IsAutoNumber = true;
                return;
            }
            var match = OracleSequenceTrigger.FindTrigger(databaseTable);
            if (match != null) pk.IsAutoNumber = true;
        }
    }
}