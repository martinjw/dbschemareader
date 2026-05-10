using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.Utilities;
using DatabaseSchemaReader.Utilities.DbProvider;
using System.Data.Common;
using System.Data.SQLite;

namespace DatabaseSchemaReaderFrameworkTests.SqlGen.SqlWriterTests
{
    [TestClass]
    public class SqlWriterSQLiteTest
    {
        private const string ProviderName = "System.Data.SQLite";
        private readonly string _databaseFile = ConnectionStrings.SqLiteFilePath;
        private readonly string _connectionString;
        private DatabaseTable _categoriesTable;
        private readonly DbProviderFactory _factory;
        private bool _isInitialized;

        public SqlWriterSQLiteTest()
        {
            _connectionString = "Data Source=" + _databaseFile;
            try
            {
                _factory = SQLiteFactory.Instance;
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Could not load Sqlite");
            }
            catch (Exception)
            {
                Console.WriteLine("Could not load Sqlite");
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            FactoryTools.SingleProviderFactory = null;
        }

        private DatabaseTable LoadCategoriesFromNorthwind()
        {
            if (_categoriesTable != null)
            {
                Console.WriteLine("Found existing categories table");
                return _categoriesTable;
            }
            if (!_isInitialized) CreateSqlite(_databaseFile, Ddl());

            EnsureProviderFactory();

            var dbReader = new DatabaseReader(_connectionString, ProviderName);
            dbReader.DataTypes(); //ensure we have datatypes (this doesn't hit the database)
            _categoriesTable = dbReader.Table("Categories"); //this hits database for columns and constraints
            if (_categoriesTable == null)
                Assert.Inconclusive("Could not load Categories table from SQLite file");
            return _categoriesTable;
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
                    AssemblyQualifiedName = _factory.GetType().AssemblyQualifiedName,
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

        public void CreateSqlite(string filePath, string sql)
        {
            // ensure directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            //already exists, so skip
            if (File.Exists(_databaseFile))
                return;

            // build connection string
            var csb = new SQLiteConnectionStringBuilder { DataSource = filePath };
            var connectionString = csb.ConnectionString;
            Console.WriteLine($"Sqlite connection string = {connectionString}");

            using (var con = new SQLiteConnection(connectionString))
            {
                con.Open();
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                }
            }

            _isInitialized = true;
        }

        [TestMethod, TestCategory("SQLite")]
        public void TestGeneratedSqlForCount()
        {
            var table = LoadCategoriesFromNorthwind();

            var runner = new SqlWriterCommonTest(SqlType.SQLite, table, _factory, _connectionString);

            runner.RunCountSql();
        }

        [TestMethod, TestCategory("SQLite")]
        public void TestGeneratedSqlForSelectAll()
        {
            var table = LoadCategoriesFromNorthwind();

            var runner = new SqlWriterCommonTest(SqlType.SQLite, table, _factory, _connectionString);

            runner.RunSelectAllSql();
        }

        [TestMethod, TestCategory("SQLite")]
        public void TestGeneratedSqlForPaging()
        {
            var table = LoadCategoriesFromNorthwind();

            var runner = new SqlWriterCommonTest(SqlType.SQLite, table, _factory, _connectionString);

            runner.RunPagingSql();
        }

        [TestMethod, TestCategory("SQLite")]
        public void TestGeneratedSqlForPagingStartToEnd()
        {
            var table = LoadCategoriesFromNorthwind();

            var runner = new SqlWriterCommonTest(SqlType.SQLite, table, _factory, _connectionString);

            runner.RunPagingStartToEndSql();
        }

        [TestMethod, TestCategory("SQLite")]
        public void TestGeneratedSqlForInsert()
        {
            //arrange
            var table = LoadCategoriesFromNorthwind();
            var writer = new SqlWriter(table, SqlType.SQLite);
            var sql = writer.InsertSql();
            int identity;

            //run generated sql
            using (var con = _factory.CreateConnection())
            {
                con.ConnectionString = _connectionString;
                con.Open();
                using (var transaction = con.BeginTransaction())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = sql;
                        cmd.Transaction = transaction;
                        foreach (var column in table.Columns)
                        {
                            if (column.IsAutoNumber) continue;

                            var par = cmd.CreateParameter();
                            par.ParameterName = writer.ParameterName(column.Name);
                            object value = DummyDataCreator.CreateData(column);
                            par.Value = value ?? DBNull.Value;
                            cmd.Parameters.Add(par);
                        }
                        identity = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    //explicit rollback. If we errored, implicit rollback.
                    transaction.Rollback();
                }
            }

            //assert
            Assert.AreNotEqual(0, identity);
        }

        private static string Ddl()
        {
            return @"CREATE TABLE [Categories]
(
  [CategoryID] INTEGER PRIMARY KEY AUTOINCREMENT,
  [CategoryName] TEXT NOT NULL,
  [Description] TEXT,
  [Picture] BLOB
);
CREATE TABLE [ProductCategory]
(
  [CategoryID] INTEGER NOT NULL,
  [ProductID] INTEGER NOT NULL,
PRIMARY KEY ([CategoryID], [ProductID])
);

CREATE TABLE [Products]
(
  [ProductID] INTEGER PRIMARY KEY AUTOINCREMENT,
  [ProductName] TEXT NOT NULL,
  [SupplierID] INTEGER,
  [CategoryID] INTEGER,
  [QuantityPerUnit] TEXT,
  [UnitPrice] NUMERIC DEFAULT 0,
  [UnitsInStock] NUMERIC DEFAULT 0,
  [UnitsOnOrder] NUMERIC DEFAULT 0,
  [ReorderLevel] NUMERIC DEFAULT 0,
  [Discontinued] INTEGER NOT NULL DEFAULT 0,
FOREIGN KEY ([CategoryID]) REFERENCES [Categories] ([CategoryID])
);

            INSERT INTO [Categories] (
  [CategoryName],  [Description],  [Picture]) VALUES (
'Beverages' ,'Soft drinks, coffees, teas, beers, and ales' ,NULL
);

INSERT INTO [Categories] (
  [CategoryName],  [Description],  [Picture]) VALUES (
'Condiments' ,'Sweet and savory sauces, relishes, spreads, and seasonings' ,NULL
);

INSERT INTO [Categories] (
  [CategoryName],  [Description],  [Picture]) VALUES (
'Confections' ,'Desserts, candies, and sweet breads' ,NULL
);

INSERT INTO [Categories] (
  [CategoryName],  [Description],  [Picture]) VALUES (
'Dairy Products' ,'Cheeses' ,NULL
);

INSERT INTO [Categories] (
  [CategoryName],  [Description],  [Picture]) VALUES (
'Grains/Cereals' ,'Breads, crackers, pasta, and cereal' ,NULL
);

INSERT INTO [Categories] (
  [CategoryName],  [Description],  [Picture]) VALUES (
'Meat/Poultry' ,'Prepared meats' ,NULL
);

INSERT INTO [Categories] (
  [CategoryName],  [Description],  [Picture]) VALUES (
'Produce' ,'Dried fruit and bean curd' ,NULL
);

INSERT INTO [Categories] (
  [CategoryName],  [Description],  [Picture]) VALUES (
'Seafood' ,'Seaweed and fish' ,NULL
);";
        }
    }
}