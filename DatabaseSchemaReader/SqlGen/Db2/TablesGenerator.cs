using System.Collections.Generic;
using System.Text;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.Db2
{

    class TablesGenerator : TablesGeneratorBase
    {
        public TablesGenerator(DatabaseSchema schema)
            : base(schema)
        {
            //we do some preprocessing here in case we are doing a conversion
            EnsureUniqueIndexNames();
        }

        private void EnsureUniqueIndexNames()
        {
            var indexes = new List<string>();
            foreach (var table in Schema.Tables)
            {
                foreach (var index in table.Indexes)
                {
                    var indexName = index.Name;
                    if (indexes.Contains(indexName))
                    {
                        //index names must be unique schema-wide. In other RDBMSs it's common to be table-scoped.
                        index.Name = table.Name + indexName;
                        continue;
                    }
                    indexes.Add(indexName);
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
