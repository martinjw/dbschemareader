using DatabaseSchemaReader.DataSchema;
using System.Collections.Generic;
using System.Text;

namespace DatabaseSchemaReader.SqlGen.SqlServer
{
    class TablesGenerator : TablesGeneratorBase
    {
        public TablesGenerator(DatabaseSchema schema) : base(schema)
        {
            //we do some preprocessing here in case we are doing a conversion
            EnsureUniqueConstraintNames();
            SqlFormatProviderInstance = new SqlFormatProvider();
        }

        public void UseGranularBatching()
        {
            SqlFormatProviderInstance = new BatchingSqlFormatProvider();
            _useGranularBatching = true;
        }

        private bool _useGranularBatching;

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
            var constraintWriter = new ConstraintWriter(table);
            if (_useGranularBatching) constraintWriter.UseGranularBatching();
            return constraintWriter;
        }

        protected override ITableGenerator LoadTableGenerator(DatabaseTable table)
        {
            var tableGenerator = new TableGenerator(table);
            if (_useGranularBatching) tableGenerator.UseGranularBatching();
            return tableGenerator;
        }

        protected ISqlFormatProvider SqlFormatProviderInstance { get; set; }
        protected override ISqlFormatProvider SqlFormatProvider()
        {
            return SqlFormatProviderInstance;
        }

        protected override void WriteDrops(StringBuilder sb)
        {
            sb.AppendLine(DropTables.Write(Schema, SqlFormatProvider(), EscapeNames));
        }
    }
}
