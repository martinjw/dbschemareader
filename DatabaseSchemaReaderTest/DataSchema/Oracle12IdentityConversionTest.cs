using System.Data;
using System.Globalization;
using DatabaseSchemaReader.Conversion;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.DataSchema
{
    [TestClass]
    public class Oracle12IdentityConversionTest
    {
        [TestMethod]
        public void ParseOracle12Identity()
        {
            //arrange
            var table = new DatabaseSchema(null, SqlType.Oracle)
                .AddTable("Demo")
                .AddColumn("ID", DbType.Int32).AddPrimaryKey()
                .AddColumn("NAME").Table;

            var dataTable = new DataTable("Name") { Locale = CultureInfo.InvariantCulture };
            dataTable.Columns.Add("TableName");
            dataTable.Columns.Add("ColumnName");
            dataTable.Columns.Add("IDENTITY_OPTIONS");
            dataTable.Columns.Add("GENERATION_TYPE");
            dataTable.Rows.Add(new object[] {"Demo", "ID", 
                "START WITH: 1, INCREMENT BY: 1, MAX_VALUE: 9999999999999999999999999999, MIN_VALUE: 1, CYCLE_FLAG: N, CACHE_SIZE: 20, ORDER_FLAG: N",
                "BY DEFAULT"});

            //act
            SchemaConstraintConverter.AddIdentity(dataTable, table);
            var id = table.FindColumn("ID");

            //assert
            Assert.IsTrue(table.HasAutoNumberColumn);
            Assert.IsTrue(id.IsAutoNumber);
            Assert.AreEqual(1, id.IdentityDefinition.IdentitySeed);
            Assert.AreEqual(1, id.IdentityDefinition.IdentityIncrement);
            Assert.IsTrue(id.IdentityDefinition.IdentityByDefault);
        }
    }
}
