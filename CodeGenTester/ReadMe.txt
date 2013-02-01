These are test harnesses for the DatabaseSchemaReader's code generation functionality.

CodeGenTester is a console that reads from a SqlExpress Northwind database, generates projects and builds the assemblies for 3 configurations: NHibernate hbm and fluent, and EF Code First.

It normally writes to %TEMP% but change the AppSetting "Destination" to change it.
If it points to this folder, it will try to build and execute the second console project, CodeGen.TestRunner.

CodeGen.TestRunner is in a solution with the 3 new projects and the correct Nuget packages. It is built and then executes code against the database (to read the Categories table) via the 3 projects.
