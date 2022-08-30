using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlX.XDevAPI.Relational;

namespace DatabaseSchemaReaderTest.SqlGen.PostgreSql
{
    [TestClass]
    public class GeneratorTest
    {
        [TestMethod]
        public void TestGeneratorEscaping()
        {
            //arrange
            var schema = new DatabaseSchema(null, SqlType.PostgreSql);
            var table = schema.AddTable("AllTypes")
                .AddColumn<int>("Id").AddIdentity()
                .AddColumn<string>("Name").AddLength(200)
                .AddColumn<int>("Age")
                .AddColumn<int>("Period")
                .Table;
            var column = table.FindColumn("Name");
            table.AddIndex("TableIndex", new[] {column});
            table.Description = "Test table";
            column.Description = "Name column";

            var factory = new DdlGeneratorFactory(SqlType.PostgreSql);
            var tableGen = factory.TableGenerator(table);
            tableGen.EscapeNames = false;

            //act
            var ddl = tableGen.Write();

            //assert
            Assert.IsTrue(ddl.Contains("INDEX TableIndex ON AllTypes(Name)"));
            Assert.IsTrue(ddl.Contains("COMMENT ON COLUMN AllTypes.Name IS 'Name column';"));
        }

        [TestMethod]
        public void TableWithBooleanType()
        {
            var schema = new DatabaseSchema(null, SqlType.PostgreSql)
                .AddDataTypes(SqlType.PostgreSql);
            var isActive = schema.AddTable("BOOLEANS")
                .AddColumn("ID", "NUMBER(6,0)").AddPrimaryKey()
                .AddColumn("IS_ACTIVE", "Bool");
            isActive.DefaultValue = "TRUE";

            var factory = new DdlGeneratorFactory(SqlType.PostgreSql);
            var tableGen = factory.TableGenerator(isActive.Table);

            //act
            var ddl = tableGen.Write();
            //assert
            Assert.IsTrue(ddl.Contains("\"IS_ACTIVE\" BOOL  NOT NULL DEFAULT TRUE"));
        }
    }
}