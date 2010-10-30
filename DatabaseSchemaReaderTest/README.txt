This test project is a Visual Studio 2008 MsTest project.
You can easily convert it to NUnit:
- delete reference to Microsoft.VisualStudio.QualityTools.UnitTestFramework 
- add references to NUnit
- in properties, add the conditional compilation symbol NUNIT

These tests are INTEGRATION TESTS. They all interact with the database.
This means the databases must exist in order to run it.
*     SqlExpress with Northwind and AdventureWorks (integrated security)
*     Oracle Express with HR (userId HR, password HR)
*     MySQL with Northwind (user id root, passwod mysql)

The tests check the standard databases- HR for Oracle, Adventureworks for SqlServer, Northwind for others (there are some customized versions for MySQL- this uses a simple SqlServer conversion).

There are also inactive tests for Firebird (using their demo database) and SqlLite (using Northwind). 

There are also tests using other ADO providers (accessed using ADO 2's DbProvdierFactories):
* Oracle's ODP (http://www.oracle.com/technetwork/topics/dotnet/index-085163.html), which is free. 
* Devart (http://www.devart.com/dotconnect/oracle/overview1.html) has free Oracle and SqlServer providers as well as licensed ones. I've tested both; they are identical for this functionality.
* DataDirect (http://web.datadirect.com/products/net/index.html) has some trial providers
