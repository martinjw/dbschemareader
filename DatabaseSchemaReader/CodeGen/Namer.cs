using System;
using System.Linq;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.CodeGen
{
    /// <summary>
    /// Translates table and column names to classes and properties.
    /// </summary>
    public class Namer : ICollectionNamer, INamer
    {
        /// <summary>
        /// Translates the namedObject's Name to a code-friendly name
        /// </summary>
        /// <param name="namedObject">The named object.</param>
        /// <returns></returns>
        public virtual string Name(INamedObject namedObject)
        {
            var name = NameFixer.ToPascalCase(namedObject.Name);
            var column = namedObject as DatabaseColumn;
            if (column != null)
            {
                //if it's a foreign key (CategoryId)
                if (column.IsForeignKey && name.EndsWith("Id", StringComparison.OrdinalIgnoreCase) && name.Length > 2)
                {
                    //remove the "Id" - it's just a "Category"
                    name = name.Substring(0, name.Length - 2);
                }
                //member name cannot be same as class name
                if (name == column.Table.NetName)
                {
                    name += "Property";
                }
            }
            return name;
        }

        /// <summary>
        /// Names the collection.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <returns></returns>
        public virtual string NameCollection(string className)
        {
            return className + "Collection";
        }

        /// <summary>
        /// For a column, returns the property name for a primary key
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns></returns>
        public virtual string PrimaryKeyName(DatabaseColumn column)
        {
            var primaryKeyName = column.NetName;
            if (column.IsPrimaryKey && column.IsForeignKey)
            {
                //if it's a composite key as well, always write an Id version
                var table = column.Table;
                if (table != null && table.HasCompositeKey)
                {
                    return primaryKeyName + "Id";
                }
                //a foreign key will be written, so we need to avoid a collision
                var refTable = column.ForeignKeyTable;
                var fkDataType = refTable != null ? refTable.NetName : column.ForeignKeyTableName;
                if (fkDataType == primaryKeyName)
                    primaryKeyName += "Id";
            }
            return primaryKeyName;
        }

        /// <summary>
        /// Returns the name of a foreign key property for a given foreign key.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="foreignKey">The foreign key.</param>
        /// <returns></returns>
        /// <remarks>
        /// If it is a simple foreign key, it is the NetName of the column
        /// if it is a composite foreign key, it is the NetName of the foreign table
        /// if there is a collision with the class name, append "Key"
        /// If there are multiple foreign keys to one table, ensure they are unique.
        /// </remarks>
        public virtual string ForeignKeyName(DatabaseTable table, DatabaseConstraint foreignKey)
        {
            var refTable = foreignKey.ReferencedTable(table.DatabaseSchema);

            if (refTable == null)
            {
                //we can't find the foreign key table, so just write the columns
                return null;
            }
            //This is a name for the foreign key. Only used for composite keys.
            var propertyName = refTable.NetName;

            //if there is only one column (not composite) use the netName of that column
            if (foreignKey.Columns.Count == 1)
            {
                var columnName = foreignKey.Columns.Single();
                var column = table.FindColumn(columnName);
                //if it is a primary key, we've used the original name for a scalar property
                if (!column.IsPrimaryKey)
                    propertyName = column.NetName;
            }
            else //composite keys
            {
                // Check whether the referenced table is used in any other key. This ensures that the property names
                // are unique.
                if (table.ForeignKeys.Count(x => x.RefersToTable == foreignKey.RefersToTable) > 1)
                {
                    // Append the key name to the property name. In the event of multiple foreign keys to the same table
                    // This will give the consumer context.
                    propertyName += foreignKey.Name;
                }
            }

            // Ensures that property name cannot be the same as class name
            if (propertyName == table.NetName)
            {
                propertyName += "Key";
            }
            return propertyName;
        }

        /// <summary>
        /// Returns the name of an inverse foreign key property. Uses <see cref="NameCollection"/>
        /// For single fks, it's a collection using the name of the fk table.
        /// For multiple fks, it's a collection using the name of the fk columns
        /// </summary>
        /// <param name="targetTable">The target table.</param>
        /// <param name="table">The table.</param>
        /// <param name="foreignKey">The foreign key.</param>
        /// <returns>
        /// Eg OrderLine has fk to Order. Order will have an ICollection&lt;OrderLine&gt; called "OrderLineCollection".
        /// Multiple fk eg Order has Delivery Address and Billing Address. 
        /// Address will have an ICollection&lt;Order&gt; called "DeliveryAddressCollection", 
        /// and another ICollection&lt;Order&gt; called "BillingAddressCollection"
        /// </returns>
        public virtual string ForeignKeyCollectionName(string targetTable, DatabaseTable table, DatabaseConstraint foreignKey)
        {
            var fksToTarget = table.ForeignKeys.Where(x => x.RefersToTable == targetTable).ToList();
            string name = table.NetName;
            if (fksToTarget.Count > 1)
                name = string.Join("", foreignKey.Columns.Select(x => table.FindColumn(x).NetName).ToArray());

            return NameCollection(name);
        }
    }
}
