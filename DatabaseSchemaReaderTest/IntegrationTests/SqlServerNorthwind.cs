using System.Diagnostics;
using System.Linq;
using DatabaseSchemaReader;
using DatabaseSchemaReader.Filters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.IntegrationTests
{
    /// <summary>
    /// Summary description for SqlServerNorthwind
    /// </summary>
    [TestClass]
    public class SqlServerNorthwind
    {

        [TestMethod, TestCategory("SqlServer.Odbc")]
        public void ReadNorthwindUsingOdbc()
        {
            //you'll get much more information from System.Data.SqlClient
            const string providername = "System.Data.Odbc";
            const string connectionString = @"Driver={SQL Server};Server=.\SQLEXPRESS;Database=Northwind;Trusted_Connection=Yes;";
            ProviderChecker.Check(providername, connectionString);

            var dbReader = new DatabaseReader(connectionString, providername) { Owner = "dbo" };
            //this is slow because it pulls in sp_ stored procedures and system views.
            dbReader.Exclusions.StoredProcedureFilter = new PrefixFilter("sp_", "fn_", "dm_", "xp_");
            var schema = dbReader.ReadAll();

            Assert.IsTrue(schema.Tables.Count > 0);
        }

        [TestMethod, TestCategory("SqlServer")]
        public void ReadNorthwindSchema()
        {
            var dbReader = TestHelper.GetNorthwindReader();
            dbReader.AllSchemas();
            var schema = dbReader.DatabaseSchema;
            
            //password is removed in SqlServer 2017
            //Assert.AreEqual(ConnectionStrings.Northwind, schema.ConnectionString, "Connection string is in the schema");
            Assert.IsNotNull(schema.ConnectionString, "Connection string is in the schema");
            Assert.AreEqual("dbo", schema.Owner, "Schema/owner is in the schema");
        }

        [TestMethod, TestCategory("SqlServer")]
        public void ReadNorthwindProducts()
        {
            var dbReader = TestHelper.GetNorthwindReader();
            var table = dbReader.Table("Products");
            Debug.WriteLine("Table " + table.Name);

            foreach (var column in table.Columns)
            {
                //because we loaded only a single table
                //relations aren't available here (to datatypes/foreign key tables)
                Debug.Write("\tColumn " + column.Name + "\t" + column.DbDataType);
                if (column.Length > 0) Debug.Write("(" + column.Length + ")");
                if (column.IsPrimaryKey) Debug.Write("\tPrimary key");
                if (column.IsForeignKey) Debug.Write("\tForeign key to " + column.ForeignKeyTableName);
                Debug.WriteLine("");
            }
            //Table Products
            //	Column ProductID	int	Primary key
            //	Column ProductName	nvarchar(40)
            //	Column SupplierID	int	Foreign key to Suppliers
            //	Column CategoryID	int	Foreign key to Categories
            //	Column QuantityPerUnit	nvarchar(20)
            //	Column UnitPrice	money
            //	Column UnitsInStock	smallint
            //	Column UnitsOnOrder	smallint
            //	Column ReorderLevel	smallint
            //	Column Discontinued	bit
        }

        [TestMethod, TestCategory("SqlServer")]
        public void ReadCaseSensitiveTableName()
        {
            var dbReader = TestHelper.GetNorthwindReader();
            var table = dbReader.Table("Products");
            Debug.WriteLine("Table " + table.Name);

            var dbReader2 = TestHelper.GetNorthwindReader();
            var table2 = dbReader.Table("PRODUCTS");
            Debug.WriteLine("Table " + table2.Name);

            for (int i = 0; i < table.Columns.Count; i++)
            {
                var col = table.Columns[i];
                var col2 = table2.Columns[i];

                Assert.AreEqual(col.Name, col2.Name);
                Assert.AreEqual(col.Length, col2.Length);
                Assert.AreEqual(col.IsPrimaryKey, col2.IsPrimaryKey);
                Assert.AreEqual(col.IsForeignKey, col2.IsForeignKey);
                Assert.AreEqual(col.IsIndexed, col2.IsIndexed);
            }
        }

        [TestMethod, TestCategory("SqlServer")]
        public void ReadNorthwindAllTables()
        {
            var dbReader = TestHelper.GetNorthwindReader();
            var tables = dbReader.AllTables();
            foreach (var table in tables)
            {
                Debug.WriteLine("Table " + table.Name);

                foreach (var column in table.Columns)
                {
                    //because we loaded only tables
                    //relations to datatypes aren't available here
                    //but foreign key tables are linked up
                    Debug.Write("\tColumn " + column.Name + "\t" + column.DbDataType);
                    if (column.Length > 0) Debug.Write("(" + column.Length + ")");
                    if (column.IsPrimaryKey) Debug.Write("\tPrimary key");
                    if (column.IsForeignKey) Debug.Write("\tForeign key to " + column.ForeignKeyTable.Name);
                    Debug.WriteLine("");
                }
            }
            //Table Products
            //	Column ProductID	int	Primary key
            //	Column ProductName	nvarchar(40)
            //	Column SupplierID	int	Foreign key to Suppliers
            //	Column CategoryID	int	Foreign key to Categories
            //	Column QuantityPerUnit	nvarchar(20)
            //	Column UnitPrice	money
            //	Column UnitsInStock	smallint
            //	Column UnitsOnOrder	smallint
            //	Column ReorderLevel	smallint
            //	Column Discontinued	bit
        }

        [TestMethod, TestCategory("SqlServer")]
        public void ReadNorthwind()
        {
            var dbReader = TestHelper.GetNorthwindReader();
            var schema = dbReader.ReadAll();

            foreach (var table in schema.Tables)
            {
                Debug.WriteLine("Table " + table.Name);

                foreach (var column in table.Columns)
                {
                    Debug.Write("\tColumn " + column.Name + "\t" + column.DataType.TypeName);
                    if (column.DataType.IsString) Debug.Write("(" + column.Length + ")");
                    if (column.IsPrimaryKey) Debug.Write("\tPrimary key");
                    if (column.IsForeignKey) Debug.Write("\tForeign key to " + column.ForeignKeyTable.Name);
                    Debug.WriteLine("");
                }
                //Table Products
                //	Column ProductID	int	Primary key
                //	Column ProductName	nvarchar(40)
                //	Column SupplierID	int	Foreign key to Suppliers
                //	Column CategoryID	int	Foreign key to Categories
                //	Column QuantityPerUnit	nvarchar(20)
                //	Column UnitPrice	money
                //	Column UnitsInStock	smallint
                //	Column UnitsOnOrder	smallint
                //	Column ReorderLevel	smallint
                //	Column Discontinued	bit
            }
        }

        [TestMethod, TestCategory("SqlServer")]
        public void ReadNorthwindViews()
        {
            var dbReader = TestHelper.GetNorthwindReader();
            var schema = dbReader.ReadAll();
            foreach (var view in schema.Views)
            {
                var sql = view.Sql;
                Assert.IsNotNull(sql, "ProcedureSource should also fill in the view source");
            }
        }


        [TestMethod, TestCategory("SqlServer")]
        public void ReadNorthwindProductsWithCodeGen()
        {
            var dbReader = TestHelper.GetNorthwindReader();
            dbReader.DataTypes(); //load the datatypes
            var table = dbReader.Table("Products");
            Debug.WriteLine("Table " + table.Name);

            foreach (var column in table.Columns)
            {
                //Cs properties (the column name could be made .Net friendly too)
                Debug.WriteLine("\tpublic " + column.DataType.NetCodeName(column) + " " + column.Name + " { get; set; }");
            }
            //	public int ProductID { get; set; }
            //	public string ProductName { get; set; }
            //	public int SupplierID { get; set; }
            //	public int CategoryID { get; set; }
            //	public string QuantityPerUnit { get; set; }
            //	public decimal UnitPrice { get; set; }
            //	public short UnitsInStock { get; set; }
            //	public short UnitsOnOrder { get; set; }
            //	public short ReorderLevel { get; set; }
            //	public bool Discontinued { get; set; }

            //get the sql
            var sqlWriter =
                new SqlWriter(table, DatabaseSchemaReader.DataSchema.SqlType.SqlServer);
            var sql = sqlWriter.SelectPageSql(); //paging sql
            sql = SqlWriter.SimpleFormat(sql); //remove line breaks

            Debug.WriteLine(sql);
            //SELECT [ProductID], [ProductName], ...etc... 
            //FROM 
            //(SELECT ROW_NUMBER() OVER( ORDER BY [ProductID]) AS 
            //rowNumber, [ProductID], [ProductName],  ...etc..
            //FROM [Products]) AS countedTable 
            //WHERE rowNumber >= (@pageSize * (@currentPage - 1)) 
            //AND rowNumber <= (@pageSize * @currentPage)
        }


        [TestMethod, TestCategory("SqlServer")]
        public void ReadNorthwindWithFilters()
        {
            //arrange
            const string category = "Categories";
            const string alphaList = "Alphabetical list of products";
            const string custorderhist = "CustOrderHist";
            var dbReader = TestHelper.GetNorthwindReader();
            dbReader.Exclusions.TableFilter.FilterExclusions.Add(category);
            dbReader.Exclusions.ViewFilter.FilterExclusions.Add(alphaList);
            dbReader.Exclusions.StoredProcedureFilter.FilterExclusions.Add(custorderhist);

            //act
            var schema = dbReader.ReadAll();

            //assert
            var table = schema.FindTableByName(category);
            Assert.IsNull(table);
            var view = schema.Views.FirstOrDefault(v => v.Name == alphaList);
            Assert.IsNull(view);
            var sproc = schema.StoredProcedures.FirstOrDefault(sp => sp.Name == custorderhist);
            Assert.IsNull(sproc);
        }

        [TestMethod, TestCategory("SqlServer")]
        public void DublicatedArgumentsDemo()
        {
            var dbReader = TestHelper.GetNorthwindReader();
            var procedures = dbReader.AllStoredProcedures();

            var proc = procedures.First(x => x.Name == "CustOrderHist");
            var argsNumber = proc.Arguments.Count();

            dbReader.AllStoredProcedures();
            Assert.AreEqual(argsNumber,
                            proc.Arguments.Count(),
                            "Number of args changed");
        }
    }
}
