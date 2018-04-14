﻿using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Procedures
{
    /// <summary>
    /// Reads the result sets from stored procedures. NB: this executes the sprocs, in a rolled-back transaction.
    /// </summary>
    public class ResultSetReader
    {
        private readonly DatabaseSchema _schema;
        private DbProviderFactory _factory;
        private readonly bool _isOracle;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultSetReader"/> class.
        /// </summary>
        /// <param name="schema">The schema.</param>
        public ResultSetReader(DatabaseSchema schema)
        {
            if (schema == null)
                throw new ArgumentNullException("schema");
            if (string.IsNullOrEmpty(schema.ConnectionString) ||
                string.IsNullOrEmpty(schema.Provider))
            {
                throw new InvalidOperationException("Schema with connection details required");
            }

            _schema = schema;
            _isOracle = (schema.Provider.IndexOf("Oracle", StringComparison.OrdinalIgnoreCase) != -1);
        }

#if !NETSTANDARD2_0
        /// <summary>
        /// Calls each procedure to find the result type.
        /// </summary>
        public void Execute()
        {
            _factory = DbProviderFactories.GetFactory(_schema.Provider);
            using (var dbConnection = _factory.CreateConnection())
            {
                dbConnection.ConnectionString = _schema.ConnectionString;
                dbConnection.Open();
                Execute(dbConnection);
            }
        }

#endif

        /// <summary>
        /// Calls each procedure to find the result type.
        /// </summary>
        public void Execute(DbConnection dbConnection)
        {
            if (dbConnection == null)
                throw new ArgumentNullException("dbConnection");
            //we need the factory to create a DataAdapter (there seems no other generic way to create one)
#if NETSTANDARD2_0
            _factory = Data.FactoryFinder.FindFactory(dbConnection);
#else
            if (_factory != null) _factory = DbProviderFactories.GetFactory(_schema.Provider);
#endif

            foreach (var procedure in _schema.StoredProcedures)
            {
                ExecuteProcedure(procedure, dbConnection);
            }
            foreach (var package in _schema.Packages)
            {
                foreach (var procedure in package.StoredProcedures)
                {
                    ExecuteProcedure(procedure, dbConnection);
                }
            }
        }

#if !NETSTANDARD2_0
        /// <summary>
        /// Calls the specified procedure to find the result type.
        /// </summary>
        /// <param name="procedure">The procedure.</param>
        public void ExecuteProcedure(DatabaseStoredProcedure procedure)
        {
            using (var dbConnection = _factory.CreateConnection())
            {
                dbConnection.ConnectionString = _schema.ConnectionString;
                dbConnection.Open();
                ExecuteProcedure(procedure, dbConnection);
            }
        }
#endif

        /// <summary>
        /// Calls the specified procedure to find the result type.
        /// </summary>
        /// <param name="procedure">The procedure.</param>
        /// <param name="connection">The database connection</param>
        public void ExecuteProcedure(DatabaseStoredProcedure procedure, DbConnection connection)
        {
            //if we can't find the factory, we can't use the data adapter
            if (_factory == null) return;

            var executionName = procedure.Name;
            if (!string.IsNullOrEmpty(procedure.Package))
                executionName = procedure.Package + "." + procedure.Name;

            //for Oracle, sprocs with REF CURSORs indicate it returns something.
            if (_isOracle && !procedure.Arguments.Any(a => a.DatabaseDataType == "REF CURSOR"))
                return;

            using (var resultSet = new DataSet { Locale = CultureInfo.InvariantCulture })
            {
                using (var command = connection.CreateCommand())
                {
                    command.Connection = connection;
                    command.CommandText = executionName;
                    command.CommandTimeout = 5;
                    command.CommandType = CommandType.StoredProcedure;

                    AddParameters(procedure, command);

                    if (connection.State == ConnectionState.Closed)
                        connection.Open();

                    using (var tx = connection.BeginTransaction())
                    {
                        command.Transaction = tx;
                        using (DbDataAdapter adapter = _factory.CreateDataAdapter())
                        {
                            adapter.SelectCommand = command;

                            try
                            {
                                adapter.FillSchema(resultSet, SchemaType.Source);
                            }
                            catch (DbException exception)
                            {
                                //ignore any db exceptions
                                Debug.WriteLine(executionName + Environment.NewLine
                                                + exception.Message);
                            }
                            catch (Exception exception) //for exceptions that don't derive from DbException
                            {
                                //ignore any db exceptions
                                Debug.WriteLine(executionName + Environment.NewLine
                                                + exception.Message);
                            }

                        }
                        tx.Rollback();
                    }
                }
                UpdateProcedure(procedure, resultSet);
            }
        }

        private static void UpdateProcedure(DatabaseStoredProcedure procedure, DataSet resultSet)
        {
            foreach (DataTable table in resultSet.Tables)
            {
                var result = new DatabaseResultSet();
                procedure.ResultSets.Add(result);
                foreach (DataColumn column in table.Columns)
                {
                    var dbColumn = new DatabaseColumn();
                    dbColumn.Name = column.ColumnName;
                    dbColumn.DbDataType = column.DataType.Name;
                    dbColumn.Length = column.MaxLength;
                    dbColumn.Nullable = column.AllowDBNull;
                    result.Columns.Add(dbColumn);
                }
            }
        }

        private static void AddParameters(DatabaseStoredProcedure procedure, DbCommand command)
        {
            foreach (var argument in procedure.Arguments)
            {
                if (argument.Ordinal == 0 && !argument.In && string.Equals(argument.Name, "RETURN_VALUE", StringComparison.OrdinalIgnoreCase))
                    continue;
                var parameter = command.CreateParameter();
                AddParameter(parameter, argument);
                command.Parameters.Add(parameter);
            }
        }

        private static void AddParameter(DbParameter parameter, DatabaseArgument argument)
        {
            parameter.ParameterName = argument.Name;
            if (argument.In && argument.DataType != null)
            {
                //add dummy data
                AddInputParameter(parameter, argument);
            }

            if (argument.Out && argument.In)
                parameter.Direction = ParameterDirection.InputOutput;
            if (argument.Out)
            {
                AddOutputParameter(parameter, argument);
            }
        }

        private static void AddInputParameter(DbParameter parameter, DatabaseArgument argument)
        {
            if (argument.DataType.IsString)
            {
                parameter.DbType = DbType.AnsiString;
                //a string "1" seems better than a letter (TO_NUMBER, CONVERTs etc)
                parameter.Value = "1";
            }
            else if (argument.DataType.IsNumeric)
            {
                parameter.Value = 1;
            }
            else if (argument.DataType.IsDateTime)
            {
                parameter.Value = DateTime.Now;
            }
        }

        private static void AddOutputParameter(DbParameter parameter, DatabaseArgument argument)
        {
            parameter.Direction = ParameterDirection.Output;
            if (argument.DataType != null)
            {
                if (argument.DataType.IsString)
                {
                    parameter.DbType = DbType.AnsiString;
                }
                else if (argument.DataType.IsNumeric)
                {
                    parameter.DbType = DbType.Int32;
                }
                else if (argument.DataType.IsDateTime)
                {
                    parameter.DbType = DbType.DateTime;
                }
            }

            if (argument.DatabaseDataType == "REF CURSOR")
            {
                AddRefCursorParameter(parameter);
            }
        }

        private static void AddRefCursorParameter(DbParameter parameter)
        {
            //we don't want a direct dependency, so we use reflection
            var fullName = parameter.GetType().FullName;
            if (fullName == "System.Data.OracleClient.OracleParameter")
            {
                var prop = parameter.GetType().GetProperty("OracleType");
                if (prop != null)
                {
                    //OracleType.Cursor
                    prop.SetValue(parameter, 5, null);
                }
            }
            else if (fullName == "Oracle.DataAccess.Client.OracleParameter")
            {
                var prop = parameter.GetType().GetProperty("OracleDbType");
                if (prop != null)
                {
                    //OracleDbType.RefCursor
                    prop.SetValue(parameter, 121, null);
                }
            }
        }
    }
}