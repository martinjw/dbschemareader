using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.SqlServerCe
{
    class TablesGenerator : SqlServer.TablesGenerator
    {
        public TablesGenerator(DatabaseSchema schema)
            : base(schema)
        {
        }
        protected override ITableGenerator LoadTableGenerator(DatabaseTable table)
        {
            return new TableGenerator(table);
        }
    }
}
