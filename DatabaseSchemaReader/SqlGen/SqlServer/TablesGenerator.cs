using System.Collections.Generic;
using System.Text;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.SqlServer
{
    class TablesGenerator : TablesGeneratorBase
    {
        public TablesGenerator(DatabaseSchema schema) : base(schema)
        {
            //we do some preprocessing here in case we are doing a conversion
            EnsureUniqueConstraintNames();
        }

        private void EnsureUniqueConstraintNames()
        {
            var names = new List<string>();
            foreach (var table in Schema.Tables)
            {
                var pk = table.PrimaryKey;
                if (pk != null)
                {
                    var name = pk.Name;
                    if (names.Contains(name))
                    {
                        pk.Name = table.Name + name;
                        continue;
                    }
                    names.Add(name);
                }
                foreach (var fk in table.ForeignKeys)
                {
                    var name = fk.Name;
                    if (names.Contains(name))
                    {
                        fk.Name = table.Name + name;
                        continue;
                    }
                    names.Add(name);
                }
            }
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
            sb.AppendLine(DropTables.Write(Schema, SqlFormatProvider()));
        }
    }
}
