using System;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen.PostgreSql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.PostgreSql
{
    [TestClass]
    public class TableGeneratorTest
    {
        [TestMethod]
        public void TestPostgreSqlTableWithDefaultValue()
        {
            //arrange
            var schema = new DatabaseSchema(null, SqlType.PostgreSql);
            var table = schema.AddTable("TableWithDefaults")
                .AddColumn<int>("Id").AddIdentity()
                .AddColumn<string>("Name").AddLength(200)
                .AddColumn<int>("SerialNum")
                .AddColumn<int>("Period")
                .AddColumn<DateTime>("CreationDate")
                .Table;
            table.AddColumn<int>("IsBig").AddNullable().DefaultValue = "1";
            table.FindColumn("SerialNum").AddNullable().DefaultValue = "nextval('serial_num_seq')";
            table.FindColumn("CreationDate").AddNullable().DefaultValue = "now()";
            var tableGen = new TableGenerator(table);

            //act
            var ddl = tableGen.Write();

            //assert
            Assert.IsTrue(ddl.Contains("\"IsBig\" INTEGER  DEFAULT 1"));
            Assert.IsTrue(ddl.Contains("\"SerialNum\" INTEGER  DEFAULT nextval('serial_num_seq')"));
            Assert.IsTrue(ddl.Contains("\"CreationDate\" TIMESTAMP  DEFAULT now()"));
        }
    }
}