using System;
using System.Data;
using System.Globalization;
using DatabaseSchemaReader;
using DatabaseSchemaReader.Data;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReaderTest.IntegrationTests;
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

namespace DatabaseSchemaReaderTest.SqlGen.InsertWriterTests
{
    [TestClass]
    public class InsertWriterTest
    {
        [TestMethod]
        public void TestSqlServerInsert()
        {
            //arrange
            var table = new DatabaseTable();
            table.Name = "Categories";
            table.Columns.Add(new DatabaseColumn { Name = "Id", DbDataType = "INTEGER" });
            table.Columns.Add(new DatabaseColumn { Name = "Name", DbDataType = "VARCHAR" });

            var dt = new DataTable { Locale = CultureInfo.InvariantCulture };
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Name", typeof(string));
            dt.Rows.Add(1, "Hello");

            var insertWriter = new InsertWriter(table, dt);
            insertWriter.IncludeIdentity = true;

            //act
            string txt = insertWriter.Write(SqlType.SqlServer);
            //we don't care about formatting
            txt = txt.Replace(Environment.NewLine, string.Empty);

            //assert
            Assert.AreEqual("INSERT INTO [Categories] (  [Id],  [Name]) VALUES (1 ,N'Hello');", txt);
        }

        [TestMethod]
        public void TestOracleInsert()
        {
            //arrange
            var table = new DatabaseTable();
            table.Name = "Categories";
            table.Columns.Add(new DatabaseColumn { Name = "Id", DbDataType = "INTEGER" });
            table.Columns.Add(new DatabaseColumn { Name = "Name", DbDataType = "VARCHAR" });

            var dt = new DataTable { Locale = CultureInfo.InvariantCulture };
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Name", typeof(string));
            dt.Rows.Add(1, "Hello");

            var insertWriter = new InsertWriter(table, dt);
            insertWriter.IncludeIdentity = true;

            //act
            string txt = insertWriter.Write(SqlType.Oracle);
            //we don't care about formatting
            txt = txt.Replace(Environment.NewLine, string.Empty);

            //assert
            Assert.AreEqual("INSERT INTO \"Categories\" (  \"Id\",  \"Name\") VALUES (1 ,'Hello');", txt);
        }

        [TestMethod]
        public void TestSqlServerInsertExcludeIdentity()
        {
            //arrange
            var table = new DatabaseTable();
            table.Name = "Categories";
            table.Columns.Add(new DatabaseColumn { Name = "Id", DbDataType = "INTEGER", IsIdentity = true });
            table.Columns.Add(new DatabaseColumn { Name = "Name", DbDataType = "VARCHAR" });

            var dt = new DataTable { Locale = CultureInfo.InvariantCulture };
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Name", typeof(string));
            dt.Rows.Add(1, "Hello");

            var insertWriter = new InsertWriter(table, dt);
            insertWriter.IncludeIdentity = false;

            //act
            string txt = insertWriter.Write(SqlType.SqlServer);
            //we don't care about formatting
            txt = txt.Replace(Environment.NewLine, string.Empty);

            //assert
            Assert.AreEqual("INSERT INTO [Categories] (  [Name]) VALUES (N'Hello');", txt);
        }

        [TestMethod]
        public void TestSqlServerInsertIncludeIdentity()
        {
            //arrange
            var table = new DatabaseTable();
            table.Name = "Categories";
            table.Columns.Add(new DatabaseColumn { Name = "Id", DbDataType = "INTEGER", IsIdentity = true });
            table.Columns.Add(new DatabaseColumn { Name = "Name", DbDataType = "VARCHAR" });

            var dt = new DataTable { Locale = CultureInfo.InvariantCulture };
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Name", typeof(string));
            dt.Rows.Add(1, "Hello");

            var insertWriter = new InsertWriter(table, dt);
            insertWriter.IncludeIdentity = true;

            //act
            string txt = insertWriter.Write(SqlType.SqlServer);
            //we don't care about formatting
            txt = txt.Replace(Environment.NewLine, string.Empty);

            //assert
            Assert.IsTrue(txt.Contains("INSERT INTO [Categories] (  [Id],  [Name]) VALUES (1 ,N'Hello');"));
            Assert.IsTrue(txt.StartsWith("SET IDENTITY_INSERT [Categories] ON", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(txt.Contains("SET IDENTITY_INSERT [Categories] OFF"));
            Assert.IsTrue(txt.Contains("DBCC CHECKIDENT ([Categories])"));
        }

        [TestMethod]
        public void TestOracleInsertExcludeIdentity()
        {
            //arrange
            var table = new DatabaseTable();
            table.Name = "Categories";
            table.Columns.Add(new DatabaseColumn { Name = "Id", DbDataType = "INTEGER", IsIdentity = true });
            table.Columns.Add(new DatabaseColumn { Name = "Name", DbDataType = "VARCHAR" });

            var dt = new DataTable { Locale = CultureInfo.InvariantCulture };
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("Name", typeof(string));
            dt.Rows.Add(1, "Hello");

            var insertWriter = new InsertWriter(table, dt);
            insertWriter.IncludeIdentity = false;

            //act
            string txt = insertWriter.Write(SqlType.Oracle);
            //we don't care about formatting
            txt = txt.Replace(Environment.NewLine, string.Empty);

            //assert
            Assert.AreEqual("INSERT INTO \"Categories\" (  \"Name\") VALUES ('Hello');", txt);
        }


        const string Providername = "System.Data.SqlClient";
        const string ConnectionString = ConnectionStrings.Northwind;

        private static DatabaseReader GetNortwindReader()
        {
            ProviderChecker.Check(Providername, ConnectionString);

            return new DatabaseReader(ConnectionString, Providername);
        }

        [TestMethod]
        public void TestInsertIntegration()
        {
            //arrange
            var dbReader = GetNortwindReader();
            var table = dbReader.Table("Orders");

            var rdr = new Reader(table, ConnectionString, Providername);
            var dt = rdr.Read();

            var insertWriter = new InsertWriter(table, dt);
            insertWriter.IncludeIdentity = true;

            //act
            string txt = insertWriter.Write(SqlType.SqlServer);

            //assert
            Console.WriteLine(txt);
            //check this manually
        }
    }
}
