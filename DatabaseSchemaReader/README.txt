From MSDN http://msdn.microsoft.com/en-us/library/kcax58fh.aspx
+Obtaining schema information from a database is accomplished with the process of schema discovery. Schema discovery allows applications to request that managed providers find and return information about the database schema, also known as metadata, of a given database. Different database schema elements such as tables, columns, and stored-procedures are exposed through schema collections. Each schema collection contains a variety of schema information specific to the provider being used.

Each of the .NET Framework managed providers implement the GetSchema method in the Connection class, and the schema information that is returned from the GetSchema method comes in the form of a DataTable. The GetSchema method is an overloaded method that provides optional parameters for specifying the schema collection to return, and restricting the amount of information returned.+

Unfortunately the information is returned in datatables, and the schema collections are different for each provider (SqlServer, Oracle, MySql).

This is an adapter which loads those collections into simple collections of plain old CLR objects, which are the same for all providers. It doesn't try to hide the underlying differences: Oracle will have packages and none of the others will. But the many small differences between tables, columns and stored procedure parameters disappear.

There is no UI (the tests show how to use the class library).

It is not optimized or particularly well designed code. It was originally written in 2005, just after .Net 2.0 came out. It has been updated to .net 3.5 (although you can see the .net 2 vintage of much of it), and the project files are VS 2008 and VS 2010.

Over the years I've used it for code generation of data access code, and for conversions between database platforms. Some of the latter code is included, but there are better solutions for data access code gen (you could still use this library to get the schema data and then use that within T4 templating, for instance).

To use it simply specify the connection string and ADO provider (eg System.Data,SqlClient or System.Data,OracleClient)

const string providername = "System.Data.SqlClient";
const string connectionString = @"Data Source=.\SQLEXPRESS;Integrated Security=true;Initial Catalog=Northwind";


Then load the schema (this will take a little time on moderate to large database structures)

var dbReader = new DatabaseReader(connectionString, providername);
var schema = dbReader.ReadAll();


The DatabaseSchema object has a collection of tables, views, stored procedures, functions, packages and datatypes. Tables and views have columns, with their datatypes.

Unlike the GetSchema method, there are no datatables, and the structure is identical for all providers.

foreach (var table in schema.Tables)
{
      Debug.WriteLine("Table " + table.Name);

      foreach (var column in table.Columns)
      {
            Debug.WriteLine("\tColumn " + column.Name + "\t" + column.DataType.TypeName);
      }
}

It links up all the foreign key tables automatically so you can do this:

if (column.IsForeignKey) Debug.Write("\tForeign key to " + column.ForeignKeyTable.Name);

Instead of loading the entire schema, you can load just tables, or a single table:

var table = dbReader.Table("Products");

Obviously the relations between the table cannot be created if you only load a single table, But even then it reads the constraints as well so you have a lot of information to play with.

There are also rudimentary tools to generate SQL (note this is VERY LIMITED):

var sqlWriter = new SqlWriter(table, DatabaseSchemaReader.DataSchema.SqlType.SqlServer);
var sql = sqlWriter.SelectPageSql(); //paging sql

And you can even generate CRUD stored procedures:

var gen = new DatabaseSchemaReader.SqlGen.SqlServer.ProcedureGenerator(table);
gen.ManualPrefix = table.Name + "__";
var path = Path.Combine(Environment.CurrentDirectory, "sqlserver_sprocs.sql");
gen.WriteToScript(path);

The SQL generation/ conversion utilities are very basic; at best they are a starting point for what you can do with the the DatabaseSchemaReader. 