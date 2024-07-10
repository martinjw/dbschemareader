//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;
//using DatabaseSchemaReader;
//using DatabaseSchemaReader.DataSchema;
//using DatabaseSchemaReader.Compare;
//using DatabaseSchemaReaderTest.IntegrationTests;

//namespace DatabaseSchemaReaderTest.SqlGen.PostgreSql
//{
//    [TestClass]
//    public class MigrationTest
//    {
//        [TestMethod, TestCategory("Postgresql")]
//        public void TestMigration()
//        {
//            //    when dropping primary key with FK constraint - commands are in wrong order and migration does not go through. Same applies for unique key

//            //CREATE TABLE employees (
//            //    employee_id SERIAL PRIMARY KEY,
//            //    name VARCHAR(100),
//            //    manager_id INTEGER,
//            //    FOREIGN KEY (manager_id) REFERENCES employees(employee_id)
//            //);
//            var schema = new DatabaseSchema(null, SqlType.PostgreSql);
//            schema.AddTable("employees")
//                .AddColumn<int>("employee_id").AddIdentity().AddPrimaryKey("pkEmployees")
//                .AddColumn<string>("name").AddLength(100)
//                .AddColumn<int>("manager_id")
//                .AddForeignKey("manager_id", "employees", "employee_id");

//            var schema2 = new DatabaseSchema(null, SqlType.PostgreSql);
//            schema2.AddTable("employees")
//                .AddColumn<int>("employee_id")
//                .AddColumn<string>("name").AddLength(100)
//                .AddColumn<int>("manager_id");
//                //.AddForeignKey("manager_id", "employees", "employee_id");

//            var comparer = new CompareSchemas(schema,schema2);
//            var migration = comparer.Execute();
//            Console.WriteLine(migration);

//            //    Type names are different in postgres and schema reader. e.g. decimal is numeric in postgresql, so dbschemareader always creates migration because it detects change.
//            //    Identity cannot be set on existing column - there is problem with default value

//        }

//        [TestMethod, TestCategory("Postgresql")]
//        public void TestColumnTypes()
//        {
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

//        }
//    }
//}
