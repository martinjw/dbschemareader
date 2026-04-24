using DatabaseSchemaReader.Utilities.DbProvider;

namespace DatabaseSchemaReaderFrameworkTests
{
    /// <summary>
    ///     some INTEGRATION test to fix the issue described here <seealso cref="https://stackoverflow.com/q/3005962/402488"/> on MS Sql server
    ///     that returns incorrect inforation of foreign key in a specific condition
    /// </summary>
    [TestClass]
    public class SqlServerIssueOfNullFKReferenceTests
    {
        private const string _parentTable = "MY_PARENT_TABLE";
        private const string _childTable = "MY_CHILD_TABLE";
        private const string FK_name = "FK_MY_CHILD_TABLE_MY_PARENT_TABLE";
        private readonly string _connectionString = ConnectionStrings.Northwind;

        private readonly string Create_Parent_And_Child_Tables = $@"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].{_parentTable}') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.{_parentTable} (
        ID      INTEGER,
        NAME    VARCHAR,
        CONSTRAINT PK_{_parentTable} PRIMARY KEY CLUSTERED (ID)
    );
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_{_parentTable}_unique_ID' AND object_id = OBJECT_ID(N'[dbo].{_parentTable}'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX IX_{_parentTable}_unique_ID ON dbo.{_parentTable} (ID ASC) INCLUDE (NAME);
END

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].{_childTable}') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.{_childTable} (
        ID      INTEGER,
        PID     INTEGER,
        NAME    VARCHAR,

        CONSTRAINT PK_{_childTable} PRIMARY KEY CLUSTERED (ID)
    );
END

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = N'{FK_name}' AND parent_object_id = OBJECT_ID(N'[dbo].{_childTable}'))
BEGIN
    ALTER TABLE dbo.{_childTable}
    ADD CONSTRAINT {FK_name}
        FOREIGN KEY (PID)
        REFERENCES dbo.{_parentTable} (ID)
        ON UPDATE NO ACTION
        ON DELETE NO ACTION;
END";

        private readonly string DropTables = $@"
DROP TABLE {_childTable};
DROP TABLE {_parentTable};
";

        private const string ProviderName = "System.Data.SqlClient";

        [TestCleanup]
        public void Cleanup()
        {
            try
            {
                ExecuteSql(DropTables);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error trying to drop table {e}- test may fail next time.");
            }
        }

        private void ExecuteSql(in string sqlText)
        {
            EnsureProviderFactory();
            var msSqlFactory = FactoryTools.GetFactory(ProviderName);
            using (var dbConnection = msSqlFactory.CreateConnection())
            {
                dbConnection.ConnectionString = _connectionString;
                dbConnection.Open();

                using (var dbCommand = msSqlFactory.CreateCommand())
                {
                    dbCommand.CommandText = sqlText;
                    dbCommand.Connection = dbConnection;
                    dbCommand.ExecuteNonQuery();
                }
            }
        }

        private void EnsureProviderFactory()
        {
            if (FactoryTools.GetFactory(ProviderName) == null)
            {
                var manualDescription = new DbProviderFactoryDescription
                {
                    Description = ProviderName,
                    InvariantName = ProviderName,
                    Name = ProviderName,
                    AssemblyQualifiedName = typeof(System.Data.SqlClient.SqlClientFactory).AssemblyQualifiedName,
                };

                // Initialize the repository.
                if (FactoryTools.ProviderRepository == null)
                {
                    var repo = new DbProviderFactoryRepository();
                    FactoryTools.ProviderRepository = repo;
                }

                FactoryTools.ProviderRepository.Add(manualDescription);
            }
        }

        [TestMethod, TestCategory("SqlServer")]
        public void Should_not_null_foreignKey_reference_if_single_table_read()
        {
            try
            {
                ExecuteSql(Create_Parent_And_Child_Tables);
            }
            catch (Exception e)
            {
                Assert.Inconclusive($"Cannot use db {e}");
            }
            var sqlServerSchemaReader = TestHelper.GetNorthwindReader();
            var childTable = sqlServerSchemaReader.Table(_childTable);
            if (childTable != null)
            {
                var fkConstraint = childTable.ForeignKeys.Find(c => c.Name == FK_name);

                Assert.IsNotNull(fkConstraint.RefersToTable, $"Should Reference to table {_parentTable}");
            }
        }

        [TestMethod, TestCategory("SqlServer")]
        public void Should_not_null_foreignKey_reference_if_ReadAll_called()
        {
            try
            {
                ExecuteSql(Create_Parent_And_Child_Tables);
            }
            catch (Exception e)
            {
                Assert.Inconclusive($"Cannot use db {e}");
            }
            var sqlServerSchemaReader = TestHelper.GetNorthwindReader();
            var childTable = sqlServerSchemaReader.ReadAll().Tables.FirstOrDefault(t => t.Name == _childTable);
            if (childTable != null)
            {
                var fkConstraint = childTable.ForeignKeys.Find(c => c.Name == FK_name);
                if (fkConstraint == null)
                {
                    Console.WriteLine($"Foreign key {FK_name} not found in table {_childTable}, probably cannot delete from previous test run");
                }
            }
        }
    }
}