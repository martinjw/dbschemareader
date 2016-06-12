using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.Databases.Oracle;
using System;
using System.Collections.Generic;
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
            return new Tables(Parameters.Owner, tableName)
                .Execute(Parameters.DbConnection);
        }

        public override IList<DatabaseColumn> Columns(string tableName)
        {
            return new Columns(Parameters.Owner, tableName)
                .Execute(Parameters.DbConnection);
        }

        public override IList<DatabaseConstraint> PrimaryKeys(string tableName)
        {
            return new Constraints(Parameters.Owner, tableName, ConstraintType.PrimaryKey)
                .Execute(Parameters.DbConnection);
        }

        public override IList<DatabaseConstraint> UniqueKeys(string tableName)
        {
            return new Constraints(Parameters.Owner, tableName, ConstraintType.UniqueKey)
                .Execute(Parameters.DbConnection);
        }

        public override IList<DatabaseConstraint> ForeignKeys(string tableName)
        {
            return new Constraints(Parameters.Owner, tableName, ConstraintType.ForeignKey)
                .Execute(Parameters.DbConnection);
        }

        public override IList<DatabaseColumn> ComputedColumns(string tableName)
        {
            return new ComputedColumns(Parameters.Owner, tableName)
                .Execute(Parameters.DbConnection);
        }

        public override IList<DatabaseColumn> IdentityColumns(string tableName)
        {
            return new IdentityColumns(Parameters.Owner, tableName)
                .Execute(Parameters.DbConnection);
        }

        public override IList<DatabaseIndex> Indexes(string tableName)
        {
            return new Indexes(Parameters.Owner, tableName)
                .Execute(Parameters.DbConnection);
        }

        public override IList<DatabaseTrigger> Triggers(string tableName)
        {
            return new Triggers(Parameters.Owner, tableName)
                .Execute(Parameters.DbConnection);
        }

        public override IList<DatabaseTable> ColumnDescriptions(string tableName)
        {
            return new ColumnDescriptions(Parameters.Owner, tableName)
                .Execute(Parameters.DbConnection);
        }

        public override IList<DatabaseTable> TableDescriptions(string tableName)
        {
            return new TableDescriptions(Parameters.Owner, tableName)
                .Execute(Parameters.DbConnection);
        }

        public override IList<DatabaseConstraint> CheckConstraints(string tableName)
        {
            return new CheckConstraints(Parameters.Owner, tableName)
                .Execute(Parameters.DbConnection);
        }

        public override IList<DatabaseView> Views(string viewName)
        {
            return new Views(Parameters.Owner, viewName)
                .Execute(Parameters.DbConnection);
        }

        public override IList<DatabaseColumn> ViewColumns(string viewName)
        {
            return new ViewColumns(Parameters.Owner, viewName)
                .Execute(Parameters.DbConnection);
        }

        public override IList<DatabasePackage> Packages(string name)
        {
            return new Packages(Parameters.Owner, name)
                .Execute(Parameters.DbConnection);
        }

        public override IList<DatabaseStoredProcedure> StoredProcedures(string name)
        {
            return new StoredProcedures(Parameters.Owner, name)
                .Execute(Parameters.DbConnection);
        }

        public override IList<DatabaseFunction> Functions(string name)
        {
            return new Functions(Parameters.Owner)
                .Execute(Parameters.DbConnection);
        }

        public override IList<DatabaseArgument> ProcedureArguments(string name)
        {
            return new ProcedureArguments(Parameters.Owner, name)
                .Execute(Parameters.DbConnection);
        }

        public override IList<ProcedureSource> ProcedureSources(string name)
        {
            return new ProcedureSources(Parameters.Owner, name)
                .Execute(Parameters.DbConnection);
        }

        public override IList<DatabaseSequence> Sequences(string name)
        {
            return new Sequences(Parameters.Owner).Execute(Parameters.DbConnection);
        }

        public override IList<DatabaseUser> Users()
        {
            return new Users().Execute(Parameters.DbConnection);
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
            if (LooksLikeAutoNumberDefaults(pk.DefaultValue))
            {
                //Oracle 12c default values from sequence
                pk.IsAutoNumber = true;
                return;
            }
            //TODO
            //var match = OracleSequenceTrigger.FindTrigger(databaseTable);
            //if (match != null) pk.IsAutoNumber = true;
        }

        /// <summary>
        /// Does the column default value look like a sequence allocation ("mysequence.NextVal")?
        /// </summary>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static bool LooksLikeAutoNumberDefaults(string defaultValue)
        {
            if (string.IsNullOrEmpty(defaultValue)) return false;
            //simple cases only. If the sequence.nextval is cast/converted,
            return defaultValue.IndexOf(".NEXTVAL", StringComparison.OrdinalIgnoreCase) != -1 ||
                defaultValue.IndexOf(".CURRVAL", StringComparison.OrdinalIgnoreCase) != -1;
        }
    }
}