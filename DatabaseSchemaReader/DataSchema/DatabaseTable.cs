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
    public class DatabaseTable
    {
        #region Fields
        //backing fields and initialize collections
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private DatabaseConstraint _primaryKey;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly List<DatabaseConstraint> _foreignKeys = new List<DatabaseConstraint>();
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly List<DatabaseConstraint> _uniqueKeys = new List<DatabaseConstraint>();
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly List<DatabaseIndex> _indexes = new List<DatabaseIndex>();
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly List<DatabaseConstraint> _checkConstraints = new List<DatabaseConstraint>();
        #endregion

        public DatabaseTable()
        {
            Columns = new List<DatabaseColumn>();
            Triggers = new List<DatabaseTrigger>();
            ForeignKeyChildren = new List<DatabaseTable>();
        }

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

        public string SchemaOwner { get; set; }

        public List<DatabaseColumn> Columns { get; private set; }

        /// <summary>
        /// Gets or sets the primary key column (assuming this isn't a composite key- check PrimaryKey.Columns.Count)
        /// </summary>
        /// <value>The primary key column.</value>
        public DatabaseColumn PrimaryKeyColumn
        {
            get { return Columns.FirstOrDefault(c => c.IsPrimaryKey); }
        }
        #region Constraints
        public DatabaseConstraint PrimaryKey
        {
            get { return _primaryKey; }
            set
            {
                _primaryKey = value;
                AddConstraintColumns(value);
            }
        }
        public List<DatabaseConstraint> ForeignKeys
        {
            get { return _foreignKeys; }
            set { value.ForEach(AddConstraint); }
        }
        public List<DatabaseConstraint> UniqueKeys
        {
            get { return _uniqueKeys; }
            set { value.ForEach(AddConstraint); }
        }
        public List<DatabaseConstraint> CheckConstraints
        {
            get { return _checkConstraints; }
            set { value.ForEach(AddConstraint); }
        }
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

        public IList<DatabaseTable> ForeignKeyChildren { get; private set; }

        public List<DatabaseTrigger> Triggers { get; set; }

        public List<DatabaseIndex> Indexes
        {
            get { return _indexes; }
            set { value.ForEach(AddIndex); }
        }
        public void AddIndex(DatabaseIndex index)
        {
            Indexes.Add(index);
            foreach (KeyValuePair<int, string> kvp in index.Columns)
            {
                string name = kvp.Value;
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

        public bool HasIdentityColumn
        {
            get
            {
                return Columns.Any(x => x.IsIdentity);
            }
        }


        public override string ToString()
        {
            return Name;
        }

    }
}
