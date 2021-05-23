using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.Databases.PostgreSql;
using DatabaseSchemaReader.SqlGen.PostgreSql;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Adapters
{
    class PostgreSqlAdapter : ReaderAdapter
    {
        private int _serverVersion;

        public PostgreSqlAdapter(SchemaParameters schemaParameters) : base(schemaParameters)
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

        public override IList<DatabaseColumn> ViewColumns(string viewName)
        {
            return new Columns(CommandTimeout, Owner, viewName)
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

        public override IList<DatabaseIndex> Indexes(string tableName)
        {
            return new Indexes(CommandTimeout, Owner, tableName)
                .Execute(ConnectionAdapter);
        }

        public override IList<DatabaseTrigger> Triggers(string tableName)
        {
            if (_serverVersion == 0)
            {
                _serverVersion = new ServerVersion(CommandTimeout).Execute(ConnectionAdapter);
            }

            var triggers = new Triggers(CommandTimeout, Owner, tableName) {ServerVersion = _serverVersion};
            return triggers.Execute(ConnectionAdapter);
        }

        public override IList<DatabaseConstraint> CheckConstraints(string tableName)
        {
            return new CheckConstraints(CommandTimeout, Owner, tableName)
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

        public override IList<DatabaseView> Views(string viewName)
        {
            var views = new Views(CommandTimeout, Owner, viewName)
                .Execute(ConnectionAdapter);
            if (string.IsNullOrEmpty(viewName) || !views.Any())
            {
                if (_serverVersion == 0)
                {
                    _serverVersion = new ServerVersion(CommandTimeout).Execute(ConnectionAdapter);
                }

                var materializedViews = new MaterializedViews(CommandTimeout, Owner, viewName) {ServerVersion = _serverVersion};
                var mviews = materializedViews
                    .Execute(ConnectionAdapter);
                foreach (var mview in mviews)
                {
                    views.Add(mview);
                }
            }
            return views;
        }

        public override IList<DatabaseFunction> Functions(string name)
        {
            return new Functions(CommandTimeout, Owner)
                .Execute(ConnectionAdapter);
        }

        public override IList<DatabaseSequence> Sequences(string name)
        {
            if (_serverVersion == 0)
            {
                _serverVersion = new ServerVersion(CommandTimeout).Execute(ConnectionAdapter);
            }
            return new Sequences(CommandTimeout, Owner)
                .Execute(ConnectionAdapter, _serverVersion);
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

        public override void PostProcessing(DatabaseTable databaseTable)
        {
            if (databaseTable == null) return;
            //the devart providers GetSchema are a little weird so we fix them up here
            var typeWriter = new DataTypeWriter();

            foreach (var databaseColumn in databaseTable.Columns)
            {
                var santizedType = typeWriter.WriteDataType(databaseColumn);
                //all the different native types are reworked
                if ((santizedType.StartsWith("VARCHAR", StringComparison.OrdinalIgnoreCase)
                    || santizedType.StartsWith("CHAR", StringComparison.OrdinalIgnoreCase)))
                {
                    if (databaseColumn.Length == -1 && databaseColumn.Precision > 0)
                    {
                        databaseColumn.Length = databaseColumn.Precision;
                        databaseColumn.Precision = -1;
                    }
                }
                if ((santizedType.StartsWith("NUMERIC", StringComparison.OrdinalIgnoreCase)
                     || santizedType.StartsWith("DECIMAL", StringComparison.OrdinalIgnoreCase)
                     || santizedType.StartsWith("INTEGER", StringComparison.OrdinalIgnoreCase)))
                {
                    if (databaseColumn.Length > 0 && databaseColumn.Precision == -1)
                    {
                        databaseColumn.Precision = databaseColumn.Length;
                        databaseColumn.Length = -1;
                    }
                }
                //if it's a varchar or char, and the length is -1 but the precision is positive, swap them
                //and vice versa for numerics.

                var defaultValue = databaseColumn.DefaultValue;
                if (!string.IsNullOrEmpty(defaultValue) && defaultValue.StartsWith("nextval('", StringComparison.OrdinalIgnoreCase))
                {
                    databaseColumn.IsAutoNumber = true;
                    databaseColumn.IsPrimaryKey = true;
                }
                //if defaultValue looks like the nextval from a sequence, it's a pk
                //change the type to serial (or bigserial), ensure it's the primary key
            }
        }
    }
}