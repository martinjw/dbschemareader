using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;

namespace CopyToSQLite
{
    sealed class DatabaseInserter : IDisposable
    {
        private readonly SQLiteConnection _connection;
        private readonly SQLiteCommand _command;
        private readonly SQLiteTransaction _sqLiteTransaction;

        public DatabaseInserter(SQLiteConnection connection, string insertSql)
        {
            _connection = connection;
            _command = new SQLiteCommand(insertSql, _connection);
            _connection.Open();
            _sqLiteTransaction = _connection.BeginTransaction();
        }

        public bool Insert(IDictionary<string, object> parameters)
        {
            bool result;
            _command.Parameters.Clear();
            foreach (var column in parameters)
            {
                _command.Parameters.AddWithValue(column.Key, column.Value);
            }
            try
            {
                _command.ExecuteNonQuery();
                result = true;
            }
            catch (SQLiteException exception)
            {
                Debug.WriteLine(exception.Message);
                result = false;
            }
            return result;
        }

        public void Dispose()
        {
            _sqLiteTransaction.Commit();
            _sqLiteTransaction.Dispose();
            _command.Dispose();
            _connection.Close();
        }
    }
}
