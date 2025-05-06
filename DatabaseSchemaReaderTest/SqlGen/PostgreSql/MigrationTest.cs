using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.PostgreSql
{
    [TestClass]
    public class MigrationTest
    {
        [TestMethod, TestCategory("Postgresql")]
        public void TestMigration()
        {
            //#197 PostgreSQL: Invalid DROP TRIGGER syntax (not sql standard!)
            var trigger = new DatabaseTrigger
            {
                Name = "TriggerName",
                SchemaOwner = "public",
                TableName = "Table1",
            };

            var factory = new DdlGeneratorFactory(SqlType.PostgreSql);
            var migrator = factory.MigrationGenerator();
            var sql = migrator.DropTrigger(trigger);
            Assert.AreEqual("DROP TRIGGER IF EXISTS \"TriggerName\" ON \"public\".\"Table1\";",sql);
            //should be DROP TRIGGER "your_trigger_name" ON "public"."your_table_name";


            //            const string providername = "Npgsql";
            //            var connectionString = ConnectionStrings.PostgreSql;
            //            ProviderChecker.Check(providername, connectionString);

            //            var dbReader = new DatabaseReader(connectionString, providername);
            //            dbReader.Owner = "public"; //otherwise you have "postgres" owned tables and views
            //            var schema = dbReader.ReadAll();
            //            //exactly the same
            //            var schema2 = dbReader.ReadAll();

            //            var employees = schema2.FindTableByName("employees");
            //            schema2.RemoveTable(employees);

            //            schema2.AddTable("employees")
            //                .AddColumn<int>("employee_id").AddIdentity().AddPrimaryKey("pkEmployees")
            //                .AddColumn<string>("name").AddLength(100)
            //                .AddColumn<int>("manager_id")
            //                .AddForeignKey("manager_id", "employees", "employee_id");

            //            var comparer = new CompareSchemas(schema2, schema);
            //            var migration = comparer.Execute();
            //            Console.WriteLine(migration);

            //            //    Type names are different in postgres and schema reader. e.g. decimal is numeric in postgresql, so dbschemareader always creates migration because it detects change.
            //            //    Identity cannot be set on existing column - there is problem with default value

        }
    }
}
