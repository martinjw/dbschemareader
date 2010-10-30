using System.Collections.Generic;
using System.Diagnostics;

namespace DatabaseSchemaReader.DataSchema
{
    public class DatabasePackage
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<DatabaseStoredProcedure> _procedures = new List<DatabaseStoredProcedure>();

        public string Name { get; set; }
        public string SchemaOwner { get; set; }
        public string Definition { get; set; }
        public string Body { get; set; }
        public List<DatabaseStoredProcedure> StoredProcedures
        {
            get { return _procedures; }
            set { _procedures = value; }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
