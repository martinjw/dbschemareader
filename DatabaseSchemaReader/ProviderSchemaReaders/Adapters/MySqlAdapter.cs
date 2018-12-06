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
            return new Tables(CommandTimeout, Owner, tableName)
                .Execute(ConnectionAdapter);
        }

        public override IList<DatabaseColumn> Columns(string tableName)
        {
            return new Columns(CommandTimeout, Owner, tableName)
                .Execute(ConnectionAdapter);
        }

        public override IList<DatabaseColumn> ComputedColumns(string tableName)
        {
            return new ComputedColumns(CommandTimeout, Owner, tableName)
                .Execute(ConnectionAdapter);
        }

        public override IList<DatabaseView> Views(string viewName)
        {
            return new Views(CommandTimeout, Owner, viewName)
                .Execute(ConnectionAdapter);
        }

        public override IList<DatabaseColumn> ViewColumns(string viewName)
        {
            return new ViewColumns(CommandTimeout, Owner, viewName)
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

        public override IList<DatabaseStoredProcedure> StoredProcedures(string name)
        {
            return new StoredProcedures(CommandTimeout, Owner, name)
                .Execute(ConnectionAdapter);
        }

        public override IList<DatabaseFunction> Functions(string name)
        {
            return new Functions(CommandTimeout, Owner, name)
                .Execute(ConnectionAdapter);
        }

        public override IList<DatabaseArgument> ProcedureArguments(string name)
        {
            return new ProcedureArguments(CommandTimeout, Owner, name)
                .Execute(ConnectionAdapter);
        }
        public override IList<DatabaseUser> Users()
        {
            return new Users(CommandTimeout).Execute(ConnectionAdapter);
        }

        public override IList<DatabaseDbSchema> Schemas()
        {
            return new Schemas(CommandTimeout).Execute(ConnectionAdapter);
        }
    }
}