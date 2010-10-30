using System;

namespace DatabaseSchemaReader.DataSchema
{
    [Serializable]
    public class DatabaseArgument
    {
        public DatabaseSchema DatabaseSchema { get; set; }
        public string Name { get; set; }
        public string SchemaOwner { get; set; }

        public string ProcedureName { get; set; }
        public string PackageName { get; set; }
        public string DatabaseDataType { get; set; }
        public DataType DataType { get; set; }
        public decimal Ordinal { get; set; }
        public int? Precision { get; set; }
        public int? Scale { get; set; }
        public int? Length { get; set; }
        public bool In { get; set; }
        public bool Out { get; set; }
        public override string ToString()
        {
            return Name;
        }

    }
}
