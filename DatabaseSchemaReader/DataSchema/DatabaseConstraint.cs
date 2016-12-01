using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// Represents a constraint (of <see cref="ConstraintType"/> such as primary key, foreign key...) that is attached to <see cref="Columns"/> of a table with name <see cref="TableName"/>
    /// </summary>
    [Serializable]
    public partial class DatabaseConstraint : NamedSchemaObject<DatabaseConstraint>
    {
        #region Fields
        //backing fields
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly List<string> _columns;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseConstraint"/> class.
        /// </summary>
        public DatabaseConstraint()
        {
            _columns = new List<string>();
        }

        /// <summary>
        /// Gets or sets the name of the parent table.
        /// </summary>
        /// <value>
        /// The name of the table.
        /// </value>
        public string TableName { get; set; }

        /// <summary>
        /// If this is a foreign key constraint, gets or sets the constraint on the foreign key table (i.e. the primary key constraint).
        /// </summary>
        /// <value>
        /// The refers to constraint.
        /// </value>
        public string RefersToConstraint { get; set; }

        /// <summary>
        /// If this is a foreign key constraint, gets or sets the foreign key table name. Use <see cref="ReferencedTable"/> to get the foreign key table.
        /// </summary>
        /// <value>
        /// The refers to table.
        /// </value>
        public string RefersToTable { get; set; }

        /// <summary>
        /// If this is a foreign key constraint, gets or sets the foreign key schema. Use <see cref="ReferencedTable"/> to get the foreign key table.
        /// </summary>
        /// <value>
        /// The refers to schema.
        /// </value>
        public string RefersToSchema { get; set; }

        /// <summary>
        /// Gets or sets the delete rule. When a row is deleted from a parent table, the DeleteRule determines what will happen in the columns of the child table (or tables). If the rule is set to Cascade, child rows will be deleted. Other options are SET NULL, SET DEFAULT and NO ACTION.
        /// </summary>
        /// <value>
        /// The delete rule.
        /// </value>
        public string DeleteRule { get; set; }

        /// <summary>
        /// Gets or sets the update rule. When a row is updated in a parent table, the UpdateRule determines what will happen in the columns of the child table (or tables). Options are CASCADE, RESTRICT, NO ACTION, SET NULL, SET DEFAULT
        /// </summary>
        /// <value>
        /// The update rule.
        /// </value>
        public string UpdateRule { get; set; }

        /// <summary>
        /// Gets or sets the type of the constraint (primary key, foreign key, unique key, check)
        /// </summary>
        /// <value>
        /// The type of the constraint.
        /// </value>
        public ConstraintType ConstraintType { get; set; }

        /// <summary>
        /// Gets the columns. A check constraint has no columns.
        /// </summary>
        public List<string> Columns { get { return _columns; } }

        /// <summary>
        /// Gets or sets the expression (check constraints only).
        /// </summary>
        public string Expression { get; set; }

        /// <summary>
        /// If this is a foreign key constraint, gets or sets the foreign key table.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <returns></returns>
        public DatabaseTable ReferencedTable(DatabaseSchema schema)
        {
            if (schema == null) return null;

            //first look up the refers to table name
            var refTable = schema.Tables
                .FirstOrDefault(table =>
                     //the string RefersToTable is the same
                     string.Equals(table.Name, RefersToTable, StringComparison.OrdinalIgnoreCase)
                     //if we have a schema name, must match too
                     && (string.IsNullOrEmpty(RefersToSchema) || 
                     string.Equals(table.SchemaOwner, RefersToSchema, StringComparison.OrdinalIgnoreCase)));

            //if not found, look for the constraint name
            if (refTable == null && !string.IsNullOrEmpty(RefersToConstraint))
                refTable = schema.Tables
                 .FirstOrDefault(table =>
                     //or the RefersToConstraint is it's primary key
                     (table.PrimaryKey != null &&
                     //one or the other may not have a name
                     string.Equals(table.PrimaryKey.Name, RefersToConstraint, StringComparison.OrdinalIgnoreCase)));

            return refTable;
        }

        /// <summary>
        /// If this is a foreign key constraint, gets the primary/unique key columns of a referenced table.
        /// </summary>
        /// <param name="schema">The schema.</param>
        public IEnumerable<string> ReferencedColumns(DatabaseSchema schema)
        {
            var referencedTable = ReferencedTable(schema);
            if (referencedTable == null) return null;

            //work item #1023 foreign key references to unique keys
            if (!string.IsNullOrEmpty(RefersToConstraint))
            {
                foreach (var uniqueKey in referencedTable.UniqueKeys)
                {
                    if (RefersToConstraint.Equals(uniqueKey.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        return uniqueKey.Columns;
                    }
                }
            }

            if (referencedTable.PrimaryKey == null) return null; //No primary key defined! 
            var refColumnList = referencedTable.PrimaryKey.Columns;
            return refColumnList;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return (Name ?? ConstraintType.ToString()) +
                " on " + TableName;
        }
    }
}
