using System;
using System.Data.Common;
using System.Transactions;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using DatabaseSchemaReader.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.Migrations
{
    [TestClass]
    public class MigrationSqlServerTest
    {
        private const string ProviderName = "System.Data.SqlClient";
        readonly string _connectionString = ConnectionStrings.Northwind;
        private readonly DbProviderFactory _factory;

        public MigrationSqlServerTest()
        {
            _factory = DbProviderFactories.GetFactory(ProviderName);
        }

        [TestMethod]
        public void TestMigration()
        {
            //arrange
            var tableName = MigrationCommon.FindFreeTableName(ProviderName, _connectionString);
            var migration = new DdlGeneratorFactory(SqlType.SqlServer).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable(tableName);
            var newColumn = MigrationCommon.CreateNewColumn();
            //this creates a nullable column with no default. Normally we automatically create a default.
            //ensure it is nullable, as we don't want to create a default which we can't delete
            newColumn.Nullable = true;
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
            var statements = ScriptTools.SplitScript(createTable);


            //we need to strip out the "GO" parts from these scripts

            using (new TransactionScope())
            {
                using (var con = _factory.CreateConnection())
                {
                    con.ConnectionString = _connectionString;
                    using (var cmd = con.CreateCommand())
                    {
                        con.Open();
                        foreach (var statement in statements)
                        {
                            //ignore the drop table bit, which has no useful commands
                            if (statement.Contains(Environment.NewLine + "-- DROP TABLE")) continue;
                            Console.WriteLine("Executing " + statement);
                            cmd.CommandText = statement;
                            cmd.ExecuteNonQuery();
                        }

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

        private static void Execute(DbCommand cmd, string statements)
        {
            foreach (var statement in ScriptTools.SplitScript(statements))
            {
                Console.WriteLine("Executing " + statement);
                cmd.CommandText = statement;
                cmd.ExecuteNonQuery();
            }
        }
    }
}
