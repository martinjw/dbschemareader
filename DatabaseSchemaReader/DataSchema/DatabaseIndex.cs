using System;
using System.Collections.Generic;

namespace DatabaseSchemaReader.DataSchema
{
    [Serializable]
    public class DatabaseIndex
    {
        public DatabaseIndex()
        {
            Columns = new SortedDictionary<int, string>();
        }

        public string Name { get; set; }

        public string SchemaOwner { get; set; }

        public string TableName { get; set; }

        public string IndexType { get; set; }

        public SortedDictionary<int, string> Columns { get; set; }

        public override string ToString()
        {
            return Name + " on " + TableName;
        }

    }
}
