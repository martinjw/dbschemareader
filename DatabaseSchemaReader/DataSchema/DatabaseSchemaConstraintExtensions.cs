using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// Extensions to enable schema to be created with a simple fluent interface
    /// </summary>
    public static class DatabaseSchemaConstraintExtensions
    {

        /// <summary>
        /// Makes this column the primary key (or part of a composite key)
        /// </summary>
        /// <param name="databaseColumn">The database column.</param>
        /// <returns></returns>
        public static DatabaseColumn AddPrimaryKey(this DatabaseColumn databaseColumn)
        {
            return databaseColumn.AddPrimaryKey(null);
        }

        /// <summary>
        /// Makes this column the primary key (or part of a composite key)
        /// </summary>
        /// <param name="databaseColumn">The database column.</param>
        /// <param name="primaryKeyName">Name of the primary key.</param>
        /// <returns></returns>
        public static DatabaseColumn AddPrimaryKey(this DatabaseColumn databaseColumn, string primaryKeyName)
        {
            if (databaseColumn == null) throw new ArgumentNullException("databaseColumn", "databaseColumn must not be null");
            var table = databaseColumn.Table;
            if (table.PrimaryKey == null)
            {
                table.PrimaryKey = new DatabaseConstraint
                                       {
                                           ConstraintType = ConstraintType.PrimaryKey,
                                           TableName = table.Name,
                                           Name = primaryKeyName
                                       };
            }
            else if (primaryKeyName != null)
            {
                table.PrimaryKey.Name = primaryKeyName;
            }
            if (!table.PrimaryKey.Columns.Contains(databaseColumn.Name))
                table.PrimaryKey.Columns.Add(databaseColumn.Name);
            databaseColumn.IsPrimaryKey = true;
            databaseColumn.Nullable = false; //you can't have a nullable pk
            //if they haven't explicitly set a type, make it integer
            if (string.IsNullOrEmpty(databaseColumn.DbDataType))
                databaseColumn.DbDataType = "INT";
            return databaseColumn;
        }

        /// <summary>
        /// Adds the identity.
        /// </summary>
        /// <param name="databaseColumn">The database column.</param>
        /// <returns></returns>
        public static DatabaseColumn AddIdentity(this DatabaseColumn databaseColumn)
        {
            if (databaseColumn == null) throw new ArgumentNullException("databaseColumn", "databaseColumn must not be null");
            var table = databaseColumn.Table;
            if (table.HasIdentityColumn && !databaseColumn.IsIdentity)
            {
                var existingIdentity = table.Columns.First(x => x.IsIdentity);
                existingIdentity.IsIdentity = false;
            }
            databaseColumn.IsIdentity = true;
            if (string.IsNullOrEmpty(databaseColumn.DbDataType))
                databaseColumn.DbDataType = "INT";
            return databaseColumn;
        }

        /// <summary>
        /// Adds a foreign key with a single column
        /// </summary>
        /// <param name="databaseColumn">The database column.</param>
        /// <param name="foreignKeyName">Name of the foreign key.</param>
        /// <param name="foreignTableName">Name of the foreign table.</param>
        /// <returns></returns>
        public static DatabaseColumn AddForeignKey(this DatabaseColumn databaseColumn, string foreignKeyName, string foreignTableName)
        {
            if (databaseColumn == null) throw new ArgumentNullException("databaseColumn", "databaseColumn must not be null");
            if (string.IsNullOrEmpty(foreignTableName)) throw new ArgumentNullException("foreignTableName", "foreignTableName must not be null");
            var table = databaseColumn.Table;
            var foreignKey = new DatabaseConstraint
            {
                ConstraintType = ConstraintType.ForeignKey,
                Name = foreignKeyName,
                TableName = table.Name,
                RefersToTable = foreignTableName
            };
            foreignKey.Columns.Add(databaseColumn.Name);
            table.AddConstraint(foreignKey);
            databaseColumn.IsForeignKey = true;

            //add the inverse relationship
            var fkTable = table.DatabaseSchema.FindTableByName(foreignTableName);
            if (fkTable != null && !fkTable.ForeignKeyChildren.Contains(table))
            {
                fkTable.ForeignKeyChildren.Add(table);
                databaseColumn.ForeignKeyTable = fkTable;
            }

            return databaseColumn;
        }

        /// <summary>
        /// Finds the constraint by name (case insensitive)
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static DatabaseConstraint FindByName(this ReadOnlyCollection<DatabaseConstraint> collection, string name)
        {
            if (collection == null) return null;
            if (string.IsNullOrEmpty(name)) return null;
            return collection.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Adds a foreign key with a single column
        /// </summary>
        /// <param name="databaseColumn">The database column.</param>
        /// <param name="foreignKeyName">Name of the foreign key.</param>
        /// <param name="foreignTable">The foreign table.</param>
        /// <returns></returns>
        public static DatabaseColumn AddForeignKey(this DatabaseColumn databaseColumn, string foreignKeyName, Func<IEnumerable<DatabaseTable>, DatabaseTable> foreignTable)
        {
            if (databaseColumn == null) throw new ArgumentNullException("databaseColumn", "databaseColumn must not be null");
            if (foreignTable == null) throw new ArgumentNullException("foreignTable", "foreignTable must not be null");
            var table = databaseColumn.Table;
            var fkTable = foreignTable(table.DatabaseSchema.Tables);
            return databaseColumn.AddForeignKey(foreignKeyName, fkTable.Name);
        }

        /// <summary>
        /// Adds a foreign key with a single column (without a name)
        /// </summary>
        /// <param name="databaseColumn">The database column.</param>
        /// <param name="foreignTableName">Name of the foreign table.</param>
        /// <returns></returns>
        public static DatabaseColumn AddForeignKey(this DatabaseColumn databaseColumn, string foreignTableName)
        {
            return databaseColumn.AddForeignKey(null, foreignTableName);
        }

        /// <summary>
        /// Makes this column a unique key.
        /// </summary>
        /// <param name="databaseColumn">The database column.</param>
        /// <returns></returns>
        public static DatabaseColumn AddUniqueKey(this DatabaseColumn databaseColumn)
        {
            return databaseColumn.AddUniqueKey(null);
        }

        /// <summary>
        /// Adds a unique key.
        /// </summary>
        /// <param name="databaseColumn">The database column.</param>
        /// <param name="uniqueKeyName">Name of the unique key.</param>
        /// <returns></returns>
        public static DatabaseColumn AddUniqueKey(this DatabaseColumn databaseColumn, string uniqueKeyName)
        {
            if (databaseColumn == null) throw new ArgumentNullException("databaseColumn", "databaseColumn must not be null");
            var table = databaseColumn.Table;
            var uk = new DatabaseConstraint
             {
                 ConstraintType = ConstraintType.UniqueKey,
                 TableName = table.Name,
                 Name = uniqueKeyName
             };
            uk.Columns.Add(databaseColumn.Name);
            table.AddConstraint(uk);
            databaseColumn.IsUniqueKey = true;
            return databaseColumn;
        }

        /// <summary>
        /// Adds the column.
        /// </summary>
        /// <param name="databaseConstraint">The database constraint.</param>
        /// <param name="databaseColumn">The database column.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">databaseConstraint</exception>
        public static DatabaseConstraint AddColumn(this DatabaseConstraint databaseConstraint, DatabaseColumn databaseColumn)
        {
            if (databaseConstraint == null) throw new ArgumentNullException("databaseConstraint");
            if (databaseColumn == null) throw new ArgumentNullException("databaseColumn");
            if (databaseColumn.TableName != databaseConstraint.TableName) throw new InvalidOperationException("Constraint and column must belong to same table");

            databaseConstraint.Columns.Add(databaseColumn.Name);
            switch (databaseConstraint.ConstraintType)
            {
                case ConstraintType.ForeignKey:
                    databaseColumn.IsForeignKey = true;
                    databaseColumn.ForeignKeyTableName = databaseConstraint.RefersToTable;
                    break;
                case ConstraintType.PrimaryKey:
                    databaseColumn.IsPrimaryKey = true;
                    break;
                case ConstraintType.UniqueKey:
                    databaseColumn.IsUniqueKey = true;
                    break;
            }

            return databaseConstraint;
        }

        /// <summary>
        /// Finds the individual foreign key constraints for a foreing key child
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="foreignKeyChild">The foreign key child.</param>
        /// <returns></returns>
        public static IList<DatabaseConstraint> InverseForeignKeys(this DatabaseTable table, DatabaseTable foreignKeyChild)
        {
            return foreignKeyChild.ForeignKeys.Where(x => x.RefersToTable == table.Name).ToList();
        }
    }
}
