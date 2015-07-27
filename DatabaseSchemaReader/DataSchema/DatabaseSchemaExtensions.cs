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
        /// Removes the table from the schema and also all foreign key references.
        /// </summary>
        /// <param name="databaseSchema">The database schema.</param>
        /// <param name="tableName">Name of the table.</param>
        public static void RemoveTable(this DatabaseSchema databaseSchema, string tableName)
        {
            if (databaseSchema == null) throw new ArgumentNullException("databaseSchema", "databaseSchema must not be null");
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException("tableName", "tableName must not be null");

            var table = databaseSchema.FindTableByName(tableName);
            RemoveTable(databaseSchema, table);
        }

        /// <summary>
        /// Removes the table from the schema and also all foreign key references.
        /// </summary>
        /// <param name="databaseSchema">The database schema.</param>
        /// <param name="table">The table.</param>
        /// <exception cref="System.ArgumentNullException">databaseSchema;databaseSchema must not be null</exception>
        public static void RemoveTable(this DatabaseSchema databaseSchema, DatabaseTable table)
        {
            if (databaseSchema == null) throw new ArgumentNullException("databaseSchema", "databaseSchema must not be null");
            if (table == null) throw new ArgumentNullException("table", "table must not be null");

            foreach (var foreignKeyChild in table.ForeignKeyChildren)
            {
                var deleteKeys = foreignKeyChild.ForeignKeys
                    .Where(x => x.RefersToTable == table.Name && x.RefersToSchema == table.SchemaOwner).ToList();
                foreach (var fk in deleteKeys)
                {
                    foreignKeyChild.RemoveForeignKey(fk);
                }
            }
            databaseSchema.Tables.Remove(table);
        }


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
        /// Adds the index with the specified name. If the index with the same name exists, add the column to the index.
        /// </summary>
        /// <param name="databaseColumn">The database column.</param>
        /// <param name="indexName">Name of the index.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">databaseColumn;databaseColumn must not be null</exception>
        public static DatabaseColumn AddIndex(this DatabaseColumn databaseColumn, string indexName)
        {
            if (databaseColumn == null) throw new ArgumentNullException("databaseColumn", "databaseColumn must not be null");
            var databaseTable = databaseColumn.Table;
            if (databaseTable == null) throw new ArgumentException("databaseColumn has no table");

            var index = databaseTable.Indexes.Find(x => Equals(x.Name, indexName));
            if (index == null)
            {
                index = new DatabaseIndex
                   {
                       Name = indexName,
                       TableName = databaseTable.Name,
                       SchemaOwner = databaseTable.SchemaOwner,
                       IndexType = "NONCLUSTERED"
                   };
                databaseTable.AddIndex(index);
            }
            index.Columns.Add(databaseColumn);
            databaseColumn.IsIndexed = true;
            return databaseColumn;
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
            if (!columns.Any()) throw new ArgumentException("columns is empty", "columns");
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

        /// <summary>
        /// Determines whether is a many to many table (association or junction table joining two or more other tables in a many to many relationship)
        /// </summary>
        /// <param name="databaseTable">The database table.</param>
        /// <returns>
        /// 	<c>true</c> if this is a many to many table; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsManyToManyTable(this DatabaseTable databaseTable)
        {
            return (databaseTable.Columns.All(c => c.IsPrimaryKey && c.IsForeignKey));
        }

        /// <summary>
        /// Via a many to many table, find the opposite many relationship
        /// </summary>
        /// <param name="manyToManyTable">The many to many table.</param>
        /// <param name="fromTable">From table.</param>
        /// <returns></returns>
        public static DatabaseTable ManyToManyTraversal(this DatabaseTable manyToManyTable, DatabaseTable fromTable)
        {
            var foreignKey = manyToManyTable.ForeignKeys.FirstOrDefault(x => x.RefersToTable != fromTable.Name);
            //a self many to many
            if (foreignKey == null) return fromTable;
            return foreignKey.ReferencedTable(fromTable.DatabaseSchema);
        }

        /// <summary>
        /// Determines whether two tables are related one to one (shared primary keys)
        /// </summary>
        /// <param name="destination">The destination table (principal).</param>
        /// <param name="origin">The origin table (dependent, has foreign key relationship to principal).</param>
        /// <returns>
        ///   <c>true</c> if this table's primary key is also a foreign key; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsSharedPrimaryKey(this DatabaseTable destination, DatabaseTable origin)
        {
            if (origin == null || destination == null) return false;
            var pk = origin.PrimaryKey;
            if (pk == null) return false;
            var columns = pk.Columns.Select(x => origin.FindColumn(x));
            //the primary key of the origin is also a foreign key to this table
            var allFk = columns.All(c => c.ForeignKeyTableName == destination.Name);
            return allFk;
        }

        /// <summary>
        /// Finds the table that this inherits from (shared primary key)
        /// </summary>
        /// <param name="table">The table.</param>
        /// <returns></returns>
        public static DatabaseTable FindInheritanceTable(this DatabaseTable table)
        {
            if (table == null) return null;
            if (table.PrimaryKeyColumn != null &&
                table.Columns.Where(c => c.IsPrimaryKey).All(c => c.IsForeignKey))
            {
                //all the primary keys are foreign keys.
                var fkTable = table.PrimaryKeyColumn.ForeignKeyTable;
                if (fkTable != null)
                {
                    var count = fkTable.ForeignKeyChildren.Count(childTable => fkTable.IsSharedPrimaryKey(childTable));
                    if (count > 1)
                    {
                        return fkTable;
                    }
                }
            }
            return null;
        }
    }
}
