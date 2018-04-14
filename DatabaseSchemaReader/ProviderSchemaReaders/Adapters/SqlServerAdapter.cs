﻿using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.Databases.SqlServer;
using DatabaseSchemaReader.ProviderSchemaReaders.ResultModels;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Adapters
{
    class SqlServerAdapter : ReaderAdapter
    {
        private bool? _isAzureSqlDatabase;
        private const int SqlServerEngineAzure = 5;

        public SqlServerAdapter(SchemaParameters schemaParameters) : base(schemaParameters)
        {
        }

        /// <summary>
        /// returns the SqlServer version (10 is SqlServer 2008).
        /// </summary>
        /// <param name="connection">The connection (must be OPEN).</param>
        /// <returns>9 is SqlServer 2005, 10 is SqlServer 2008, 11 is SqlServer 2012, 12 is SqlServer 2014</returns>
        public int SqlServerVersion(DbConnection connection)
        {
            //an open connection contains a server version
            //SqlServer 2014 = 12.00.2000
            //SqlAzure (as of 201407 it's SqlServer 2012) = 11.0.9216.62
            //SqlServer 2012 SP2 = 11.0.5058.0
            //SqlServer 2008 R2 SP2 = 10.50.4000.0
            //2005 = 9.00.5000.00 , 2000 = 8.00.2039
            int serverVersion;
            var version = connection.ServerVersion;
            if (string.IsNullOrEmpty(version) || !int.TryParse(version.Substring(0, 2), out serverVersion))
            {
                serverVersion = 9; //SqlServer 2005
            }
            return serverVersion;
        }

        public bool IsAzureSqlDatabase
        {
            get
            {
                if (!_isAzureSqlDatabase.HasValue)
                {
                    var conn = ConnectionAdapter;
                    var serverVersion = SqlServerVersion(conn.DbConnection);

                    if (serverVersion < 11) //before SqlServer 2012, there was no cloud edition
                    {
                        _isAzureSqlDatabase = false;
                    }
                    else
                    {
                        using (var command = conn.DbConnection.CreateCommand())
                        {
                            //Database Engine edition of the instance of SQL Server installed on the server.
                            //1 = Personal or Desktop Engine (Not available for SQL Server 2005.)
                            //2 = Standard (This is returned for Standard and Workgroup.)
                            //3 = Enterprise (This is returned for Enterprise, Enterprise Evaluation, and Developer.)
                            //4 = Express (This is returned for Express, Express Edition with Advanced Services, and Windows Embedded SQL.)
                            //5 = SQL Database
                            //NB: in MONO this returns a SqlVariant, so the CAST is required
                            command.CommandText = "SELECT CAST(SERVERPROPERTY('EngineEdition') AS int)";
                            _isAzureSqlDatabase = (int)command.ExecuteScalar() == SqlServerEngineAzure;
                        }
                    }
                }
                return _isAzureSqlDatabase.Value;
            }
        }

        public override IList<DataType> DataTypes()
        {
            return new DataTypes().Execute();
        }

        public override IList<DatabaseTable> Tables(string tableName)
        {
            return new Tables(Owner, tableName)
                .Execute(ConnectionAdapter);
        }

        public override IList<DatabaseColumn> Columns(string tableName)
        {
            return new Columns(Owner, tableName)
                .Execute(ConnectionAdapter);
        }

        public override IList<DatabaseView> Views(string viewName)
        {
            return new Views(Owner, viewName)
                .Execute(ConnectionAdapter);
        }

        public override IList<ProcedureSource> ViewSources(string viewName)
        {
            return new ViewSources(Owner, viewName)
                .Execute(ConnectionAdapter);
        }

        public override IList<DatabaseColumn> ViewColumns(string viewName)
        {
            return new ViewColumns(Owner, viewName)
                .Execute(ConnectionAdapter);
        }

        public override IList<DatabaseIndex> ViewIndexes(string tableName)
        {
            return new ViewIndexes(Owner, tableName)
                .Execute(ConnectionAdapter);
        }

        public override IList<DatabaseColumn> IdentityColumns(string tableName)
        {
            return new IdentityColumns(Owner, tableName)
                .Execute(ConnectionAdapter);
        }

        public override IList<DatabaseConstraint> CheckConstraints(string tableName)
        {
            return new CheckConstraints(Owner, tableName)
                .Execute(ConnectionAdapter);
        }

        public override IList<DatabaseConstraint> PrimaryKeys(string tableName)
        {
            return new Constraints(Owner, tableName, ConstraintType.PrimaryKey)
                .Execute(ConnectionAdapter);
        }

        public override IList<DatabaseConstraint> UniqueKeys(string tableName)
        {
            return new Constraints(Owner, tableName, ConstraintType.UniqueKey)
                .Execute(ConnectionAdapter);
        }

        public override IList<DatabaseConstraint> ForeignKeys(string tableName)
        {
            return new Constraints(Owner, tableName, ConstraintType.ForeignKey)
                .Execute(ConnectionAdapter);
        }

        public override IList<DatabaseConstraint> DefaultConstraints(string tableName)
        {
            return new DefaultConstraints(Owner, tableName)
                .Execute(ConnectionAdapter);
        }

        public override IList<DatabaseColumn> ComputedColumns(string tableName)
        {
            return new ComputedColumns(Owner, tableName)
                .Execute(ConnectionAdapter);
        }

        public override IList<DatabaseIndex> Indexes(string tableName)
        {
            return new Indexes(Owner, tableName)
                .Execute(ConnectionAdapter);
        }

        public override IList<DatabaseTrigger> Triggers(string tableName)
        {
            return new Triggers(Owner, tableName)
                .Execute(ConnectionAdapter);
        }

        public override IList<DatabaseTable> ColumnDescriptions(string tableName)
        {
            return new ColumnDescriptions(Owner, tableName)
                .Execute(ConnectionAdapter);
        }

        public override IList<DatabaseTable> TableDescriptions(string tableName)
        {
            return new TableDescriptions(Owner, tableName)
                .Execute(ConnectionAdapter);
        }

        public override IList<DatabaseSequence> Sequences(string name)
        {
            return new Sequences(Owner).Execute(ConnectionAdapter);
        }

        public override IList<DatabaseStoredProcedure> StoredProcedures(string name)
        {
            return new StoredProcedures(Owner, name)
                .Execute(ConnectionAdapter);
        }

        public override IList<DatabaseFunction> Functions(string name)
        {
            return new Functions(Owner, name)
                .Execute(ConnectionAdapter);
        }

        public override IList<DatabaseArgument> ProcedureArguments(string name)
        {
            return new ProcedureArguments(Owner, name)
                .Execute(ConnectionAdapter);
        }

        public override IList<ProcedureSource> ProcedureSources(string name)
        {
            return new ProcedureSources(Owner, null)
                .Execute(ConnectionAdapter);
        }

        public override IList<DatabaseUser> Users()
        {
            return new Users().Execute(ConnectionAdapter);
        }

        public override IList<DatabaseDbSchema> Schemas()
        {
            return new Schemas().Execute(ConnectionAdapter);
        }

        public override void PostProcessing(DatabaseTable databaseTable)
        {
            if (databaseTable == null) return;
            //look at default values to see if uses a sequence
            LookForAutoGeneratedId(databaseTable);
        }

        private static void LookForAutoGeneratedId(DatabaseTable databaseTable)
        {
            var pk = databaseTable.PrimaryKeyColumn;
            if (pk == null) return;
            if (databaseTable.HasAutoNumberColumn) return;
            if (string.IsNullOrEmpty(pk.DefaultValue)) return;
            if (pk.DefaultValue.IndexOf("NEXT VALUE FOR ", StringComparison.OrdinalIgnoreCase) != -1)
                pk.IsAutoNumber = true;
        }
    }
}