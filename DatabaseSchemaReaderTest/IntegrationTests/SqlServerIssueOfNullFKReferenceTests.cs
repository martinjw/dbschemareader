using static DatabaseSchemaReader.DataSchema.SqlType;
using DatabaseSchemaReader.ProviderSchemaReaders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.Common;
using DatabaseSchemaReader;
using System.Linq;

namespace DatabaseSchemaReaderTest.IntegrationTests
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
CREATE TABLE {_parentTable} (
    ID      INTEGER,
    NAME    VARCHAR,
    CONSTRAINT PK_{_parentTable} PRIMARY KEY CLUSTERED (ID)
)
CREATE UNIQUE NONCLUSTERED INDEX IX_{_parentTable}_unique_ID ON {_parentTable} (ID ASC) INCLUDE (NAME)

CREATE TABLE {_childTable} (
    ID      INTEGER,
    PID     INTEGER,
    NAME    VARCHAR,

    CONSTRAINT PK_{_childTable} PRIMARY KEY CLUSTERED (ID)
   ,CONSTRAINT {FK_name}
        FOREIGN KEY (PID)
        REFERENCES {_parentTable} (ID)
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)";
        private readonly string DropTables = $@"
DROP TABLE {_childTable};
DROP TABLE MY_PARENT_TABLE;
";

        [TestCleanup]
        public void Cleanup()
        {
            ExecuteSql(DropTables);
        }

        private void ExecuteSql(in string sqlText)
        {
            var msSqlFactory = DbProviderFactories.GetFactory("System.Data.SqlClient");
            DbConnection dbConnection = msSqlFactory.CreateConnection();
            dbConnection.ConnectionString = _connectionString;
            dbConnection.Open();

            DbCommand dbCommand = msSqlFactory.CreateCommand();
            dbCommand.CommandText = sqlText;
            dbCommand.Connection = dbConnection;
            dbCommand.ExecuteNonQuery();
        }

        [TestMethod, TestCategory("SqlServer")]
        public void Should_not_null_foreignKey_reference_if_single_table_read()
        {
            ExecuteSql(Create_Parent_And_Child_Tables);

            var sqlServerSchemaReader = TestHelper.GetNorthwindReader();
            var childTable = sqlServerSchemaReader.Table(_childTable);
            var fkConstraint = childTable.ForeignKeys.Find(c => c.Name == FK_name);

            Assert.IsNotNull(fkConstraint.RefersToTable, $"Should Reference to table {_parentTable}");
        }

        [TestMethod, TestCategory("SqlServer")]
        public void Should_not_null_foreignKey_reference_if_ReadAll_called()
        {
            ExecuteSql(Create_Parent_And_Child_Tables);

            var sqlServerSchemaReader = TestHelper.GetNorthwindReader();
            var childTable = sqlServerSchemaReader.ReadAll().Tables.First(t => t.Name == _childTable);
            var fkConstraint = childTable.ForeignKeys.Find(c => c.Name == FK_name);

            Assert.IsNotNull(fkConstraint.RefersToTable, $"Should Reference to table {_parentTable}");
        }
    }
}