using System.Linq;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen
{
    [TestClass]
    public class SchemaExtensionsTest
    {
        private readonly DatabaseSchema _schema;

        public SchemaExtensionsTest()
        {
            var schema = new DatabaseSchema(null, SqlType.SqlServer);
            schema.AddTable("Category").AddColumn<int>("Id").AddPrimaryKey()
                .AddColumn<string>("Name").AddLength(10);
            schema.AddTable("Warehouse").AddColumn<int>("Id").AddPrimaryKey()
                .AddColumn<string>("Address").AddLength(100);
            schema.AddTable("Product").AddColumn<int>("Id").AddPrimaryKey()
                .AddColumn<string>("Name").AddLength(20).AddIndex("Idx_Name")
                .AddColumn<int>("CategoryId")
                .AddIndex("Idx_CatId")
                .AddForeignKey("FkProduct_Category", "Category")
                .AddColumn<int>("WarehouseId")
                .AddIndex("Idx_WhId")
                .AddForeignKey("FkProduct_Warehouse", "Warehouse");
            _schema = schema;
        }

        [TestMethod]
        public void TestCreateTables()
        {
            var sql = _schema.ToSqlCreateTables();
            var sql2 = _schema.ToSqlCreateTables(new SqlGenerationParameters { EscapeNames = false, IncludeSchema = false, UseGranularBatching = true });

            Assert.IsNotNull(sql);
            Assert.AreNotEqual(sql, sql2, "The parameters have altered the sql");
        }

        [TestMethod]
        public void TestCreateTable()
        {
            var table = _schema.FindTableByName("Product");
            var sql = table.ToSqlCreateTable();
            var sql2 = table.ToSqlCreateTable(new SqlGenerationParameters { EscapeNames = false, IncludeSchema = false, UseGranularBatching = true });

            Assert.IsNotNull(sql);
            Assert.AreNotEqual(sql, sql2, "The parameters have altered the sql");
        }

        [TestMethod]
        public void TestCreateForeignKeys()
        {
            var table = _schema.FindTableByName("Product");
            var sql = table.ToSqlCreateForeignKeys();
            var sql2 = table.ToSqlCreateForeignKeys(new SqlGenerationParameters { EscapeNames = false, IncludeSchema = false, UseGranularBatching = true });

            Assert.IsNotNull(sql);
            Assert.AreNotEqual(sql, sql2, "The parameters have altered the sql");
        }

        [TestMethod]
        public void TestSelect()
        {
            var table = _schema.FindTableByName("Product");
            var sql = table.ToSqlSelectById(false);
            Assert.IsNotNull(sql);
        }

        [TestMethod]
        public void TestSelectPaged()
        {
            var table = _schema.FindTableByName("Product");
            var sql = table.ToSqlSelectPaged(false);
            Assert.IsNotNull(sql);
        }

        [TestMethod]
        public void TestSelectInsert()
        {
            var table = _schema.FindTableByName("Product");
            var sql = table.ToSqlSelectInsert(false);
            Assert.IsNotNull(sql);
        }

        [TestMethod]
        public void TestSelectUpdate()
        {
            var table = _schema.FindTableByName("Product");
            var sql = table.ToSqlSelectUpdate(false);
            Assert.IsNotNull(sql);
        }

        [TestMethod]
        public void TestNetClass()
        {
            var table = _schema.FindTableByName("Product");
            var code = table.ToClass();
            Assert.IsNotNull(code);
        }


        [TestMethod]
        public void TestAddColumn()
        {
            var table = _schema.FindTableByName("Product");
            var column = table.FindColumn("Name");
            var sql = column.ToSqlAddColumn();
            Assert.IsNotNull(sql);
        }

        [TestMethod]
        public void TestDropColumn()
        {
            var table = _schema.FindTableByName("Product");
            var column = table.FindColumn("Name");
            var sql = column.ToSqlDropColumn();
            Assert.IsNotNull(sql);
        }


        [TestMethod]
        public void TestAddConstraint()
        {
            var table = _schema.FindTableByName("Product");
            var constraint = table.ForeignKeys.First();
            var sql = constraint.ToSqlAddConstraint(table);
            var sql2 = constraint.ToSqlAddConstraint(table, new SqlGenerationParameters { UseGranularBatching = true});
            Assert.IsNotNull(sql);
            Assert.IsTrue(sql2.Trim().EndsWith("GO"));
        }
    }
}