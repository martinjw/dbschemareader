using System;

namespace DatabaseSchemaReader.DataSchema
{
    [Serializable]
    public class DatabaseView : DatabaseTable
    {
        public string Sql { get; set; }
        
        public override string ToString()
        {
            return "View: " + base.ToString();
        }
    }


}
