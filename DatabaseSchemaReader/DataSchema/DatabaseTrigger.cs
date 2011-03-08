using System;

namespace DatabaseSchemaReader.DataSchema
{
    [Serializable]
    public class DatabaseTrigger
    {
        public DatabaseSchema DatabaseSchema { get; set; }

        public string Name { get; set; }

        public string SchemaOwner { get; set; }

        public string TableName { get; set; }

        public string TriggerBody { get; set; }

        public string TriggerEvent { get; set; }

        public override string ToString()
        {
            return Name + " on " + TableName;
        }
    }
}
