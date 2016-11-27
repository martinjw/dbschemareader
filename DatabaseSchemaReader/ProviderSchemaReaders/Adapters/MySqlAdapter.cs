using System.Collections.Generic;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.Databases.MySql;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Adapters
{
    class MySqlAdapter : ReaderAdapter
    {
        public MySqlAdapter(SchemaParameters schemaParameters) : base(schemaParameters)
        {
        }
		
        public override IList<DataType> DataTypes()
        {
            return new DataTypeList().Execute();
        }

        public override IList<DatabaseTable> Tables(string tableName)
        {
            return new Tables(Owner, tableName)
                .Execute(DbConnection);
        }

        public override IList<DatabaseColumn> Columns(string tableName)
        {
            return new Columns(Owner, tableName)
                .Execute(DbConnection);
        }

        public override IList<DatabaseColumn> ComputedColumns(string tableName)
        {
            return new ComputedColumns(Owner, tableName)
                .Execute(DbConnection);
        }

        public override IList<DatabaseView> Views(string viewName)
        {
            return new Views(Owner, viewName)
                .Execute(DbConnection);
        }

        public override IList<DatabaseColumn> ViewColumns(string viewName)
        {
            return new ViewColumns(Owner, viewName)
                .Execute(DbConnection);
        }

        public override IList<DatabaseConstraint> PrimaryKeys(string tableName)
        {
            return new Constraints(Owner, tableName, ConstraintType.PrimaryKey)
                .Execute(DbConnection);
        }

        public override IList<DatabaseConstraint> UniqueKeys(string tableName)
        {
            return new Constraints(Owner, tableName, ConstraintType.UniqueKey)
                .Execute(DbConnection);
        }

        public override IList<DatabaseConstraint> ForeignKeys(string tableName)
        {
            return new Constraints(Owner, tableName, ConstraintType.ForeignKey)
                .Execute(DbConnection);
        }

        public override IList<DatabaseColumn> IdentityColumns(string tableName)
        {
            return new IdentityColumns(Owner, tableName)
                .Execute(DbConnection);
        }

        public override IList<DatabaseIndex> Indexes(string tableName)
        {
            return new Indexes(Owner, tableName)
                .Execute(DbConnection);
        }

        public override IList<DatabaseTrigger> Triggers(string tableName)
        {
            return new Triggers(Owner, tableName)
                .Execute(DbConnection);
        }

        public override IList<DatabaseStoredProcedure> StoredProcedures(string name)
        {
            return new StoredProcedures(Owner, name)
                .Execute(DbConnection);
        }

        public override IList<DatabaseFunction> Functions(string name)
        {
            return new Functions(Owner, name)
                .Execute(DbConnection);
        }

        public override IList<DatabaseArgument> ProcedureArguments(string name)
        {
            return new ProcedureArguments(Owner, name)
                .Execute(DbConnection);
        }
        public override IList<DatabaseUser> Users()
        {
            return new Users().Execute(DbConnection);
        }
    }
}