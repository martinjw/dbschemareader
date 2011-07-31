using System;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// Extensions to enable schema to be created with a simple fluent interface
    /// </summary>
    public static class DatabaseSchemaExtensions
    {
        /// <summary>
        /// Adds a table.
        /// </summary>
        /// <param name="databaseSchema">The database schema.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        public static DatabaseTable AddTable(this DatabaseSchema databaseSchema, string tableName)
        {
            if (databaseSchema == null) throw new ArgumentNullException("databaseSchema", "databaseSchema must not be null");
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException("tableName", "tableName must not be null");

            var table = new DatabaseTable { Name = tableName };
            databaseSchema.Tables.Add(table);
            table.DatabaseSchema = databaseSchema;
            return table;
        }

        /// <summary>
        /// Adds a table.
        /// </summary>
        /// <param name="databaseTable">The database table.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        public static DatabaseTable AddTable(this DatabaseTable databaseTable, string tableName)
        {
            if (databaseTable == null) throw new ArgumentNullException("databaseTable", "databaseTable must not be null");
            var schema = databaseTable.DatabaseSchema;
            return schema.AddTable(tableName);
        }

        /// <summary>
        /// Adds the table.
        /// </summary>
        /// <param name="databaseColumn">The database column.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        public static DatabaseTable AddTable(this DatabaseColumn databaseColumn, string tableName)
        {
            if (databaseColumn == null) throw new ArgumentNullException("databaseColumn", "databaseColumn must not be null");
            var table = databaseColumn.Table;
            return table.AddTable(tableName);
        }

        /// <summary>
        /// Adds a column.
        /// </summary>
        /// <param name="databaseTable">The database table.</param>
        /// <param name="databaseColumn">The database column.</param>
        /// <returns></returns>
        public static DatabaseColumn AddColumn(this DatabaseTable databaseTable, DatabaseColumn databaseColumn)
        {
            if (databaseTable == null) throw new ArgumentNullException("databaseTable", "databaseTable must not be null");
            if (databaseColumn == null) throw new ArgumentNullException("databaseColumn", "databaseColumn must not be null");
            //adds the column with backreferences
            databaseTable.Columns.Add(databaseColumn);
            databaseColumn.Table = databaseTable;
            databaseColumn.TableName = databaseTable.Name;
            return databaseColumn;
        }

        /// <summary>
        /// Adds a column.
        /// </summary>
        /// <param name="databaseColumn">The database column.</param>
        /// <param name="column">The column.</param>
        /// <returns></returns>
        public static DatabaseColumn AddColumn(this DatabaseColumn databaseColumn, DatabaseColumn column)
        {
            if (databaseColumn == null) throw new ArgumentNullException("databaseColumn", "databaseColumn must not be null");
            var table = databaseColumn.Table;
            return table.AddColumn(column);
        }
        /// <summary>
        /// Adds a column.
        /// </summary>
        /// <param name="databaseTable">The database table.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <returns></returns>
        public static DatabaseColumn AddColumn(this DatabaseTable databaseTable, string columnName)
        {
            if (databaseTable == null) throw new ArgumentNullException("databaseTable", "databaseTable must not be null");
            if (string.IsNullOrEmpty(columnName)) throw new ArgumentNullException("columnName", "columnName must not be null");

            var column = new DatabaseColumn { Name = columnName };
            AddColumn(databaseTable, column);
            return column;
        }

        /// <summary>
        /// Adds a column.
        /// </summary>
        /// <param name="databaseTable">The database table.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="dataType">Type of the data.</param>
        /// <returns></returns>
        public static DatabaseColumn AddColumn(this DatabaseTable databaseTable, string columnName, string dataType)
        {
            if (databaseTable == null) throw new ArgumentNullException("databaseTable", "databaseTable must not be null");
            if (string.IsNullOrEmpty(columnName)) throw new ArgumentNullException("columnName", "columnName must not be null");
            if (string.IsNullOrEmpty(dataType)) throw new ArgumentNullException("dataType", "dataType must not be null");

            var column = new DatabaseColumn { Name = columnName, DbDataType = dataType };
            AddColumn(databaseTable, column);
            return column;
        }

        /// <summary>
        /// Adds a column.
        /// </summary>
        /// <param name="databaseTable">The database table.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="dataType">Type of the data.</param>
        /// <param name="columnInitialization">The column initialization.</param>
        /// <returns></returns>
        public static DatabaseColumn AddColumn(this DatabaseTable databaseTable, string columnName, string dataType, Action<DatabaseColumn> columnInitialization)
        {
            if (databaseTable == null) throw new ArgumentNullException("databaseTable", "databaseTable must not be null");
            if (string.IsNullOrEmpty(columnName)) throw new ArgumentNullException("columnName", "columnName must not be null");
            if (string.IsNullOrEmpty(dataType)) throw new ArgumentNullException("dataType", "dataType must not be null");
            if (columnInitialization == null) throw new ArgumentNullException("columnInitialization", "columnInitialization must not be null");

            var column = new DatabaseColumn { Name = columnName, DbDataType = dataType };
            AddColumn(databaseTable, column);
            columnInitialization(column);
            return column;
        }

        /// <summary>
        /// Adds a column to the parent table.
        /// </summary>
        /// <param name="databaseColumn">The database column.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <returns></returns>
        public static DatabaseColumn AddColumn(this DatabaseColumn databaseColumn, string columnName)
        {
            if (databaseColumn == null) throw new ArgumentNullException("databaseColumn", "databaseColumn must not be null");
            var table = databaseColumn.Table;
            return table.AddColumn(columnName);
        }

        /// <summary>
        /// Adds a column to the parent table.
        /// </summary>
        /// <param name="databaseColumn">The database column.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="dataType">Type of the data.</param>
        /// <returns></returns>
        public static DatabaseColumn AddColumn(this DatabaseColumn databaseColumn, string columnName, string dataType)
        {
            if (databaseColumn == null) throw new ArgumentNullException("databaseColumn", "databaseColumn must not be null");
            var table = databaseColumn.Table;
            return table.AddColumn(columnName, dataType);
        }

        /// <summary>
        /// Adds a column to the parent table.
        /// </summary>
        /// <param name="databaseColumn">The database column.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="dataType">Type of the data.</param>
        /// <param name="columnInitialization">The column initialization.</param>
        /// <returns></returns>
        public static DatabaseColumn AddColumn(this DatabaseColumn databaseColumn, string columnName, string dataType, Action<DatabaseColumn> columnInitialization)
        {
            if (databaseColumn == null) throw new ArgumentNullException("databaseColumn", "databaseColumn must not be null");
            var table = databaseColumn.Table;
            return table.AddColumn(columnName, dataType, columnInitialization);
        }
    }
}
