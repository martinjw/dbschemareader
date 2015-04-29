See http://dbschemareader.codeplex.com/ for source code, documentation and GUIs.

===General===

Since .Net 2.0, we can obtain schema information from databases such as tables, columns and stored procedures. This is implemented through the DbConnection.GetSchema method. The ADO providers are inconsistent and inconvienent (DataTables...), so this library is an adapter which loads those collections into simple collections of plain old CLR objects, which are the same for all providers. It also obtains additional information using the database metadata directly.

Supported databases include SqlServer, SqlServer Ce, Oracle (via Microsoft, ODP and Devart), MySQL, SQLite, Postgresql, DB2, Ingres, VistaDb and Sybase ASE/ASA/UltraLite.

===Use===

To use it simply specify the connection string and ADO provider (eg System.Data,SqlClient or System.Data.OracleClient)

const string providername = "System.Data.SqlClient";
const string connectionString = @"Data Source=.\SQLEXPRESS;Integrated Security=true;Initial Catalog=Northwind";
//Create the database reader object.
var dbReader = new DatabaseReader(connectionString, providername);
//for Oracle, specify the Owner (Schema) as the full schema of an Oracle database is huge and will be very slow to load.
//var dbReader = new DatabaseReader("Data Source=XE;User Id=hr;Password=hr;", "System.Data.OracleClient", "HR");
//load the schema (this will take a little time on moderate to large database structures)
var schema = dbReader.ReadAll();

The DatabaseSchema object has a collection of tables, views, stored procedures, functions, packages and datatypes. Tables and views have columns, with their datatypes.

===Code generation===

//first the standard schema reader
const string providername = "System.Data.SqlClient";
const string connectionString = @"Data Source=.\SQLEXPRESS;Integrated Security=true;Initial Catalog=Northwind";
var reader = new DatabaseReader(connectionString, providername);
//for Oracle, specify dbReader.Owner = "MyOwner";
var schema = reader.ReadAll();

//now write the code
var directory = new DirectoryInfo(Environment.CurrentDirectory);
var settings = new CodeWriterSettings
				   {
					   Namespace = "Northwind.Domain",
					   //CodeTarget = CodeTarget.Poco //default is POCO, or use EF Code First/NHibernate
				   };
var codeWriter = new CodeWriter(schema, settings);
codeWriter.Execute(directory);

===SQL generation===
//Simple SQL
var sqlWriter = new SqlWriter(schema.FindTableByName("ORDERS"), SqlType.PostgreSql);
var selectSql = sqlWriter.SelectPageStartToEndRowSql(); //and others...

//Script data INSERTs
var sw = new DatabaseSchemaReader.Data.ScriptWriter {IncludeIdentity = true};
var inserts = sw.ReadTable("ORDERS", connectionString, providername);

===Comparisons===

You can compare the schemas of two databases to get a diff script. 
//load your schemas
var acceptanceDb = new DatabaseReader(connectionString, providername).ReadAll();
var developmentDb = new DatabaseReader(connectionString2, providername).ReadAll();

//compare
var comparison = new CompareSchemas(acceptanceDb, developmentDb);
var script = comparison.Execute(); //script to upgrade acceptanceDb into the same schema as developmentDb.

===Migrations (low level)===
//create a schema model
var dbSchema = new DatabaseSchema(null, SqlType.Oracle);
var table = dbSchema.AddTable("LOOKUP");
table.AddColumn<int>("Id").AddPrimaryKey().AddColumn<string>("Name").AddLength(30);
var newColumn = table.AddColumn("Updated", DbType.DateTime).AddNullable();
//create a migration generator
var factory = new DatabaseSchemaReader.SqlGen.DdlGeneratorFactory(SqlType.Oracle);
var migrations = factory.MigrationGenerator();
//turn the model into scripts
var tableScript = migrations.AddTable(table);
var columnScript = migrations.AddColumn(table, newColumn);
