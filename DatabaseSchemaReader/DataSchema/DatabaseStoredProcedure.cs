using System;
using System.Collections.Generic;

namespace DatabaseSchemaReader.DataSchema
{
    [Serializable]
    public class DatabaseStoredProcedure
    {
        public DatabaseStoredProcedure()
        {
            Arguments = new List<DatabaseArgument>();
            ResultSets = new List<DatabaseResultSet>();
        }

        public string Name { get; set; }

        public string SchemaOwner { get; set; }

        public string Package { get; set; }

        public string Sql { get; set; }

        public List<DatabaseArgument> Arguments { get; private set; }
        public IList<DatabaseResultSet> ResultSets { get; private set; }

        public DatabaseSchema DatabaseSchema { get; set; }

        public string NetName { get; set; }

        public override string ToString()
        {
            return Name;
        }

    }
}
