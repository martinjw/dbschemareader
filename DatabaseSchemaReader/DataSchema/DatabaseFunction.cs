using System.Collections.Generic;
using System.Diagnostics;

namespace DatabaseSchemaReader.DataSchema
{
    public class DatabaseFunction
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<DatabaseArgument> _arguments = new List<DatabaseArgument>();

        public string Name { get; set; }
        public string SchemaOwner { get; set; }
        public string Sql { get; set; }
        public string Package { get; set; }

        public List<DatabaseArgument> Arguments
        {
            get { return _arguments; }
            set { _arguments = value; }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
