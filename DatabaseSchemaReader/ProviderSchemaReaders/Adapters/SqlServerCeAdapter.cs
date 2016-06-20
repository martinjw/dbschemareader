using System.Collections.Generic;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.Databases.SqlServerCe;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Adapters
{
    class SqlServerCeAdapter : ReaderAdapter
    {
        public SqlServerCeAdapter(SchemaParameters schemaParameters) : base(schemaParameters)
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

        public override IList<DatabaseIndex> Indexes(string tableName)
        {
            return new Indexes(Parameters.Owner, tableName)
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
    }
}