using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
#if !NUNIT
using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
using TestContext = System.Object;
#endif

namespace DatabaseSchemaReaderTest.SqlGen
{
    [TestClass]
    public class SqlWriterTest
    {

        [TestMethod]
        public void TestGeneratedSqlForSelectAll()
        {
            //arrange
            var schema = new DatabaseSchema(null, SqlType.SqlServer);
            schema.AddTable("Category").AddColumn<int>("Id").AddPrimaryKey()
                .AddColumn<string>("FirstName").AddLength(10)
                .Table.SchemaOwner = "first";
            schema.AddTable("Category").AddColumn<int>("Id").AddPrimaryKey()
                .AddColumn<string>("SecondName").AddLength(20)
                .Table.SchemaOwner = "second";
            var table = schema.FindTableByName("Category"); //this will find one of them
            var writer = new SqlWriter(table, SqlType.SqlServer);

            //act
            var sql = writer.SelectAllSql();

            //assert

        }
    }
}
