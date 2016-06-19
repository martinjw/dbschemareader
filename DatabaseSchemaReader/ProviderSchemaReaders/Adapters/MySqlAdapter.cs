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
            return new Tables(Parameters.Owner, tableName)
                .Execute(Parameters.DbConnection);
        }

        public override IList<DatabaseColumn> Columns(string tableName)
        {
            return new Columns(Parameters.Owner, tableName)
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

        public override IList<DatabaseStoredProcedure> StoredProcedures(string name)
        {
            return new StoredProcedures(Parameters.Owner, name)
                .Execute(Parameters.DbConnection);
        }

        public override IList<DatabaseFunction> Functions(string name)
        {
            return new Functions(Parameters.Owner, name)
                .Execute(Parameters.DbConnection);
        }

        public override IList<DatabaseArgument> ProcedureArguments(string name)
        {
            return new ProcedureArguments(Parameters.Owner, name)
                .Execute(Parameters.DbConnection);
        }
        public override IList<DatabaseUser> Users()
        {
            return new Users().Execute(Parameters.DbConnection);
        }
    }
}