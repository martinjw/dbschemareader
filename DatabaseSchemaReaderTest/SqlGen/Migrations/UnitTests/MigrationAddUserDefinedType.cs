using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DatabaseSchemaReaderTest.SqlGen.Migrations.UnitTests
{
    [TestClass]
    public class MigrationAddUserDefinedType
    {
        [TestMethod]
        public void TestAddUserDataType()
        {
            //arrange
            var migration = new DdlGeneratorFactory(SqlType.SqlServer).MigrationGenerator();

            //CREATE TYPE SSN
            //FROM varchar(11) NOT NULL

            var udt = new UserDataType
            {
                Name = "SSN",
                SchemaOwner = "dbo",
                DbTypeName = "varchar",
                MaxLength = 11,
                Nullable = false
            };

            //act
            var sql = migration.AddUserDataType(udt);

            //assert
            Assert.IsTrue(sql.StartsWith("CREATE TYPE [dbo].[SSN] FROM VARCHAR (11) NOT NULL", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }

        //[TestMethod]
        //public void TestAddUserTableType()
        //{
        //    //arrange
        //    var migration = new DdlGeneratorFactory(SqlType.SqlServer).MigrationGenerator();

        //    //CREATE TYPE LocationTableType AS TABLE   
        //    //    ( LocationName VARCHAR(50)  
        //    //    , CostRate INT );  

        //    var udt = new UserDefinedTable
        //    {
        //        Name = "LocationTableType",
        //        SchemaOwner = "dbo",
        //        Columns =
        //        {
        //            new DatabaseColumn { Name= "LocationName", DbDataType = "varchar", Length = 50},
        //            new DatabaseColumn {Name = "CostRate", DbDataType = "int"}
        //        }
        //    };

        //    //act
        //    var sql = migration.AddUserDefinedTableType(udt);

        //    //assert
        //    Assert.IsTrue(sql.StartsWith("CREATE TYPE [dbo].[LocationTableType] AS TABLE", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        //}

        //CREATE TYPE InventoryItem AS TABLE
        //(
        //	[Name] NVARCHAR(50) NOT NULL,
        //	SupplierId BIGINT NOT NULL,
        //	Price DECIMAL (18, 4) NULL,
        //	PRIMARY KEY (Name),
        //	INDEX IX_InventoryItem_Price (Price)
        //)

    }
}