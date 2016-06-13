using System.Collections.Generic;
using System.Linq.Expressions;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.DataSchema
{
    [TestClass]
    public class CompositeKeyTest
    {
        [TestMethod]
        public void TestCompositeKeys()
        {
            //arrange
            var schema = new DatabaseSchema(null, null);
            schema.AddTable("Store")
                .AddColumn<int>("Store_Id").AddPrimaryKey("Store_PK")
                .AddColumn<string>("Name").AddLength(10)

                .AddTable("StoreSale")
                .AddColumn<int>("Store_Id").AddForeignKey("Store_FK", "Store")
                .AddColumn<int>("StoreSale_Id")

                .AddTable("StoreSaleDetail")
                .AddColumn<int>("Store_Id").AddForeignKey("Store_FK", "Store")
                .AddColumn<int>("StoreSale_Id")
                .AddColumn<int>("StoreSaleDetail_Id")
                ;
            var store = schema.FindTableByName("Store");
            var storeSale = schema.FindTableByName("StoreSale");
            var storeSaleDetail = schema.FindTableByName("StoreSaleDetail");
            var pk1 = new DatabaseConstraint { ConstraintType = ConstraintType.PrimaryKey };
            pk1.Columns.Add("Store_Id");
            pk1.Columns.Add("StoreSale_Id");
            storeSale.AddConstraint(pk1);

            var pk2 = new DatabaseConstraint { ConstraintType = ConstraintType.PrimaryKey };
            pk2.Columns.Add("Store_Id");
            pk2.Columns.Add("StoreSale_Id");
            pk2.Columns.Add("StoreSaleDetail_Id");
            storeSaleDetail.AddConstraint(pk2);

            var fk = new DatabaseConstraint { ConstraintType = ConstraintType.ForeignKey, RefersToTable = "StoreSale" };
            fk.Columns.Add("Store_Id");
            fk.Columns.Add("StoreSale_Id");
            storeSaleDetail.AddConstraint(fk);

            //act
            DatabaseSchemaFixer.UpdateReferences(schema);

            //assert
            Assert.AreEqual(2, store.ForeignKeyChildren.Count, "Store is target of foreign keys from StoreSale and StoreSaleDetail");
            Assert.AreEqual(1, storeSale.ForeignKeyChildren.Count, "StoreSale is target of foreign key from StoreSaleDetail");

            var storeId = storeSaleDetail.FindColumn("Store_Id");
            Assert.AreEqual(2, storeId.ForeignKeyTableNames.Count, "StoreSaleDetail.StoreId is fk to both Store and StoreSale");
            var storeSaleId = storeSaleDetail.FindColumn("StoreSale_Id");
            Assert.AreEqual(1, storeSaleId.ForeignKeyTableNames.Count, "StoreSaleDetail.StoreSale_Id is fk to StoreSale");

        }
    }
}
