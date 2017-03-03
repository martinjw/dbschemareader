using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.Databases.Oracle;
using System;
using System.Collections.Generic;
using System.Linq;
using DatabaseSchemaReader.ProviderSchemaReaders.ResultModels;

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
            return new Tables(Owner, tableName)
                .Execute(DbConnection, DbTransaction);
        }

        public override IList<DatabaseColumn> Columns(string tableName)
        {
            return new Columns(Owner, tableName)
                .Execute(DbConnection, DbTransaction);
        }

        public override IList<DatabaseConstraint> PrimaryKeys(string tableName)
        {
            return new Constraints(Owner, tableName, ConstraintType.PrimaryKey)
                .Execute(DbConnection, DbTransaction);
        }

        public override IList<DatabaseConstraint> UniqueKeys(string tableName)
        {
            return new Constraints(Owner, tableName, ConstraintType.UniqueKey)
                .Execute(DbConnection, DbTransaction);
        }

        public override IList<DatabaseConstraint> ForeignKeys(string tableName)
        {
            return new Constraints(Owner, tableName, ConstraintType.ForeignKey)
                .Execute(DbConnection, DbTransaction);
        }

        public override IList<DatabaseColumn> ComputedColumns(string tableName)
        {
            return new ComputedColumns(Owner, tableName)
                .Execute(DbConnection, DbTransaction);
        }

        public override IList<DatabaseColumn> IdentityColumns(string tableName)
        {
            return new IdentityColumns(Owner, tableName)
                .Execute(DbConnection, DbTransaction);
        }

        public override IList<DatabaseIndex> Indexes(string tableName)
        {
            return new Indexes(Owner, tableName)
                .Execute(DbConnection, DbTransaction);
        }

        public override IList<DatabaseTrigger> Triggers(string tableName)
        {
            return new Triggers(Owner, tableName)
                .Execute(DbConnection, DbTransaction);
        }

        public override IList<DatabaseTable> ColumnDescriptions(string tableName)
        {
            return new ColumnDescriptions(Owner, tableName)
                .Execute(DbConnection, DbTransaction);
        }

        public override IList<DatabaseTable> TableDescriptions(string tableName)
        {
            return new TableDescriptions(Owner, tableName)
                .Execute(DbConnection, DbTransaction);
        }

        public override IList<DatabaseConstraint> CheckConstraints(string tableName)
        {
            return new CheckConstraints(Owner, tableName)
                .Execute(DbConnection, DbTransaction);
        }

        public override IList<DatabaseView> Views(string viewName)
        {
            var views = new Views(Owner, viewName)
                .Execute(DbConnection, DbTransaction);
            if (string.IsNullOrEmpty(viewName) || !views.Any())
            {
                var mviews = new MaterializedViews(Owner, viewName)
                    .Execute(DbConnection, DbTransaction);
                foreach (var mview in mviews)
                {
                    views.Add(mview);
                }
            }
            return views;
        }

        public override IList<DatabaseColumn> ViewColumns(string viewName)
        {
            var columns = new ViewColumns(Owner, viewName)
                .Execute(DbConnection, DbTransaction);
            if (string.IsNullOrEmpty(viewName) || !columns.Any())
            {
                var mCols = new MaterializedViewColumns(Owner, viewName)
                    .Execute(DbConnection, DbTransaction);
                foreach (var mcol in mCols)
                {
                    columns.Add(mcol);
                }
            }
            return columns;
        }

        public override IList<DatabaseIndex> ViewIndexes(string tableName)
        {
            return new ViewIndexes(Owner, tableName)
                .Execute(DbConnection, DbTransaction);
        }

        public override IList<DatabasePackage> Packages(string name)
        {
            return new Packages(Owner, name)
                .Execute(DbConnection, DbTransaction);
        }

        public override IList<DatabaseStoredProcedure> StoredProcedures(string name)
        {
            return new StoredProcedures(Owner, name)
                .Execute(DbConnection, DbTransaction);
        }

        public override IList<DatabaseFunction> Functions(string name)
        {
            return new Functions(Owner)
                .Execute(DbConnection, DbTransaction);
        }

        public override IList<DatabaseArgument> ProcedureArguments(string name)
        {
            return new ProcedureArguments(Owner, name)
                .Execute(DbConnection, DbTransaction);
        }

        public override IList<ProcedureSource> ProcedureSources(string name)
        {
            return new ProcedureSources(Owner, name)
                .Execute(DbConnection, DbTransaction);
        }

        public override IList<DatabaseSequence> Sequences(string name)
        {
            return new Sequences(Owner).Execute(DbConnection, DbTransaction);
        }

        public override IList<DatabaseUser> Users()
        {
            return new Users().Execute(DbConnection, DbTransaction);
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