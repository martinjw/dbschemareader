using System;
using System.Collections.Generic;
using System.Data.Common;

namespace CopyToSQLite
{
    class DatabaseInserter : IDisposable
    {
        protected readonly DbConnection Connection;
        private readonly DbCommand _command;
        private readonly DbTransaction _sqLiteTransaction;

        public DatabaseInserter(DbConnection connection, string insertSql)
        {
            Connection = connection;
            _command = Connection.CreateCommand();
            _command.Connection = Connection;
            _command.CommandText = insertSql;
            Connection.Open();
            _sqLiteTransaction = Connection.BeginTransaction();
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

        protected virtual void CompleteTable()
        {
        }

        public void Dispose()
        {
            CompleteTable();
            _sqLiteTransaction.Commit();
            _sqLiteTransaction.Dispose();
            _command.Dispose();
            Connection.Close();
        }
    }
}
