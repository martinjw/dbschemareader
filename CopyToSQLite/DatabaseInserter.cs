using System;
using System.Collections.Generic;
using System.Data.Common;

namespace CopyToSQLite
{
    sealed class DatabaseInserter : IDisposable
    {
        private readonly DbConnection _connection;
        private readonly DbCommand _command;
        private readonly DbTransaction _sqLiteTransaction;

        public DatabaseInserter(DbConnection connection, string insertSql)
        {
            _connection = connection;
            _command = _connection.CreateCommand();
            _command.Connection = _connection;
            _command.CommandText = insertSql;
            _connection.Open();
            _sqLiteTransaction = _connection.BeginTransaction();
        }

        public bool Insert(IDictionary<string, object> parameters)
        {
            bool result;
            _command.Parameters.Clear();
            foreach (var column in parameters)
            {
                var par = _command.CreateParameter();
                par.ParameterName = column.Key;
                par.Value = column.Value;
                _command.Parameters.Add(par);
            }
            try
            {
                _command.ExecuteNonQuery();
                result = true;
            }
            catch (DbException exception)
            {
                //flatten the sql format
                var insertSql = _command.CommandText.Replace(Environment.NewLine, " ").Replace("  ", " ");
                LastErrorMessage = exception.Message + " " + insertSql;
                result = false;
            }
            return result;
        }

        public string LastErrorMessage { get; private set; }

        public void Dispose()
        {
            _sqLiteTransaction.Commit();
            _sqLiteTransaction.Dispose();
            _command.Dispose();
            _connection.Close();
        }
    }
}
