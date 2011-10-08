using System;
using System.Collections.Generic;
using System.Linq;

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
            table.SchemaOwner = databaseSchema.Owner;
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
        /// Adds the index.
        /// </summary>
        /// <param name="databaseTable">The database table.</param>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="columns">The columns.</param>
        /// <returns></returns>
        public static DatabaseTable AddIndex(this DatabaseTable databaseTable, string indexName, IEnumerable<DatabaseColumn> columns)
        {
            if (databaseTable == null) throw new ArgumentNullException("databaseTable", "databaseTable must not be null");
            if (columns.Count() == 0) throw new ArgumentException("columns is empty", "columns");
            var index = new DatabaseIndex
                            {
                                Name = indexName,
                                TableName = databaseTable.Name,
                                SchemaOwner = databaseTable.SchemaOwner,
                                IndexType = "NONCLUSTERED"
                            };
            index.Columns.AddRange(columns);
            databaseTable.AddIndex(index);
            return databaseTable;
        }
    }
}
