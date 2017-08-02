
namespace DatabaseSchemaReader.ProviderSchemaReaders.ResultModels
{
    class ProcedureSource
    {
        public string SchemaOwner { get; set; }
        public string Name { get; set; }
        public SourceType SourceType { get; set; }
        public string Text { get; set; }

        public override string ToString()
        {
            return $"Source for {SchemaOwner}.{Name} {SourceType}";
        }
    }
}
