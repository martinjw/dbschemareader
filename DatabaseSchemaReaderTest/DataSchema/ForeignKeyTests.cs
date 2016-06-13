using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.DataSchema
{
    [TestClass]
    public class ForeignKeyTests
    {
        [TestMethod]
        public void TestForeignKeysToPrimaryKey()
        {
            //arrange
            var schema = new DatabaseSchema(null, null);
            schema.AddTable("Target")
                .AddColumn<int>("Target_Id").AddPrimaryKey("TARGET_PK")
                .AddColumn<string>("Name").AddLength(10).AddUniqueKey("TARGET_UQ1")

                .AddTable("Source")
                .AddColumn<int>("Source_Id").AddPrimaryKey("SOURCE_PK")
                .AddColumn<int>("Target_Id").AddForeignKey("SOURCE_FK", "Target");
            var source = schema.FindTableByName("Source");
            var target = schema.FindTableByName("Target");

            //act
            var fk = source.ForeignKeys[0];

            //assert
            Assert.AreEqual("Target", fk.RefersToTable, "Name of referenced table");
            Assert.AreEqual(target, fk.ReferencedTable(schema), "Actual reference to table");
            Assert.AreEqual(target.PrimaryKey.Columns, fk.ReferencedColumns(schema), "Columns are matched up");
        }

        [TestMethod]
        public void TestForeignKeysToUniqueKey()
        {
            //arrange
            var schema = new DatabaseSchema(null, null);
            schema.AddTable("Target")
                .AddColumn<int>("Target_Id").AddPrimaryKey("TARGET_PK")
                .AddColumn<string>("Name").AddLength(10).AddUniqueKey("TARGET_UQ1")

                .AddTable("Source")
                .AddColumn<int>("Source_Id").AddPrimaryKey("SOURCE_PK")
                .AddColumn<string>("Target_Name");
            var source = schema.FindTableByName("Source");
            var target = schema.FindTableByName("Target");
            source.AddConstraint(new DatabaseConstraint
                                     {
                                         ConstraintType = ConstraintType.ForeignKey,
                                         Name = "SOURCE_FK",
                                         RefersToTable = "Target",
                                         TableName = "Source",
                                         RefersToConstraint = "TARGET_UQ1",
                                     });
            var fk = source.ForeignKeys[0];
            fk.Columns.Add("Target_Name");

            //act
            var referencedColumns = fk.ReferencedColumns(schema);

            //assert
            Assert.AreEqual(target.UniqueKeys[0].Columns, referencedColumns, "Columns are matched up");
        }

        [TestMethod]
        public void TestForeignKeysCrossSchema()
        {
            //arrange
            var schema = new DatabaseSchema(null, null);
            schema.AddTable("Target")
                .AddColumn<int>("Target_Id").AddPrimaryKey("TARGET_PK")
                .AddColumn<string>("Name").AddLength(10).AddUniqueKey("TARGET_UQ1")

                .AddTable("Source")
                .AddColumn<int>("Source_Id").AddPrimaryKey("SOURCE_PK")
                .AddColumn<int>("Target_Id");
            var source = schema.FindTableByName("Source");
            var target = schema.FindTableByName("Target");
            target.SchemaOwner = "dbo";
            source.SchemaOwner = "other";
            var targetId = source.FindColumn("Target_Id");
            targetId.AddForeignKey("SOURCE_FK", tables => target);

            //act
            var fk = source.ForeignKeys[0];

            //assert
            Assert.AreEqual("Target", fk.RefersToTable, "Name of referenced table");
            Assert.AreEqual(target, fk.ReferencedTable(schema), "Actual reference to table");
            Assert.AreEqual(target.PrimaryKey.Columns, fk.ReferencedColumns(schema), "Columns are matched up");
        }
    }
}
