This test project is a Visual Studio 2008 MsTest project.
You can easily convert it to NUnit:
- delete reference to Microsoft.VisualStudio.QualityTools.UnitTestFramework 
- add reference to NUnit.framework
- in properties, add the conditional compilation symbol NUNIT

These tests are INTEGRATION TESTS. They all interact with the database, IO and .net ADO data providers.
This means the databases must exist in order to run it.
*     SqlExpress with Northwind and AdventureWorks (integrated security)
*     Oracle Express with HR (userId HR, password HR)
*     MySQL with Northwind (user id root, passwod mysql)

All ADO providers are accessed using ADO 2's DbProviderFactories - there are no direct references.
If the provider is not installed (eg Devart.Oracle), the test runs as Inconclusive.
If the database cannot be opened (invalid connection string/ password etc), the test runs as Inconclusive.

The tests check the standard databases- HR for Oracle, Adventureworks for SqlServer, Northwind for others (there are some customized versions for MySQL- this uses a simple SqlServer conversion).

There are also tests for Firebird (using their demo database) and SqlLite (using Northwind). 

There are also tests using other ADO providers:
* Oracle's ODP (http://www.oracle.com/technetwork/topics/dotnet/index-085163.html), which is free. 
* Devart (http://www.devart.com/dotconnect/oracle/overview1.html) has free Oracle and SqlServer providers as well as licensed ones. I've tested both; they are identical for this functionality.
* DataDirect (http://web.datadirect.com/products/net/index.html) has some trial providers
