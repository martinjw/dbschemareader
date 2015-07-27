These are test harnesses for the DatabaseSchemaReader's code generation functionality.

0. Ensure you have a SQLExpress Northwind database to test against (or you can change the connection strings)
1. Change the CodeGenTester\app.config appsetting Destination to point to this directory
2. Run build.bat.

Details
-------

CodeGenTester is a console that reads from a SqlExpress Northwind database, generates projects and builds the assemblies for 3 configurations: NHibernate hbm and fluent, and EF Code First.

It normally writes to %TEMP% but change the AppSetting "Destination" to change it.
If it points to this folder, it will try to build and execute the second console project, CodeGen.TestRunner.

CodeGen.TestRunner is in a solution with the 3 new projects and the correct Nuget packages. It is built and then executes code against the database (to read the Categories table) via the 3 projects.

The .bat file runs the msbuild file which builds and runs CodeGenTester.