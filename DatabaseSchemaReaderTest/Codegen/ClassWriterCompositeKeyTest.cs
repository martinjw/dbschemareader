using System;
using System.Linq;
using DatabaseSchemaReader.CodeGen;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.Codegen
{
    [TestClass]
    public class ClassWriterCompositeKeyTest
    {
        [TestMethod]
        public void TestCompositeKey()
        {
            //arrange
            var schema = Arrange();
            var table = schema.FindTableByName("Origin");
            var cw = new ClassWriter(table, new CodeWriterSettings());

            //act
            var txt = cw.Write();

            //assert
            //virtual of type tableName+Key named Key
            var hasKey = txt.Contains("public virtual OriginKey Key { get; set; }");
            Assert.IsTrue(hasKey);
        }

        [TestMethod]
        public void TestCompositeKeyWithCodeFirst()
        {
            //arrange
            var schema = Arrange();
            var table = schema.FindTableByName("Origin");
            var cw = new ClassWriter(table, new CodeWriterSettings { CodeTarget = CodeTarget.PocoEntityCodeFirst });

            //act
            var txt = cw.Write();

            //assert
            //non virtual actual properties
            var hasKey = txt.Contains("public string OriginKey1 { get; set; }");
            Assert.IsTrue(hasKey);
        }

        [TestMethod]
        public void TestCompositeForeignKey()
        {
            //arrange
            var schema = Arrange();
            var table = schema.FindTableByName("Products");
            var cw = new ClassWriter(table, new CodeWriterSettings());

            //act
            var txt = cw.Write();

            //assert
            //virtual of type tableName
            var hasKey = txt.Contains("public virtual Origin Origin { get; set; }");
            Assert.IsTrue(hasKey);
        }

        [TestMethod]
        public void TestCompositeForeignKeyCodeFirst()
        {
            //arrange
            var schema = Arrange();
            var table = schema.FindTableByName("Products");
            var cw = new ClassWriter(table, new CodeWriterSettings
            {
                CodeTarget = CodeTarget.PocoEntityCodeFirst,
                UseForeignKeyIdProperties = true
            });

            //act
            var txt = cw.Write();

            //assert
            //virtual of type tableName+Key named Key
            var hasKeyId1 = txt.Contains("public string OriginKey1 { get; set; }");
            var hasKeyId2 = txt.Contains("public string OriginKey2 { get; set; }");
            var hasForeignKey = txt.Contains("public virtual Origin Origin { get; set; }");
            Assert.IsTrue(hasKeyId1);
            Assert.IsTrue(hasKeyId2);
            Assert.IsTrue(hasForeignKey);
        }

        private static DatabaseSchema Arrange()
        {
            const string key1 = "OriginKey1";
            const string key2 = "OriginKey2";

            var schema = new DatabaseSchema(null, null);
            var table = schema.AddTable("Products")
                              .AddColumn("ProductId", "INT").AddPrimaryKey().AddIdentity()
                              .AddColumn("ProductName", "NVARCHAR")
                              .AddColumn(key1, "NVARCHAR").AddForeignKey("Origin")
                              .AddColumn(key2, "NVARCHAR")
                              .Table;

            table.ForeignKeys.Single().AddColumn(table.FindColumn(key2));

            var pk = new DatabaseConstraint { ConstraintType = ConstraintType.PrimaryKey, };
            pk.Columns.Add(key1);
            pk.Columns.Add(key2);
            schema.AddTable("Origin")
                  .AddColumn<string>(key1).AddPrimaryKey()
                  .AddColumn<string>(key2)
                  .Table.AddConstraint(pk);

            schema.DataTypes.Add(new DataType("INT", "System.Int32"));
            schema.DataTypes.Add(new DataType("NVARCHAR", "System.String"));
            DatabaseSchemaFixer.UpdateDataTypes(schema);
            //make sure .Net names are assigned
            PrepareSchemaNames.Prepare(schema, new Namer());

            return schema;
        }
    }
}
