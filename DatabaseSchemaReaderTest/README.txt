This test project is a Visual Studio 2015 MsTest project.

Many of these tests are INTEGRATION TESTS. They interact with the database, IO and .net ADO data providers.

All ADO providers are accessed using ADO 2's DbProviderFactories. Although there are Nuget references, the tests and Database Schema Reader do not use the providers directly.
If the provider is not installed (eg Devart.Oracle), the test runs as Inconclusive.
If the database cannot be opened (invalid connection string/ password etc), the test runs as Inconclusive.

If the providers are not available, or the database is not accessible, the tests should return Inconclusive.
Some providers are referenced (via Nuget), but may still fail if the database is not installed.
Database installation scripts are included in the DatabaseScripts folder.

The following databases are used in the tests:
*     SqlExpress with Northwind and AdventureWorks (integrated security)
*     Oracle Express with HR (userId HR, password HR)
         to enable HR in Oracle XE open sqlplus
        > ALTER USER hr ACCOUNT UNLOCK;
        > ALTER USER hr IDENTIFIED BY HR;
*     MySQL with Northwind (user id root, password mysql)
*     Postgresql with world
*     Firebird with Employee.Fdb
*     SQLite with Northwind (database file is Northwind.db, it is created locally by the assemblyInitialize event in InitSQLite)
*     SqlServer CE 4 with Northwind (database file is C:\Data\Northwind.sdf)
*     DB2 on localhost with standard Sample database (user db2admin, password db2)
*     Ingres on localhost with standard demodb database
*     Sybase ASA 15 on localhost with standard pubs3 database
*     Sybase ASE 12 on localhost with standard v12 demo database
*     Sybase UltraLite (v12) with standard custdb database, installed in default Windows 7 directory

The tests check the standard databases- HR for Oracle, Adventureworks for SqlServer, Northwind for others (there are some customized versions for MySQL- this uses a simple SqlServer conversion).

There are also tests using other ADO providers:
* Oracle's ODP (http://www.oracle.com/technetwork/topics/dotnet/index-085163.html), which is free. 
* Devart (http://www.devart.com/dotconnect/oracle/overview1.html) has free Oracle, MySql, PostgreSql and SQLite providers as well as licensed ones. I've tested both free and professional Oracle drivers; they are identical for this functionality.
* DataDirect (http://web.datadirect.com/products/net/index.html) has some trial providers

# AppVeyor
Integration tests for databases apart from SqlServer are identified by TestCategoryAttribute.
See the appveyor.yml file.
