using System;

namespace DatabaseSchemaReader.DataSchema
{
    [Serializable]
    public partial class DatabaseUser
    {
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
