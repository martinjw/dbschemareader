using System;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.DataSchema
{
    [TestClass]
    public class PostgreSqlTest
    {
        [TestMethod]
        public void GivenDateTimeOffset()
        {
            //arrange
            var schema = new DatabaseSchema(null, SqlType.PostgreSql);
            schema
                .AddTable("Category")
                .AddColumn<int>("CategoryId").AddPrimaryKey()
               ;

            var table = schema.FindTableByName("Category");

            //act
            var col = table.AddColumn("DateTimeOffset", typeof(DateTimeOffset));

            //assert
            Assert.AreEqual("TIMESTAMP WITH TIME ZONE", col.DbDataType);
        }

        [TestMethod]
        public void GivenTimeSpan()
        {
            //arrange
            var schema = new DatabaseSchema(null, SqlType.PostgreSql);
            schema
                .AddTable("Category")
                .AddColumn<int>("CategoryId").AddPrimaryKey()
               ;

            var table = schema.FindTableByName("Category");

            //act
            var col = table.AddColumn("Timespan", typeof(TimeSpan));

            //assert
            Assert.AreEqual("INTERVAL", col.DbDataType);
        }


        [TestMethod]
        public void GivenByte()
        {
            //arrange
            var schema = new DatabaseSchema(null, SqlType.PostgreSql);
            schema
                .AddTable("Category")
                .AddColumn<int>("CategoryId").AddPrimaryKey()
               ;

            var table = schema.FindTableByName("Category");

            //act
            var col = table.AddColumn("Byte", typeof(Byte));

            //assert
            Assert.AreEqual("NUMBER", col.DbDataType);
            Assert.AreEqual(1, col.Precision);
        }


        [TestMethod]
        public void GivenUnnamedConstraintsThenStandardNamesAssigned()
        {
            //arrange
            var schema = new DatabaseSchema(null, SqlType.PostgreSql);
            schema
                .AddTable("Category")
                .AddColumn<int>("CategoryId").AddPrimaryKey()
                .AddColumn<string>("CategoryName").AddLength(50).AddNullable()

                .AddTable("Product")
                .AddColumn<int>("Id").AddPrimaryKey()
                .AddColumn<string>("ProductName").AddLength(50).AddUniqueKey()
                .AddColumn<int>("CategoryId").AddForeignKey("Category")
               ;
            var table = schema.FindTableByName("Product");
            var ddlGen = new DdlGeneratorFactory(SqlType.PostgreSql);
            var tabGen = ddlGen.TableGenerator(table);

            //act
            var ddl = tabGen.Write();

            //assert
            var hasPrimaryKey =
                ddl.Contains("ALTER TABLE \"Product\" ADD CONSTRAINT \"Product_Id_pkey\" PRIMARY KEY (\"Id\");");
            var hasUniqueKey =
                ddl.Contains("ALTER TABLE \"Product\" ADD CONSTRAINT \"Product_ProductName_key\" UNIQUE (\"ProductName\");");
            Assert.IsTrue(hasPrimaryKey);
            Assert.IsTrue(hasUniqueKey);
        }
    }
}
