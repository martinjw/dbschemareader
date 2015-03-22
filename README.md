# DatabaseSchemaReader

A simple, cross-database facade over .Net 2.0 DbProviderFactories to read database metadata.

Any ADO provider can be read  (SqlServer, SqlServer CE 4, MySQL, SQLite, System.Data.OracleClient, ODP, Devart, PostgreSql, DB2...) into a single standard model.

https://github.com/martinjw/dbschemareader or https://dbschemareader.codeplex.com/

https://dbschemareader.codeplex.com/documentation

Nuget: Install-Package DatabaseSchemaReader https://img.shields.io/nuget/v/DatabaseSchemaReader.svg https://www.nuget.org/packages/DatabaseSchemaReader/

* Database schema read from most ADO providers
* Simple .net code generation:
  * Generate POCO classes for tables, and NHibernate or EF Code First mapping files
  * Generate simple ADO classes to use stored procedures
* Simple sql generation:
  * Generate table DDL (and translate to another SQL syntax, eg SqlServer to Oracle or SQLite)
  * Generate CRUD stored procedures (for SqlServer, Oracle, MySQL, DB2)
* Copy a database schema and data from any provider (SqlServer, Oracle etc) to a new SQLite database (and, with limitations, to SqlServer CE 4)
* Compare two schemas to generate a migration script
* Simple cross-database migrations generator

## Use

```C#
//To use it simply specify the connection string and ADO provider (eg System.Data,SqlClient or System.Data.OracleClient)
const string providername = "System.Data.SqlClient";
const string connectionString = @"Data Source=.\SQLEXPRESS;Integrated Security=true;Initial Catalog=Northwind";

//Create the database reader object.
var dbReader = new DatabaseReader(connectionString, providername);
//For Oracle, you should always specify the Owner (Schema).
//dbReader.Owner = "HR";

//Then load the schema (this will take a little time on moderate to large database structures)
var schema = dbReader.ReadAll();

//There are no datatables, and the structure is identical for all providers.
foreach (var table in schema.Tables)
{
  //do something with your model
}
```

## UIs

There are two simple UIs.

* DatabaseSchemaViewer. It reads all the schema and displays it in a treeview. It also includes options for 
 - code generation, table DDL and stored procedure generation.
 - comparing the schema to another database.

* CopyToSQLite. It reads all the schema and creates a new SQLite database file with the same tables and data. If Sql Server CE 4.0 is detected, it can do the same for that database. These databases do not have the full range of data types as other databases, so creating tables may fail (e.g. SqlServer CE 4 does not have VARCHAR(MAX)). In addition, copying data may violate foreign key constraints (especially for identity primary keys) and will fail.
