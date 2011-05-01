using System;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using DatabaseSchemaReader.Utilities;
using DatabaseSchemaReaderTest.IntegrationTests;

namespace DatabaseSchemaReaderTest.SqlGen
{
    class MigrationCommon
    {
        public static string FindFreeTableName(string providerName, string connectionString)
        {
            ProviderChecker.Check(providerName, connectionString);

            var dbReader = new DatabaseReader(connectionString, providerName);
            var tables = dbReader.TableList();
            //find an unused table name. 
            const string tableName = "TESTDSR";
            var suffix = string.Empty;
            var i = 0;
            while (tables.Any(t => t.Name.Equals(tableName + suffix, StringComparison.OrdinalIgnoreCase)))
            {
                i++;
                suffix = i.ToString(CultureInfo.InvariantCulture);
            }
            return tableName + suffix;
        }

        public static DatabaseTable CreateTestTable(string tableName)
        {
            //we only need a schema because MySQL foreign key references do not allow just the foreign key table name- they need the columns too
            var schema = new DatabaseSchema(null, null);

            var testTable = new DatabaseTable { Name = tableName };
            schema.Tables.Add(testTable);
            testTable.DatabaseSchema = schema; //the migration will discover this and know how to link the self referencing table

            var intDataType = new DataType("INT", "System.Int32");
            var idColumn = new DatabaseColumn
            {
                Name = "Id",
                DbDataType = "int",
                DataType = intDataType,
                Nullable = false
            };
            testTable.Columns.Add(idColumn);

            var parentColumn = new DatabaseColumn
            {
                Name = "Parent", //for a self-referencing foreign key
                DbDataType = "int",
                DataType = intDataType,
                Nullable = true
            };
            testTable.Columns.Add(parentColumn);

            var nameColumn = new DatabaseColumn
            {
                Name = "NAME",
                DbDataType = "VARCHAR",
                Length = 10,
                DataType = new DataType("VARCHAR", "string")
            };
            testTable.Columns.Add(nameColumn);

            var primaryKey = new DatabaseConstraint { ConstraintType = ConstraintType.PrimaryKey, Name = "PK_" + tableName };
            primaryKey.Columns.Add("Id");
            testTable.PrimaryKey = primaryKey;

            return testTable;
        }

        public static DatabaseColumn CreateNewColumn()
        {
            return new DatabaseColumn
            {
                Name = "COUNTRY",
                DbDataType = "VARCHAR",
                Length = 20,
                DataType = new DataType("VARCHAR", "string"),
                Nullable = true
            };
        }

        public static DatabaseConstraint CreateUniqueConstraint(DatabaseColumn column)
        {
            var constraint = new DatabaseConstraint
                                         {
                                             Name = "UK_COUNTRY",
                                             ConstraintType = ConstraintType.UniqueKey,
                                         };
            constraint.Columns.Add(column.Name);
            return constraint;
        }

        public static DatabaseConstraint CreateForeignKey(DatabaseTable databaseTable)
        {
            var constraint = new DatabaseConstraint
            {
                Name = "FK_" + databaseTable.Name,
                ConstraintType = ConstraintType.ForeignKey,
                RefersToTable = databaseTable.Name
            };
            constraint.Columns.Add("Parent");
            return constraint;
        }

        public static void ExecuteScripts(string providerName, string connectionString, string tableName, IMigrationGenerator migrationGenerator)
        {
            var table = CreateTestTable(tableName);
            var newColumn = CreateNewColumn();
            var unqiueConstraint = CreateUniqueConstraint(newColumn);
            var fk = CreateForeignKey(table);

            var createTable = migrationGenerator.CreateTable(table);
            var addColumn = migrationGenerator.AddColumn(table, newColumn);
            var addUniqueConstraint = migrationGenerator.AddConstraint(table, unqiueConstraint);
            var addForeignKey = migrationGenerator.AddConstraint(table, fk);

            var dropForeignKey = migrationGenerator.DropConstraint(table, fk);
            var dropUniqueConstraint = migrationGenerator.DropConstraint(table, unqiueConstraint);
            var dropColumn = migrationGenerator.DropColumn(table, newColumn);
            var dropTable = migrationGenerator.DropTable(table);


            var factory = DbProviderFactories.GetFactory(providerName);
            using (var con = factory.CreateConnection())
            {
                con.ConnectionString = connectionString;
                using (var cmd = con.CreateCommand())
                {
                    con.Open();
                    using (var tx = con.BeginTransaction())
                    {
                        cmd.Transaction = tx;
                        foreach (var statement in ScriptTools.SplitBySemiColon(createTable))
                        {
                            Console.WriteLine("Executing " + statement);
                            cmd.CommandText = statement;
                            cmd.ExecuteNonQuery();
                        }

                        foreach (var statement in ScriptTools.SplitBySemiColon(addColumn))
                        {
                            Console.WriteLine("Executing " + statement);
                            cmd.CommandText = statement;
                            cmd.ExecuteNonQuery();
                        }

                        foreach (var statement in ScriptTools.SplitBySemiColon(addUniqueConstraint))
                        {
                            Console.WriteLine("Executing " + statement);
                            cmd.CommandText = statement;
                            cmd.ExecuteNonQuery();
                        }

                        foreach (var statement in ScriptTools.SplitBySemiColon(addForeignKey))
                        {
                            Console.WriteLine("Executing " + statement);
                            cmd.CommandText = statement;
                            cmd.ExecuteNonQuery();
                        }

                        foreach (var statement in ScriptTools.SplitBySemiColon(dropForeignKey))
                        {
                            Console.WriteLine("Executing " + statement);
                            cmd.CommandText = statement;
                            cmd.ExecuteNonQuery();
                        }

                        foreach (var statement in ScriptTools.SplitBySemiColon(dropUniqueConstraint))
                        {
                            Console.WriteLine("Executing " + statement);
                            cmd.CommandText = statement;
                            cmd.ExecuteNonQuery();
                        }

                        foreach (var statement in ScriptTools.SplitBySemiColon(dropColumn))
                        {
                            Console.WriteLine("Executing " + statement);
                            cmd.CommandText = statement;
                            cmd.ExecuteNonQuery();
                        }

                        foreach (var statement in ScriptTools.SplitBySemiColon(dropTable))
                        {
                            Console.WriteLine("Executing " + statement);
                            cmd.CommandText = statement;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

    }
}
