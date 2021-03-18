using System.Data.SQLite;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest
{
    [TestClass]
    public class InitSqLite
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            //initialize SQLite database
            var filePath = ConnectionStrings.SqLiteFilePath;
            AssemblyCleanup();
            SQLiteConnection.CreateFile(filePath);
            var csb = new SQLiteConnectionStringBuilder { DataSource = filePath };
            var connectionString = csb.ConnectionString;
            using (var con = new SQLiteConnection(connectionString))
            {
                using (var cmd = con.CreateCommand())
                {
                    cmd.Connection = con;
                    cmd.CommandText = Ddl();
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }


        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            var filePath = ConnectionStrings.SqLiteFilePath;
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
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

CREATE TABLE [CustomerCustomerDemo]
(
  [CustomerID] TEXT NOT NULL,
  [CustomerTypeID] TEXT NOT NULL,
PRIMARY KEY ([CustomerID], [CustomerTypeID]),
FOREIGN KEY ([CustomerTypeID]) REFERENCES [CustomerDemographics] ([CustomerTypeID]),
FOREIGN KEY ([CustomerID]) REFERENCES [Customers] ([CustomerID])
);

CREATE TABLE [CustomerDemographics]
(
  [CustomerTypeID] TEXT PRIMARY KEY NOT NULL,
  [CustomerDesc] TEXT
);


CREATE TABLE [Customers]
(
  [CustomerID] TEXT PRIMARY KEY NOT NULL,
  [CompanyName] TEXT NOT NULL,
  [ContactName] TEXT,
  [ContactTitle] TEXT,
  [Address] TEXT,
  [City] TEXT,
  [Region] TEXT,
  [PostalCode] TEXT,
  [Country] TEXT,
  [Phone] TEXT,
  [Fax] TEXT
);


CREATE TABLE [Employees]
(
  [EmployeeID] INTEGER PRIMARY KEY AUTOINCREMENT,
  [LastName] TEXT NOT NULL,
  [FirstName] TEXT NOT NULL,
  [Title] TEXT,
  [TitleOfCourtesy] TEXT,
  [BirthDate] DATETIME,
  [HireDate] DATETIME,
  [Address] TEXT,
  [City] TEXT,
  [Region] TEXT,
  [PostalCode] TEXT,
  [Country] TEXT,
  [HomePhone] TEXT,
  [Extension] TEXT,
  [Photo] BLOB,
  [Notes] TEXT,
  [ReportsTo] INTEGER,
  [PhotoPath] TEXT,
FOREIGN KEY ([ReportsTo]) REFERENCES [Employees] ([EmployeeID])
);



CREATE TABLE [EmployeeTerritories]
(
  [EmployeeID] INTEGER NOT NULL,
  [TerritoryID] TEXT NOT NULL,
PRIMARY KEY ([EmployeeID], [TerritoryID]),
FOREIGN KEY ([EmployeeID]) REFERENCES [Employees] ([EmployeeID]),
FOREIGN KEY ([TerritoryID]) REFERENCES [Territories] ([TerritoryID])
);


CREATE TABLE [Order Details]
(
  [OrderID] INTEGER NOT NULL,
  [ProductID] INTEGER NOT NULL,
  [UnitPrice] NUMERIC NOT NULL DEFAULT 0,
  [Quantity] NUMERIC NOT NULL DEFAULT 1,
  [Discount] NUMERIC NOT NULL DEFAULT 0,
PRIMARY KEY ([OrderID], [ProductID]),
FOREIGN KEY ([OrderID]) REFERENCES [Orders] ([OrderID]),
FOREIGN KEY ([ProductID]) REFERENCES [Products] ([ProductID])
);


CREATE TABLE [Orders]
(
  [OrderID] INTEGER PRIMARY KEY AUTOINCREMENT,
  [CustomerID] TEXT,
  [EmployeeID] INTEGER,
  [OrderDate] DATETIME,
  [RequiredDate] DATETIME,
  [ShippedDate] DATETIME,
  [ShipVia] INTEGER,
  [Freight] NUMERIC DEFAULT 0,
  [ShipName] TEXT,
  [ShipAddress] TEXT,
  [ShipCity] TEXT,
  [ShipRegion] TEXT,
  [ShipPostalCode] TEXT,
  [ShipCountry] TEXT,
FOREIGN KEY ([CustomerID]) REFERENCES [Customers] ([CustomerID]),
FOREIGN KEY ([EmployeeID]) REFERENCES [Employees] ([EmployeeID]),
FOREIGN KEY ([ShipVia]) REFERENCES [Shippers] ([ShipperID])
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
FOREIGN KEY ([CategoryID]) REFERENCES [Categories] ([CategoryID]),
FOREIGN KEY ([SupplierID]) REFERENCES [Suppliers] ([SupplierID])
);



CREATE TABLE [Region]
(
  [RegionID] INTEGER PRIMARY KEY NOT NULL,
  [RegionDescription] TEXT NOT NULL
);


CREATE TABLE [Shippers]
(
  [ShipperID] INTEGER PRIMARY KEY AUTOINCREMENT,
  [CompanyName] TEXT NOT NULL,
  [Phone] TEXT
);

CREATE TABLE [Suppliers]
(
  [SupplierID] INTEGER PRIMARY KEY AUTOINCREMENT,
  [CompanyName] TEXT NOT NULL,
  [ContactName] TEXT,
  [ContactTitle] TEXT,
  [Address] TEXT,
  [City] TEXT,
  [Region] TEXT,
  [PostalCode] TEXT,
  [Country] TEXT,
  [Phone] TEXT,
  [Fax] TEXT,
  [HomePage] TEXT
);

CREATE TABLE [Territories]
(
  [TerritoryID] TEXT PRIMARY KEY NOT NULL,
  [TerritoryDescription] TEXT NOT NULL,
  [RegionID] INTEGER NOT NULL,
FOREIGN KEY ([RegionID]) REFERENCES [Region] ([RegionID])
);

CREATE TABLE [CompoundKeys] (
   [Key1] TEXT NOT NULL,
   [Key2] TEXT NOT NULL,
   CONSTRAINT [Pk_CompoundKeys] PRIMARY KEY([Key1], [Key2])
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
);
";
        }
    }
}
