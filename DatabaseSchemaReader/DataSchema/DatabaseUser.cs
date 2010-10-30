using System;

namespace DatabaseSchemaReader.DataSchema
{
    [Serializable]
    public class DatabaseUser
    {
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
