using System.Data;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.DataTypeMapping
{
    [TestClass]
    public class DataTypeMappingDecimalTest
    {

        [TestMethod]
        public void TestSqlServerDecimal()
        {
            var mapper = DataTypeMappingFactory.DataTypeMapper(SqlType.SqlServer);
            var dataType = mapper.Map(DbType.Decimal);

            //assert
            Assert.AreEqual("DECIMAL", dataType);
        }


        [TestMethod]
        public void TestSqlServerCeDecimal()
        {
            var mapper = DataTypeMappingFactory.DataTypeMapper(SqlType.SqlServerCe);
            var dataType = mapper.Map(DbType.Decimal);

            //assert
            Assert.AreEqual("DECIMAL", dataType);
        }

        [TestMethod]
        public void TestOracleDecimal()
        {
            var mapper = DataTypeMappingFactory.DataTypeMapper(SqlType.Oracle);
            var dataType = mapper.Map(DbType.Decimal);

            //assert
            Assert.AreEqual("NUMBER", dataType);
        }


        [TestMethod]
        public void TestMySqlDecimal()
        {
            var mapper = DataTypeMappingFactory.DataTypeMapper(SqlType.MySql);
            var dataType = mapper.Map(DbType.Decimal);

            //assert
            Assert.AreEqual("DECIMAL", dataType);
        }


        [TestMethod]
        public void TestSqLiteDecimal()
        {
            var mapper = DataTypeMappingFactory.DataTypeMapper(SqlType.SQLite);
            var dataType = mapper.Map(DbType.Decimal);

            //assert
            Assert.AreEqual("NUMERIC", dataType);
        }


        [TestMethod]
        public void TestDb2Decimal()
        {
            var mapper = DataTypeMappingFactory.DataTypeMapper(SqlType.Db2);
            var dataType = mapper.Map(DbType.Decimal);

            //assert
            Assert.AreEqual("DECIMAL", dataType);
        }

        [TestMethod]
        public void TestPostgreSqlDecimal()
        {
            var mapper = DataTypeMappingFactory.DataTypeMapper(SqlType.PostgreSql);
            var dataType = mapper.Map(DbType.Decimal);

            //assert
            Assert.AreEqual("DECIMAL", dataType);
        }
    }
}
