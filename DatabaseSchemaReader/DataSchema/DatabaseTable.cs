using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// A table in the database
    /// </summary>
    [Serializable]
    public partial class DatabaseTable
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
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseTable"/> class.
        /// </summary>
        public DatabaseTable()
        {
            _columns = new List<DatabaseColumn>();
            _triggers = new List<DatabaseTrigger>();
            _foreignKeyChildren = new List<DatabaseTable>();
            _indexes = new List<DatabaseIndex>();
            _uniqueKeys = new List<DatabaseConstraint>();
            _foreignKeys = new List<DatabaseConstraint>();
            _checkConstraints = new List<DatabaseConstraint>();
        }

        /// <summary>
        /// Gets or sets the database schema.
        /// </summary>
        /// <value>
        /// The database schema.
        /// </value>
        public DatabaseSchema DatabaseSchema { get; set; }

        /// <summary>
        /// Gets or sets the table name (original database format)
        /// </summary>
        /// <value>
        /// The table name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the table name in .Net (C#) compatible format.
        /// </summary>
        /// <value>
        /// The .net name
        /// </value>
        public string NetName { get; set; }

        /// <summary>
        /// Gets or sets the schema owner.
        /// </summary>
        /// <value>
        /// The schema owner.
        /// </value>
        public string SchemaOwner { get; set; }

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
        /// Gets or sets the foreign keys.
        /// </summary>
        /// <value>
        /// The foreign keys.
        /// </value>
        public List<DatabaseConstraint> ForeignKeys
        {
            get { return _foreignKeys; }
            set { value.ForEach(AddConstraint); }
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
            set { value.ForEach(AddConstraint); }
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
            set { value.ForEach(AddConstraint); }
        }
        /// <summary>
        /// Adds the constraint of any type (primary key, foreign key, unique key, check)
        /// </summary>
        /// <param name="con">The constraint.</param>
        public void AddConstraint(DatabaseConstraint con)
        {
            switch (con.ConstraintType)
            {
                case ConstraintType.PrimaryKey:
                    PrimaryKey = con;
                    break;
                case ConstraintType.ForeignKey:
                    ForeignKeys.Add(con);
                    break;
                case ConstraintType.UniqueKey:
                    UniqueKeys.Add(con);
                    break;
                case ConstraintType.Check:
                    CheckConstraints.Add(con);
                    break;
            }
            AddConstraintColumns(con);
        }

        private void AddConstraintColumns(DatabaseConstraint con)
        {
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
        /// Gets the foreign key children.
        /// </summary>
        public List<DatabaseTable> ForeignKeyChildren { get { return _foreignKeyChildren;  } }

        /// <summary>
        /// Gets the triggers.
        /// </summary>
        public List<DatabaseTrigger> Triggers { get { return _triggers;  } }

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
            return Columns.Find(delegate(DatabaseColumn col) { return col.Name.Equals(name, StringComparison.OrdinalIgnoreCase); });
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
        /// Gets a value indicating whether this instance has an identity column.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has an identity column; otherwise, <c>false</c>.
        /// </value>
        public bool HasIdentityColumn
        {
            get
            {
                return Columns.Any(x => x.IsIdentity);
            }
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
