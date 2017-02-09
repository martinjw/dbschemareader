using System;
using System.Collections.Generic;
using DatabaseSchemaReader.DataSchema;

namespace SqlServerSchemaReader.Schema
{
    /// <summary>
    /// A User Defined Table Type
    /// </summary>
    public class TableType : NamedSchemaObject<TableType>
    {
        private DatabaseConstraint _primaryKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableType"/> class.
        /// </summary>
        public TableType()
        {
            DependentArguments = new List<DatabaseArgument>();
            Columns = new List<DatabaseColumn>();
            UniqueKeys = new List<DatabaseConstraint>();
            CheckConstraints = new List<DatabaseConstraint>();
        }

        /// <summary>
        /// Gets or sets the columns.
        /// </summary>
        public List<DatabaseColumn> Columns { get; set; }

        /// <summary>
        /// Gets or sets the primary key.
        /// </summary>
        public DatabaseConstraint PrimaryKey
        {
            get { return _primaryKey; }
            set
            {
                _primaryKey = value;
                AddConstraintColumns(value);
            }
        }

        /// <summary>
        /// Gets or sets the unique keys.
        /// </summary>
        public List<DatabaseConstraint> UniqueKeys { get; }

        /// <summary>
        /// Gets or sets the check constraints.
        /// </summary>
        public List<DatabaseConstraint> CheckConstraints { get; }


        /// <summary>
        /// After adding constraints, call this to update the constraint columns.
        /// </summary>
        public void UpdateConstraintColumns()
        {
            AddConstraintColumns(PrimaryKey);
            foreach (var uniqueKey in UniqueKeys)
            {
                AddConstraintColumns(uniqueKey);
            }
        }

        private void AddConstraintColumns(DatabaseConstraint con)
        {
            if (con == null) return;
            foreach (string name in con.Columns)
            {
                var col = Columns.Find(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
                if (col == null) continue;
                switch (con.ConstraintType)
                {
                    case ConstraintType.PrimaryKey:
                        col.IsPrimaryKey = true;
                        break;

                    case ConstraintType.UniqueKey:
                        col.IsUniqueKey = true;
                        break;
                }
            }
        }
        /// <summary>
        /// Gets the dependent stored procedure arguments.
        /// </summary>
        public List<DatabaseArgument> DependentArguments { get; }

    }
}
