using System;
using System.Data;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.Migrations.UnitTests
{
    [TestClass]
    public class MigrationAddColumnDefaults
    {

        [TestMethod]
        public void TestPostgreSqlColumnDefaults()
        {
            //MigrationGenerator does not output default values for columns
            //https://github.com/martinjw/dbschemareader/issues/11


            //arrange
            var gen = new DdlGeneratorFactory(SqlType.PostgreSql).MigrationGenerator();
            gen.IncludeSchema = false;

            var pkSeqName = "Seq_PK_Generator";
            var pkSeq = new DatabaseSequence()
            {
                Name = pkSeqName,
                MinimumValue = 1,
                IncrementBy = 1,
            };

            //gen.AddSequence(pkSeq).Replace(";", " CACHE;");

            var newTable = new DatabaseTable { Name = "TestTable" };

            var idColumn = newTable.AddColumn("Id", DbType.Int64);
            idColumn.AddPrimaryKey("PK_TestTable");
            idColumn.DefaultValue = $"nextval('{pkSeqName}')";
            var summaryColumn = newTable.AddColumn("Summary", DbType.String);
            summaryColumn.Length = 100;

            //act
            var ddl = gen.AddTable(newTable);

            //assert

            //expected
            /*
CREATE SEQUENCE "Seq_PK_Generator" INCREMENT BY 1 MINVALUE 1 CACHE;

CREATE TABLE "TestTable"
(
  "Id" BIGINT NOT NULL, --default value missing
  "Summary" VARCHAR (100)  NOT NULL
);
ALTER TABLE "TestTable" ADD CONSTRAINT "PK_TestTable" PRIMARY KEY ("Id");
             */

            Assert.IsTrue(ddl.IndexOf("BIGINT NOT NULL DEFAULT nextval('Seq_PK_Generator'),", StringComparison.OrdinalIgnoreCase) != -1, "default value should be included");
        }
    }
}
