using System.Data;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.Oracle
{
    [TestClass]
    public class LongIntegerTest
    {
        [TestMethod]
        public void LongIntShouldBeNumber19()
        {
            //#12 MigrationGenerator does not generate correct oracle data type for int64

            //arrange
            var schema = new DatabaseSchema(null, SqlType.SqlServer);
            var newTable = schema.AddTable("TestTable");

            var idColumn = newTable.AddColumn("Id", DbType.Int64);
            idColumn.AddPrimaryKey("PK_TestTable");

            var summaryColumn = newTable.AddColumn("Summary", DbType.String);
            summaryColumn.Length = 100;

            var migration = new DdlGeneratorFactory(SqlType.Oracle).MigrationGenerator();

            //act
            var sql = migration.AddTable(newTable);

            //assert

            Assert.IsTrue(sql.Contains("\"Id\" NUMBER (19) NOT NULL"));
        }
    }
}
