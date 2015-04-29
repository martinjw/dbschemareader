using System.Text;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.PostgreSql
{
    class TablesGenerator : TablesGeneratorBase
    {
        public TablesGenerator(DatabaseSchema schema)
            : base(schema)
        {
        }

        protected override ConstraintWriterBase LoadConstraintWriter(DatabaseTable table)
        {
            return new ConstraintWriter(table);
        }

        protected override ITableGenerator LoadTableGenerator(DatabaseTable table)
        {
            return new TableGenerator(table);
        }

        protected override ISqlFormatProvider SqlFormatProvider()
        {
            return new SqlFormatProvider();
        }

        protected override void WriteDrops(StringBuilder sb)
        {
            var formatter = SqlFormatProvider();
            foreach (var table in Schema.Tables)
            {
                //there is an implicit 'DELETE FROM fkTable' for foreign keys
                sb.AppendLine("-- DROP TABLE IF EXISTS " + formatter.Escape(table.Name) + " CASCADE;");
            }
        }

    }
}
