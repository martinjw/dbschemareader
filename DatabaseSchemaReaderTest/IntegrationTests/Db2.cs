using DatabaseSchemaReader;
#if !NUNIT
using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
using TestContext = System.Object;
#endif

namespace DatabaseSchemaReaderTest.IntegrationTests
{
    /// <summary>
    /// These are INTEGRATION tests using databases.
    /// The following databases should exist on localhost:
    ///     Db2 with Sample (user id root, passwod mysql)
    /// </summary>
    [TestClass]
    public class Db2
    {
        [TestMethod]
        public void Db2Test()
        {
            const string providername = "IBM.Data.DB2";
            const string connectionString = @"Server=localhost:50000;UID=db2admin;pwd=db2;Database=Sample";
            ProviderChecker.Check(providername, connectionString);

            var factory = System.Data.Common.DbProviderFactories.GetFactory(providername);
            using (var connection = factory.CreateConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();
                string metaDataCollections = System.Data.Common.DbMetaDataCollectionNames.MetaDataCollections;
                var dt = connection.GetSchema(metaDataCollections);
                foreach (System.Data.DataRow row in dt.Rows)
                {
                    var collectionName = (string)row["CollectionName"];
                    System.Diagnostics.Debug.WriteLine(collectionName);
                    if (collectionName != metaDataCollections)
                    {
                        var col = connection.GetSchema(collectionName);
                        foreach (System.Data.DataColumn column in col.Columns)
                        {
                            System.Diagnostics.Debug.WriteLine("\t" + column.ColumnName + "\t" + column.DataType.Name);
                        }
                    }
                }
                connection.Close();
            }


            var dbReader = new DatabaseReader(connectionString, providername);
            var schema = dbReader.ReadAll();
            var table = schema.FindTableByName("EMPLOYEE");
            Assert.IsTrue(table.Columns.Count > 0);
        }
    }
}
