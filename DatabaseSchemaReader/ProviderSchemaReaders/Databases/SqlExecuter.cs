using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases
{

    abstract class SqlExecuter<T> : SqlExecuter where T : new()
    {
        protected SqlExecuter(int? commandTimeout, string owner)
        {
            Owner = owner;
            CommandTimeout = commandTimeout;
        }
        protected List<T> Result { get; } = new List<T>();
    }

    abstract class SqlExecuter
    {
        public string Sql { get; set; }

        public string Owner { get; set; }

        public int? CommandTimeout { get; set; }

        protected void ExecuteDbReader(IConnectionAdapter connectionAdapter)
        {
            Trace.WriteLine($"Sql: {Sql}");
            using (var cmd = BuildCommand(connectionAdapter))
            {
                cmd.CommandText = Sql;
                AddParameters(cmd);
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        Mapper(dr);
                    }
                }
            }
        }

        protected DbCommand BuildCommand(IConnectionAdapter connectionAdapter)
        {
            var connection = connectionAdapter.DbConnection;
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }
            var cmd = connection.CreateCommand();
            var transaction = connectionAdapter.DbTransaction;
            if (transaction != null)
            {
                cmd.Transaction = transaction;
            }

            if (CommandTimeout.HasValue && CommandTimeout.Value >= 0)
            {
                cmd.CommandTimeout = CommandTimeout.Value;
            }

            return cmd;
        }

        protected static DbParameter AddDbParameter(DbCommand command, string parameterName, object value, DbType? dbType = null)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.Value = value ?? DBNull.Value;
            if (dbType.HasValue) parameter.DbType = dbType.Value; //SqlServerCe needs this
            command.Parameters.Add(parameter);
            return parameter;
        }

        protected abstract void AddParameters(DbCommand command);

        protected abstract void Mapper(IDataRecord record);
    }
}
