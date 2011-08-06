using System;
using System.Data;
using DatabaseSchemaReader.SqlGen;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// AddColumn extensions (on table and column)
    /// </summary>
    public static class DatabaseSchemaAddColumnExtensions
    {
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
            if (String.IsNullOrEmpty(columnName)) throw new ArgumentNullException("columnName", "columnName must not be null");

            var column = new DatabaseColumn { Name = columnName };
            AddColumn(databaseTable, column);
            return column;
        }

        /// <summary>
        /// Adds a column.
        /// </summary>
        /// <param name="databaseTable">The database table.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="dataType">Type of the data. Eg "INT", "VARCHAR(10)", "DECIMAL(10,2)"</param>
        /// <returns></returns>
        public static DatabaseColumn AddColumn(this DatabaseTable databaseTable, string columnName, string dataType)
        {
            if (databaseTable == null) throw new ArgumentNullException("databaseTable", "databaseTable must not be null");
            if (String.IsNullOrEmpty(columnName)) throw new ArgumentNullException("columnName", "columnName must not be null");
            if (String.IsNullOrEmpty(dataType)) throw new ArgumentNullException("dataType", "dataType must not be null");

            var column = DataTypeConverter.ParseDataType(dataType);
            column.Name = columnName;
            AddColumn(databaseTable, column);
            return column;
        }

        /// <summary>
        /// Adds a column.
        /// </summary>
        /// <param name="databaseTable">The database table.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="dbType"><see cref="DbType"/>.</param>
        /// <returns></returns>
        public static DatabaseColumn AddColumn(this DatabaseTable databaseTable, string columnName, DbType dbType)
        {
            var dataType = DataTypeMappingFactory.DataTypeMapper(databaseTable).Map(dbType);
            return databaseTable.AddColumn(columnName, dataType);
        }

        /// <summary>
        /// Adds a column.
        /// </summary>
        /// <param name="databaseTable">The database table.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="dataType">Type of the data. Eg "INT", "VARCHAR(10)", "DECIMAL(10,2)"</param>
        /// <param name="columnInitialization">The column initialization.</param>
        /// <returns></returns>
        public static DatabaseColumn AddColumn(this DatabaseTable databaseTable, string columnName, string dataType, Action<DatabaseColumn> columnInitialization)
        {
            if (databaseTable == null) throw new ArgumentNullException("databaseTable", "databaseTable must not be null");
            if (String.IsNullOrEmpty(columnName)) throw new ArgumentNullException("columnName", "columnName must not be null");
            if (String.IsNullOrEmpty(dataType)) throw new ArgumentNullException("dataType", "dataType must not be null");
            if (columnInitialization == null) throw new ArgumentNullException("columnInitialization", "columnInitialization must not be null");

            var column = DataTypeConverter.ParseDataType(dataType);
            column.Name = columnName;
            AddColumn(databaseTable, column);
            columnInitialization(column);
            return column;
        }

        /// <summary>
        /// Adds a column.
        /// </summary>
        /// <param name="databaseTable">The database table.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="dbType"><see cref="DbType"/>.</param>
        /// <param name="columnInitialization">The column initialization.</param>
        /// <returns></returns>
        public static DatabaseColumn AddColumn(this DatabaseTable databaseTable, string columnName, DbType dbType, Action<DatabaseColumn> columnInitialization)
        {
            var dataType = DataTypeMappingFactory.DataTypeMapper(databaseTable).Map(dbType);
            return databaseTable.AddColumn(columnName, dataType, columnInitialization);
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
        /// <param name="dataType">Type of the data. Eg "INT", "VARCHAR(10)", "DECIMAL(10,2)"</param>
        /// <returns></returns>
        public static DatabaseColumn AddColumn(this DatabaseColumn databaseColumn, string columnName, string dataType)
        {
            if (databaseColumn == null) throw new ArgumentNullException("databaseColumn", "databaseColumn must not be null");
            var table = databaseColumn.Table;
            return table.AddColumn(columnName, dataType);
        }

        /// <summary>
        /// Adds a column.
        /// </summary>
        /// <param name="databaseColumn">The database column.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="dbType"><see cref="DbType"/>.</param>
        /// <returns></returns>
        public static DatabaseColumn AddColumn(this DatabaseColumn databaseColumn, string columnName, DbType dbType)
        {
            if (databaseColumn == null) throw new ArgumentNullException("databaseColumn", "databaseColumn must not be null");
            var dataType = DataTypeMappingFactory.DataTypeMapper(databaseColumn.Table).Map(dbType);
            return databaseColumn.AddColumn(columnName, dataType);
        }

        /// <summary>
        /// Adds a column to the parent table.
        /// </summary>
        /// <param name="databaseColumn">The database column.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="dataType">Type of the data. Eg "INT", "VARCHAR(10)", "DECIMAL(10,2)"</param>
        /// <param name="columnInitialization">The column initialization.</param>
        /// <returns></returns>
        public static DatabaseColumn AddColumn(this DatabaseColumn databaseColumn, string columnName, string dataType, Action<DatabaseColumn> columnInitialization)
        {
            if (databaseColumn == null) throw new ArgumentNullException("databaseColumn", "databaseColumn must not be null");
            var table = databaseColumn.Table;
            return table.AddColumn(columnName, dataType, columnInitialization);
        }

        /// <summary>
        /// Adds the column.
        /// </summary>
        /// <param name="databaseColumn">The database column.</param>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="dbType"><see cref="DbType"/>.</param>
        /// <param name="columnInitialization">The column initialization.</param>
        /// <returns></returns>
        public static DatabaseColumn AddColumn(this DatabaseColumn databaseColumn, string columnName, DbType dbType, Action<DatabaseColumn> columnInitialization)
        {
            if (databaseColumn == null) throw new ArgumentNullException("databaseColumn", "databaseColumn must not be null");
            var dataType = DataTypeMappingFactory.DataTypeMapper(databaseColumn.Table).Map(dbType);
            return databaseColumn.AddColumn(columnName, dataType, columnInitialization);
        }

        /// <summary>
        /// Adds a length to a column. Use -1 for MAX.
        /// </summary>
        /// <param name="databaseColumn">The database column.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static DatabaseColumn AddLength(this DatabaseColumn databaseColumn, int length)
        {
            if (databaseColumn == null) throw new ArgumentNullException("databaseColumn", "databaseColumn must not be null");
            databaseColumn.Length = length;
            return databaseColumn;
        }


        /// <summary>
        /// Adds a precision and scale to a column
        /// </summary>
        /// <param name="databaseColumn">The database column.</param>
        /// <param name="precision">The precision (number of digits in a number)</param>
        /// <param name="scale">The scale (number of digits to the right of the decimal point)</param>
        /// <returns></returns>
        public static DatabaseColumn AddPrecisionScale(this DatabaseColumn databaseColumn, int precision, int scale)
        {
            if (databaseColumn == null) throw new ArgumentNullException("databaseColumn", "databaseColumn must not be null");
            databaseColumn.Precision = precision;
            databaseColumn.Scale = scale;
            return databaseColumn;
        }

        /// <summary>
        /// Make the column nullable.
        /// </summary>
        /// <param name="databaseColumn">The database column.</param>
        /// <returns></returns>
        public static DatabaseColumn AddNullable(this DatabaseColumn databaseColumn)
        {
            if (databaseColumn == null) throw new ArgumentNullException("databaseColumn", "databaseColumn must not be null");
            databaseColumn.Nullable = true;
            return databaseColumn;
        }
    }
}
