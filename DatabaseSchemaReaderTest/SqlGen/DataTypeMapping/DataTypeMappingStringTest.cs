using System.Data;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.DataTypeMapping
{
    [TestClass]
    public class DataTypeMappingStringTest
    {

        [TestMethod]
        public void TestUnicodeStringViaTable()
        {
            var mapper = DataTypeMappingFactory.DataTypeMapper(new DatabaseTable());
            var dataType = mapper.Map(DbType.String);

            //assert
            Assert.AreEqual("NVARCHAR", dataType);
        }
        [TestMethod]
        public void TestUnicodeStringViaTableWithSchema()
        {
            var schema = new DatabaseSchema(null, "System.Data.OracleClient");
            var table = schema.AddTable("Orders");
            var mapper = DataTypeMappingFactory.DataTypeMapper(table);
            var dataType = mapper.Map(DbType.String);

            //assert
            Assert.AreEqual("NVARCHAR2", dataType);
        }

        [TestMethod]
        public void TestSqlServerAnsiString()
        {
            var mapper = DataTypeMappingFactory.DataTypeMapper(SqlType.SqlServer);
            var dataType = mapper.Map(DbType.AnsiString);

            //assert
            Assert.AreEqual("VARCHAR", dataType);
        }

        [TestMethod]
        public void TestSqlServerUnicodeString()
        {
            var mapper = DataTypeMappingFactory.DataTypeMapper(SqlType.SqlServer);
            var dataType = mapper.Map(DbType.String);

            //assert
            Assert.AreEqual("NVARCHAR", dataType);
        }


        [TestMethod]
        public void TestSqlServerCeUnicodeString()
        {
            var mapper = DataTypeMappingFactory.DataTypeMapper(SqlType.SqlServerCe);
            var dataType = mapper.Map(DbType.String);

            //assert
            Assert.AreEqual("NVARCHAR", dataType);
        }

        [TestMethod]
        public void TestOracleAnsiString()
        {
            var mapper = DataTypeMappingFactory.DataTypeMapper(SqlType.Oracle);
            var dataType = mapper.Map(DbType.AnsiString);

            //assert
            Assert.AreEqual("VARCHAR2", dataType);
        }

        [TestMethod]
        public void TestOracleUnicodeString()
        {
            var mapper = DataTypeMappingFactory.DataTypeMapper(SqlType.Oracle);
            var dataType = mapper.Map(DbType.String);

            //assert
            Assert.AreEqual("NVARCHAR2", dataType);
        }


        [TestMethod]
        public void TestMySqlUnicodeString()
        {
            var mapper = DataTypeMappingFactory.DataTypeMapper(SqlType.MySql);
            var dataType = mapper.Map(DbType.String);

            //assert
            Assert.AreEqual("VARCHAR", dataType);
        }


        [TestMethod]
        public void TestSqLiteUnicodeString()
        {
            var mapper = DataTypeMappingFactory.DataTypeMapper(SqlType.SQLite);
            var dataType = mapper.Map(DbType.String);

            //assert
            Assert.AreEqual("TEXT", dataType);
        }


        [TestMethod]
        public void TestDb2UnicodeString()
        {
            var mapper = DataTypeMappingFactory.DataTypeMapper(SqlType.Db2);
            var dataType = mapper.Map(DbType.String);

            //assert
            Assert.AreEqual("VARCHAR", dataType);
        }

        [TestMethod]
        public void TestPostgreSqlUnicodeString()
        {
            var mapper = DataTypeMappingFactory.DataTypeMapper(SqlType.PostgreSql);
            var dataType = mapper.Map(DbType.String);

            //assert
            Assert.AreEqual("VARCHAR", dataType);
        }
    }
}
