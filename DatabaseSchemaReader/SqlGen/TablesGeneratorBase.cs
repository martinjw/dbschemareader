using System.Text;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen
{
    abstract class TablesGeneratorBase : ITablesGenerator
    {
        protected readonly DatabaseSchema Schema;

        protected TablesGeneratorBase(DatabaseSchema schema)
        {
            Schema = schema;
            IncludeSchema = true;
        }

        public bool IncludeSchema { get; set; }

        public string Write()
        {
            var sb = new StringBuilder();

            //do tables
            foreach (var table in Schema.Tables)
            {
                var tableGenerator = LoadTableGenerator(table);
                tableGenerator.IncludeSchema = IncludeSchema;
                sb.AppendLine(tableGenerator.Write());
            }
            sb.AppendLine();

            //now we can do foreign keys
            foreach (var table in Schema.Tables)
            {
                var constraintWriter = LoadConstraintWriter(table);
                //SQLite has no ALTER TABLE support, so this is not supported
                if (constraintWriter == null) continue;
                constraintWriter.IncludeSchema = IncludeSchema;
                sb.AppendLine(constraintWriter.WriteForeignKeys());
            }
            sb.AppendLine();

            //in case we want to delete it again
            WriteDrops(sb);
            sb.AppendLine();

            return sb.ToString();
        }

        protected abstract ConstraintWriterBase LoadConstraintWriter(DatabaseTable table);
        protected abstract ITableGenerator LoadTableGenerator(DatabaseTable table);
        protected abstract ISqlFormatProvider SqlFormatProvider();
        protected abstract void WriteDrops(StringBuilder sb);
    }
}
