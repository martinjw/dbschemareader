using System;
using System.Data.Common;
using System.IO;
using System.Transactions;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using DatabaseSchemaReader.Utilities;
using DatabaseSchemaReaderTest.IntegrationTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.Migrations
{
    [TestClass]
    public class MigrationSqlServerCeTest
    {
        private const string ProviderName = "System.Data.SqlServerCe.4.0";
        private const string FilePath = @"C:\Data\northwind.sdf";

        [TestMethod, TestCategory("SqlServerCe")]
        public void TestMigration()
        {
            //arrange

            DbProviderFactory factory = null;
            try
            {
                factory = DbProviderFactories.GetFactory(ProviderName);
            }
            catch (ArgumentException)
            {
                Assert.Inconclusive("Unable to find System.Data.SqlServerCe.4.0 Data Provider. It may not be installed.");
            }
            if (!File.Exists(FilePath))
            {
                Assert.Inconclusive("SqlServerCe4 test requires database file " + FilePath);
            }

            const string connectionString = "Data Source=\"" + FilePath + "\"";
            ProviderChecker.Check(ProviderName, connectionString);


            var tableName = MigrationCommon.FindFreeTableName(ProviderName, connectionString);
            var migration = new DdlGeneratorFactory(SqlType.SqlServerCe).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable(tableName);
            var newColumn = MigrationCommon.CreateNewColumn();
            var unqiueConstraint = MigrationCommon.CreateUniqueConstraint(newColumn);
            var fk = MigrationCommon.CreateForeignKey(table);
            var index = MigrationCommon.CreateUniqueIndex(newColumn, tableName);

            var createTable = migration.AddTable(table);
            var addColumn = migration.AddColumn(table, newColumn);
            var addUniqueConstraint = migration.AddConstraint(table, unqiueConstraint);
            var addForeignKey = migration.AddConstraint(table, fk);
            var addUniqueIndex = migration.AddIndex(table, index);

            var dropUniqueIndex = migration.DropIndex(table, index);
            var dropForeignKey = migration.DropConstraint(table, fk);
            var dropUniqueConstraint = migration.DropConstraint(table, unqiueConstraint);
            var dropColumn = migration.DropColumn(table, newColumn);
            var dropTable = migration.DropTable(table);


            using (new TransactionScope())
            {
                using (var con = factory.CreateConnection())
                {
                    con.ConnectionString = connectionString;
                    using (var cmd = con.CreateCommand())
                    {
                        con.Open();

                        Execute(cmd, createTable);

                        Execute(cmd, addColumn);

                        Execute(cmd, addUniqueConstraint);

                        Execute(cmd, addForeignKey);


                        Execute(cmd, dropForeignKey);

                        Execute(cmd, dropUniqueConstraint);

                        Execute(cmd, addUniqueIndex);

                        Execute(cmd, dropUniqueIndex);

                        Execute(cmd, dropColumn);

                        Execute(cmd, dropTable);
                    }
                }
            }
        }

        private static void Execute(DbCommand cmd, string sql)
        {
            //we need to strip out the "GO" parts from these scripts AND by ; 
            foreach (var batch in ScriptTools.SplitScript(sql))
            {
                foreach (var statement in ScriptTools.SplitBySemicolon(batch))
                {
                    Console.WriteLine("Executing " + statement);
                    cmd.CommandText = statement;
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
