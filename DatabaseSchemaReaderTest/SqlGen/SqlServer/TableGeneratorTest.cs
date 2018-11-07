using System;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.SqlServer
{
    [TestClass]
    public class TableGeneratorTest
    {
        [TestMethod]
        public void TestSqlServerTableWithComputedColumn()
        {
            //arrange
            var schema = new DatabaseSchema(null, SqlType.SqlServer);
            var table = schema.AddTable("AllTypes")
                  .AddColumn<int>("Id").AddIdentity()
                  .AddColumn<string>("Name").AddLength(200)
                  .AddColumn<int>("Age")
                  .AddColumn<int>("Period")
                  .Table;
            table.AddColumn<int>("ComputedAge").ComputedDefinition = "(Age - Period)";
            var tableGen = new TableGenerator(table);

            //act
            var ddl = tableGen.Write();

            //assert
            Assert.IsTrue(ddl.Contains("[ComputedAge] AS (Age - Period)"));
        }

        [TestMethod]
        public void TestSqlServerTableWithIdentity()
        {
            //arrange
            var schema = new DatabaseSchema(null, SqlType.SqlServer);
            var table = schema.AddTable("Test")
                  .AddColumn<int>("Id").AddIdentity()
                  .AddColumn<string>("Name").AddLength(200)
                  .Table;
            var tableGen = new TableGenerator(table);

            //act
            var ddl = tableGen.Write();

            //assert
            Assert.IsTrue(ddl.Contains("[Id] INT IDENTITY(1,1)  NOT NULL"));
        }

        [TestMethod]
        public void TestSqlServerTableWithSequenceAutoNumber()
        {
            //arrange
            var schema = new DatabaseSchema(null, SqlType.SqlServer);
            var table = schema.AddTable("Test");
            var id = table.AddColumn<int>("Id").AddPrimaryKey();
            id.DefaultValue = "NEXT VALUE FOR [MySequence]";
            id.IsAutoNumber = true; //but id.IdentityDefinition == null
            table.AddColumn<string>("Name").AddLength(200);
            var tableGen = new TableGenerator(table);

            //act
            var ddl = tableGen.Write();

            //assert
            Assert.IsTrue(ddl.Contains("[Id] INT NOT NULL DEFAULT NEXT VALUE FOR [MySequence]"));
        }

        [TestMethod]
        public void TestSqlServerTableWithColumnDescription()
        {
            //arrange
            var schema = new DatabaseSchema(null, SqlType.SqlServer);
            var table = schema.AddTable("Test");
            var id = table.AddColumn<int>("Id").AddPrimaryKey();
            id.Description = "This is the table's primary key";
            table.AddColumn<string>("Name").AddLength(200);
            var tableGen = new TableGenerator(table);

            //act
            var ddl = tableGen.Write();

            //assert
            Assert.IsTrue(ddl.Contains("EXEC sys.sp_addextendedproperty"));
            Assert.IsTrue(ddl.Contains("'This is the table''s primary key'"));
        }

        [TestMethod]
        public void TestSqlServerTableWithTableDescription()
        {
            //arrange
            var schema = new DatabaseSchema(null, SqlType.SqlServer);
            var table = schema.AddTable("Test");
            table.Description = "This is the table's description";
            table.AddColumn<int>("Id").AddPrimaryKey();
            var tableGen = new TableGenerator(table);

            //act
            var ddl = tableGen.Write();

            //assert
            Assert.IsTrue(ddl.Contains("EXEC sys.sp_addextendedproperty"));
            Assert.IsTrue(ddl.Contains("'This is the table''s description'"));
        }

        [TestMethod]
        public void TestSqlServerWithSysDateTime()
        {
            //arrange
            var schema = new DatabaseSchema(null, SqlType.SqlServer);
            var table = schema.AddTable("Test");
            table.AddColumn<int>("Id").AddPrimaryKey();
            // [Update] DATETIME2(3)  NOT NULL DEFAULT (SYSDATETIME()),
            var created = table.AddColumn<DateTime>("Created");
            created.DefaultValue = "(SYSDATETIME())";
            var tableGen = new TableGenerator(table);

            //act
            var ddl = tableGen.Write();

            //assert
            Assert.IsTrue(ddl.Contains("DEFAULT (SYSDATETIME())"));
        }
    }
}
