using System.Text;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.SqlServer
{
    class TablesGenerator : TablesGeneratorBase
    {
        public TablesGenerator(DatabaseSchema schema) : base(schema)
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

        protected override void WriteDrops(StringBuilder sb)
        {
            foreach (var table in Schema.Tables)
            {
                foreach (var foreignKey in table.ForeignKeys)
                {
                    sb.AppendLine("-- ALTER TABLE " + StringEscaper.Escape(table.Name) + " DROP FOREIGN KEY " + foreignKey.Name + ";");
                    
                }
            } 
            foreach (var table in Schema.Tables)
            {
                sb.AppendLine("-- DROP TABLE " + StringEscaper.Escape(table.Name) + " CASCADE CONSTRAINTS;");
            }
        }
    }
}
