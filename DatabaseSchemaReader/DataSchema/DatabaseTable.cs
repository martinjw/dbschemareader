using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// A table in the database
    /// </summary>
    [Serializable]
    public partial class DatabaseTable : NamedSchemaObject<DatabaseTable>
    {
        #region Fields

        //backing fields and initialize collections
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private DatabaseConstraint _primaryKey;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly List<DatabaseConstraint> _foreignKeys;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly List<DatabaseConstraint> _uniqueKeys;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly List<DatabaseIndex> _indexes;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly List<DatabaseConstraint> _checkConstraints;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly List<DatabaseColumn> _columns;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly List<DatabaseTrigger> _triggers;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly List<DatabaseTable> _foreignKeyChildren;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly List<DatabaseConstraint> _defaultConstraints;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseTable"/> class.
        /// </summary>
        public DatabaseTable()
        {
            _columns = new List<DatabaseColumn>();
            _triggers = new List<DatabaseTrigger>();
            _indexes = new List<DatabaseIndex>();

            _foreignKeys = new List<DatabaseConstraint>();
            _foreignKeyChildren = new List<DatabaseTable>();
            _uniqueKeys = new List<DatabaseConstraint>();
            _checkConstraints = new List<DatabaseConstraint>();
            _defaultConstraints = new List<DatabaseConstraint>();
        }

        /// <summary>
        /// Gets or sets the database schema. Circular reference useful to move back up the tree.
        /// </summary>
        /// <value>
        /// The database schema.
        /// </value>
        [XmlIgnore]
        public DatabaseSchema DatabaseSchema { get; set; }

        /// <summary>
        /// Gets or sets the table name in .Net (C#) compatible format.
        /// </summary>
        /// <value>
        /// The .net name
        /// </value>
        public string NetName { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets the columns.
        /// </summary>
        public List<DatabaseColumn> Columns { get { return _columns; } }

        /// <summary>
        /// Gets or sets the primary key column (assuming this isn't a composite key- check PrimaryKey.Columns.Count)
        /// </summary>
        /// <value>The primary key column.</value>
        public DatabaseColumn PrimaryKeyColumn
        {
            get { return Columns.FirstOrDefault(c => c.IsPrimaryKey); }
        }

        #region Constraints

        /// <summary>
        /// Gets or sets the primary key.
        /// </summary>
        /// <value>
        /// The primary key.
        /// </value>
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
        /// Gets the foreign keys. Add using <see cref="AddConstraint"/>.
        /// </summary>
        /// <value>
        /// The foreign keys.
        /// </value>
        public List<DatabaseConstraint> ForeignKeys
        {
            get { return _foreignKeys; }
        }

        /// <summary>
        /// Gets or sets the unique keys.
        /// </summary>
        /// <value>
        /// The unique keys.
        /// </value>
        public List<DatabaseConstraint> UniqueKeys
        {
            get { return _uniqueKeys; }
        }

        /// <summary>
        /// Gets or sets the check constraints.
        /// </summary>
        /// <value>
        /// The check constraints.
        /// </value>
        public List<DatabaseConstraint> CheckConstraints
        {
            get { return _checkConstraints; }
        }

        /// <summary>
        /// Gets the default constraints.
        /// </summary>
        /// <value>
        /// The default constraints.
        /// </value>
        public List<DatabaseConstraint> DefaultConstraints
        {
            get { return _defaultConstraints; }
        }

        /// <summary>
        /// Adds the constraints of any type (primary key, foreign key, unique key, check)
        /// </summary>
        /// <param name="constraints">The constraints.</param>
        public void AddConstraints(IEnumerable<DatabaseConstraint> constraints)
        {
            if (constraints == null) return; //nothing to do

            foreach (var constraint in constraints)
            {
                AddConstraint(constraint);
            }
        }

        /// <summary>
        /// Adds the constraint of any type (primary key, foreign key, unique key, check)
        /// </summary>
        /// <param name="con">The constraint.</param>
        public void AddConstraint(DatabaseConstraint con)
        {
            if (con == null) throw new ArgumentNullException("con");

            switch (con.ConstraintType)
            {
                case ConstraintType.PrimaryKey:
                    PrimaryKey = con;
                    break;

                case ConstraintType.ForeignKey:
                    _foreignKeys.Add(con);
                    break;

                case ConstraintType.UniqueKey:
                    _uniqueKeys.Add(con);
                    break;

                case ConstraintType.Check:
                    _checkConstraints.Add(con);
                    break;

                case ConstraintType.Default:
                    _defaultConstraints.Add(con);
                    break;
            }
            AddConstraintColumns(con);
        }

        private void AddConstraintColumns(DatabaseConstraint con)
        {
            if (con == null) return;
            foreach (string name in con.Columns)
            {
                AddConstraintFindColumn(con, name);
            }
        }

        private void AddConstraintFindColumn(DatabaseConstraint con, string name)
        {
            foreach (DatabaseColumn col in Columns)
            {
                if (col.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    switch (con.ConstraintType)
                    {
                        case ConstraintType.PrimaryKey:
                            col.IsPrimaryKey = true;
                            break;

                        case ConstraintType.ForeignKey:
                            if (!string.IsNullOrEmpty(con.RefersToTable))
                            {
                                //ignore fk constraint to a pk without a table.
                                col.IsForeignKey = true;
                                col.ForeignKeyTableName = con.RefersToTable;
                            }
                            break;

                        case ConstraintType.UniqueKey:
                            col.IsUniqueKey = true;
                            break;
                    }
                    break;
                }
            }
        }

        #endregion

        /// <summary>
        /// Removes the foreign key and cleans the column markers.
        /// </summary>
        /// <param name="foreignKey">The foreign key.</param>
        /// <exception cref="System.ArgumentNullException">foreignKey;foreignkey cannot be null</exception>
        /// <exception cref="System.InvalidOperationException">Must be a foreign key</exception>
        internal void RemoveForeignKey(DatabaseConstraint foreignKey)
        {
            if (foreignKey == null) throw new ArgumentNullException("foreignKey", "foreignkey cannot be null");
            if (foreignKey.ConstraintType != ConstraintType.ForeignKey) throw new InvalidOperationException("Must be a foreign key");
            if (!_foreignKeys.Contains(foreignKey)) throw new InvalidOperationException("Foreign key not for this table or already removed");
            _foreignKeys.Remove(foreignKey);
            foreach (var columnName in foreignKey.Columns)
            {
                var column = FindColumn(columnName);
                if (column != null)
                {
                    column.IsForeignKey = false;
                    column.ForeignKeyTableName = null;
                    column.ForeignKeyTable = null;
                }
            }
        }

        /// <summary>
        /// Gets the foreign key children.
        /// </summary>
        [XmlIgnore]
        public List<DatabaseTable> ForeignKeyChildren { get { return _foreignKeyChildren; } }

        /// <summary>
        /// Gets the triggers.
        /// </summary>
        public List<DatabaseTrigger> Triggers { get { return _triggers; } }

        /// <summary>
        /// Gets or sets the indexes.
        /// </summary>
        /// <value>
        /// The indexes.
        /// </value>
        public List<DatabaseIndex> Indexes
        {
            get { return _indexes; }
            set { value.ForEach(AddIndex); }
        }

        /// <summary>
        /// Adds an index.
        /// </summary>
        /// <param name="index">The index.</param>
        public void AddIndex(DatabaseIndex index)
        {
            Indexes.Add(index);

            foreach (DatabaseColumn column in index.Columns)
            {
                string name = column.Name;
                foreach (DatabaseColumn col in Columns)
                {
                    if (col.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        col.IsIndexed = true;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Finds the column.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public DatabaseColumn FindColumn(string name)
        {
            return Columns.Find(col => col.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets a value indicating whether this instance has a composite key.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has a composite key; otherwise, <c>false</c>.
        /// </value>
        public bool HasCompositeKey
        {
            get
            {
                if (PrimaryKey == null) return false;
                return PrimaryKey.Columns.Count > 1;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this table has an autonumber column (identity or equivalent).
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this table has an autonumber column; otherwise, <c>false</c>.
        /// </value>
        public bool HasAutoNumberColumn
        {
            get { return Columns.Any(x => x.IsAutoNumber); }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }
    }
}