using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DatabaseSchemaReader.DataSchema
{
    [Serializable]
    public class DatabaseIndex
    {
        #region Fields
        //backing fields
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly List<DatabaseColumn> _columns;
        #endregion

        
        public DatabaseIndex()
        {
            _columns = new List<DatabaseColumn>();
        }

        public string Name { get; set; }

        public string SchemaOwner { get; set; }

        public string TableName { get; set; }

        public string IndexType { get; set; }

        public List<DatabaseColumn> Columns { get { return _columns; } }

        public override string ToString()
        {
            return Name + " on " + TableName;
        }

    }
}
