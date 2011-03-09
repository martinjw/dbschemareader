using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DatabaseSchemaReader.DataSchema
{
    [Serializable]
    public partial class DatabasePackage
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<DatabaseStoredProcedure> _procedures = new List<DatabaseStoredProcedure>();
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<DatabaseFunction> _functions = new List<DatabaseFunction>();

        public DatabasePackage()
        {
            
        }

        public string Name { get; set; }
        public string SchemaOwner { get; set; }
        public string Definition { get; set; }
        public string Body { get; set; }
        public List<DatabaseStoredProcedure> StoredProcedures
        {
            get { return _procedures; }
            set { _procedures = value; }
        }
        public List<DatabaseFunction> Functions
        {
            get { return _functions; }
            set { _functions = value; }
        }
        
        public string NetName { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
