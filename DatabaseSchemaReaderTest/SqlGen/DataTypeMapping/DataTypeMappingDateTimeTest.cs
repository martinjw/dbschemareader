using System.Data;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.DataTypeMapping
{
    [TestClass]
    public class DataTypeMappingDateTimeTest
    {

        [TestMethod]
        public void TestSqlServer()
        {
            var mapper = DataTypeMappingFactory.DataTypeMapper(SqlType.SqlServer);
            var dataType = mapper.Map(DbType.DateTime2);

            //assert
            Assert.AreEqual("DATETIME2", dataType);
        }


        [TestMethod]
        public void TestSqlServerCe()
        {
            var mapper = DataTypeMappingFactory.DataTypeMapper(SqlType.SqlServerCe);
            var dataType = mapper.Map(DbType.DateTime2);

            //assert
            Assert.AreEqual("DATETIME", dataType);
        }

        [TestMethod]
        public void TestOracle()
        {
            var mapper = DataTypeMappingFactory.DataTypeMapper(SqlType.Oracle);
            var dataType = mapper.Map(DbType.DateTime2);

            //assert
            Assert.AreEqual("TIMESTAMP", dataType);
        }


        [TestMethod]
        public void TestMySql()
        {
            var mapper = DataTypeMappingFactory.DataTypeMapper(SqlType.MySql);
            var dataType = mapper.Map(DbType.DateTime2);

            //assert
            Assert.AreEqual("DATETIME", dataType);
        }


        [TestMethod]
        public void TestSqLite()
        {
            var mapper = DataTypeMappingFactory.DataTypeMapper(SqlType.SQLite);
            var dataType = mapper.Map(DbType.DateTime2);

            //assert
            Assert.AreEqual("DATETIME", dataType);
        }


        [TestMethod]
        public void TestDb2()
        {
            var mapper = DataTypeMappingFactory.DataTypeMapper(SqlType.Db2);
            var dataType = mapper.Map(DbType.DateTime2);

            //assert
            Assert.AreEqual("TIMESTAMP", dataType);
        }

        [TestMethod]
        public void TestPostgreSql()
        {
            var mapper = DataTypeMappingFactory.DataTypeMapper(SqlType.PostgreSql);
            var dataType = mapper.Map(DbType.DateTime2);

            //assert
            Assert.AreEqual("TIMESTAMP", dataType);
        }
    }
}
