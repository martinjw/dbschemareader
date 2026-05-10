using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DatabaseSchemaReaderTest.IntegrationTests
{
    /// <summary>
    /// Summary description for SqlServerNorthwind
    /// </summary>
    [TestClass]
    public class SqlServerNorthwind
    {
        //[TestMethod, TestCategory("SqlServer.Odbc")]
        //public void ReadNorthwindUsingOdbc()
        //{
        //    //you'll get much more information from System.Data.SqlClient
        //    const string providername = "System.Data.Odbc";
        //    const string connectionString = @"Driver={SQL Server};Server=.\SQLEXPRESS;Database=Northwind;Trusted_Connection=Yes;";
        //    ProviderChecker.Check(providername, connectionString);

        //    var dbReader = new DatabaseReader(connectionString, providername) { Owner = "dbo" };
        //    //this is slow because it pulls in sp_ stored procedures and system views.
        //    dbReader.Exclusions.StoredProcedureFilter = new PrefixFilter("sp_", "fn_", "dm_", "xp_");
        //    var schema = dbReader.ReadAll();

        //    Assert.IsTrue(schema.Tables.Count > 0);
        //}

        [TestMethod, TestCategory("SqlServer")]
        public void ReadNorthwindSchema()
        {
            DatabaseSchema schema = null;
            var ok = TestHelper.GetNorthwindReader(dbReader =>
            {
                dbReader.AllSchemas();
                schema = dbReader.DatabaseSchema;
            });
            if (!ok) return;

            //password is removed in SqlServer 2017
            //Assert.AreEqual(ConnectionStrings.Northwind, schema.ConnectionString, "Connection string is in the schema");
            Assert.IsNotNull(schema.ConnectionString, "Connection string is in the schema");
            Assert.AreEqual("dbo", schema.Owner, "Schema/owner is in the schema");
        }

        [TestMethod, TestCategory("SqlServer")]
        public void ReadNorthwindProducts()
        {
            DatabaseTable table = null;
            var ok = TestHelper.GetNorthwindReader(dbReader =>
            {
                table = dbReader.Table("Products");
            });
            if (!ok) return;

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
            DatabaseTable table = null;
            var ok = TestHelper.GetNorthwindReader(dbReader =>
            {
                table = dbReader.Table("Products");
            });
            if (!ok) return;
            DatabaseTable table2 = null;
            ok = TestHelper.GetNorthwindReader(dbReader =>
            {
                table2 = dbReader.Table("PRODUCTS");
            });
            if (!ok) return;

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
            IList<DatabaseTable> tables = null;
            var ok = TestHelper.GetNorthwindReader(dbReader =>
            {
                tables = dbReader.AllTables();
            });
            if (!ok) return;

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
            DatabaseSchema schema = null;
            var ok = TestHelper.GetNorthwindReader(dbReader =>
            {
                dbReader.AllSchemas();
                schema = dbReader.DatabaseSchema;
            });
            if (!ok) return;

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

            var tableType = schema.UserDefinedTables.Find(x =>
                string.Equals(x.Name, "LocationTableType", StringComparison.Ordinal));
            if (tableType != null) //our custom Northwind - see create_schema_northwind
            {
                Assert.AreEqual(2, tableType.Columns.Count);
            }

            //test sproc with a table value parameter, which is a user defined table type with 2 columns
            var sproc = schema.StoredProcedures.FirstOrDefault(x => x.Name == "usp_GetMaxFromTvp");
            if (sproc != null)
            {
                var arg = sproc.Arguments.First();
                Assert.AreEqual(2, arg.UserDefinedTable.Columns.Count);
            }

            var ssn = schema.UserDataTypes.Find(x => x.Name == "SSN");
            if (ssn != null)
            {
                Assert.IsTrue(ssn.DataType.IsString, "Underlying datatype is assigned to UDT");
            }
        }

        [TestMethod, TestCategory("SqlServer")]
        public void ReadNorthwindViews()
        {
            DatabaseSchema schema = null;
            var ok = TestHelper.GetNorthwindReader(dbReader =>
            {
                dbReader.AllSchemas();
                schema = dbReader.DatabaseSchema;
            });
            if (!ok) return;
            foreach (var view in schema.Views)
            {
                var sql = view.Sql;
                Assert.IsNotNull(sql, "ProcedureSource should also fill in the view source");
            }
        }

        [TestMethod, TestCategory("SqlServer")]
        public void ReadNorthwindProductsWithCodeGen()
        {
            DatabaseTable table = null;
            var ok = TestHelper.GetNorthwindReader(dbReader =>
            {
                dbReader.DataTypes(); //load the datatypes
                table = dbReader.Table("Products");
            });
            if (!ok) return;

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

            //act
            DatabaseSchema schema = null;
            var ok = TestHelper.GetNorthwindReader(dbReader =>
            {
                dbReader.Exclusions.TableFilter.FilterExclusions.Add(category);
                dbReader.Exclusions.ViewFilter.FilterExclusions.Add(alphaList);
                dbReader.Exclusions.StoredProcedureFilter.FilterExclusions.Add(custorderhist);
                schema = dbReader.ReadAll();
            });
            if (!ok) return;

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
            IList<DatabaseStoredProcedure> procedures = null;
            var ok = TestHelper.GetNorthwindReader(dbReader =>
            {
                procedures = dbReader.AllStoredProcedures();
            });
            if (!ok) return;

            var proc = procedures.First(x => x.Name == "CustOrderHist");
            var argsNumber = proc.Arguments.Count();

            Assert.IsTrue(argsNumber > 0);
        }
    }
}