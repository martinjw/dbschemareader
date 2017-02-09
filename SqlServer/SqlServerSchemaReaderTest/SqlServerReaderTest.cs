using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServerSchemaReader;
using SqlServerSchemaReader.Schema;
using System.Linq;

namespace SqlServerSchemaReaderTest
{
    [TestClass]
    public class SqlServerReaderTest
    {
        private void Arrange()
        {
            if (_schema != null) return;
            const string providername = "System.Data.SqlClient";
            var connectionString = ConnectionStrings.TestSchema;
            ProviderChecker.Check(providername, connectionString);

            var dr = new SqlServerDatabaseReader(connectionString) { Owner = "dbo" };
            _schema = dr.ReadAll() as SqlServerSchema;
        }

        private SqlServerSchema _schema;

        [TestMethod]
        public void TestTables()
        {
            Arrange();

            Assert.IsNotNull(_schema);
            //we exposed a typed property for tables.
            //All tables should be SqlServerTables (override SchemaFactory)
            Assert.AreEqual(_schema.Tables.Count, _schema.SqlServerTables.Count());
            //pick the first table with stats
            var table = _schema.SqlServerTables.FirstOrDefault(t => t.DatabaseStatistics.Count > 0);
            Assert.IsNotNull(table); //should be some stats somewhere
        }

        [TestMethod]
        public void TestAliasTypes()
        {
            Arrange();

            Assert.IsNotNull(_schema);
            Assert.IsTrue(_schema.AliasTypes.Count > 0, "There should be an alias type");
            var ssn = _schema.AliasTypes.Find(at => at.Name == "SSN");
            Assert.IsNotNull(ssn, "SSN is an alias type");
            Assert.AreEqual(11, ssn.MaxLength, "Has a max length of 11");
            Assert.IsFalse(ssn.Nullable, "ssn is non-nullable");
            //there should be a table called SsnTable
            var col = ssn.DependentColumns.Find(c => c.TableName == "SsnTable");
            Assert.IsNotNull(col);
            //there should be a sproc called FindSsn
            var arg = ssn.DependentArguments.Find(a => a.ProcedureName == "FindSsn");
            Assert.IsNotNull(arg);
        }

        [TestMethod]
        public void TestTableTypes()
        {
            Arrange();

            Assert.IsNotNull(_schema);
            Assert.IsTrue(_schema.TableTypes.Count > 0, "There should be an table type");
            var tableType = _schema.TableTypes.Find(at => at.Name == "TestTableType");
            Assert.IsNotNull(tableType, "TestTableType is an table type");
            Assert.AreEqual(7, tableType.Columns.Count, "Has 7 columns");

            var computed = tableType.Columns.Find(x => x.Name == "ComputedValue");
            Assert.IsTrue(computed.IsComputed, "computed column is detected");
            var defaulted = tableType.Columns.Find(x => x.Name == "DefaultedValue");
            Assert.IsNotNull(defaulted.DefaultValue, "default column has default value");
            var unique = tableType.Columns.Find(x => x.Name == "UniqueName");
            Assert.IsTrue(unique.IsUniqueKey, "unique column is unique");

            Assert.AreEqual(2, tableType.PrimaryKey.Columns.Count, "pk has 2 columns");

        }

    }
}