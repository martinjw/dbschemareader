using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases
{

    abstract class SqlExecuter<T> : SqlExecuter where T : new()
    {

        protected List<T> Result { get; } = new List<T>();

    }

    abstract class SqlExecuter
    {

        public string Sql { get; set; }

        public string Owner { get; set; }

        protected void ExecuteDbReader(DbConnection connection)
        {
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }

            using (var cmd = connection.CreateCommand())
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

        protected DbParameter AddDbParameter(DbCommand command, string parameterName, object value)
        {
            DbParameter parameter = command.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.Value = value ?? DBNull.Value;
            command.Parameters.Add(parameter);
            return parameter;
        }

        protected abstract void AddParameters(DbCommand command);

        protected abstract void Mapper(IDataRecord record);
    }
}
