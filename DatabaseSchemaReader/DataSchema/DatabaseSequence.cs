
namespace DatabaseSchemaReader.DataSchema
{
    public class DatabaseSequence
    {
        public string Name { get; set; }
        public string SchemaOwner { get; set; }
        public decimal? MininumValue { get; set; }
        public decimal? MaximumValue { get; set; }
        public int IncrementBy { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
