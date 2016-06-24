using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.DataSchema
{
    /// <summary>
    /// Summary description for CanSerializeTest
    /// </summary>
    [TestClass]
    public class CanSerializeTest
    {

        [TestMethod]
        public void BinarySerializeTest()
        {
            var dbReader = TestHelper.GetNorthwindReader();
            var schema = dbReader.ReadAll();

            var f = new BinaryFormatter();

            using (var stm = new FileStream("schema.bin", FileMode.Create))
            {
                f.Serialize(stm, schema);
            }

            DatabaseSchema clone;
            using (var stm = new FileStream("schema.bin", FileMode.Open))
            {
                clone = (DatabaseSchema)f.Deserialize(stm);
            }

            Assert.AreEqual(schema.DataTypes.Count, clone.DataTypes.Count);
            Assert.AreEqual(schema.StoredProcedures.Count, clone.StoredProcedures.Count);
            Assert.AreEqual(schema.Tables.Count, clone.Tables.Count);
            Assert.AreEqual(schema.Tables[0].Columns.Count, clone.Tables[0].Columns.Count);
        }


        [TestMethod]
        public void XmlSerializeTest()
        {
            var dbReader = TestHelper.GetNorthwindReader();
            var schema = dbReader.ReadAll();

            var f = new System.Xml.Serialization.XmlSerializer(schema.GetType());
            using (var stm = new FileStream("schema.xml", FileMode.Create))
            {
                f.Serialize(stm, schema);
            }

            DatabaseSchema clone;
            using (var stm = new FileStream("schema.xml", FileMode.Open))
            {
                clone = (DatabaseSchema)f.Deserialize(stm);
            }

            //the clone has lost some useful cross linking.

            Assert.AreEqual(schema.DataTypes.Count, clone.DataTypes.Count);
            Assert.AreEqual(schema.StoredProcedures.Count, clone.StoredProcedures.Count);
            Assert.AreEqual(schema.Tables.Count, clone.Tables.Count);
            Assert.AreEqual(schema.Tables[0].Columns.Count, clone.Tables[0].Columns.Count);
        }

        [TestMethod]
        public void DataContractSerializeTest()
        {
            var dbReader = TestHelper.GetNorthwindReader();
            var schema = dbReader.ReadAll();

            //XmlSerializer won't work because there are circular dependencies
            //var f = new XmlSerializer(schema.GetType());

            var f = new DataContractSerializer(schema.GetType(), "DatabaseSchema", "SchemaReader", new List<Type>(), 32767, false, true, null);

            using (var stm = new FileStream("schema.xml", FileMode.Create))
            {
                f.WriteObject(stm, schema);
            }

            DatabaseSchema clone;
            using (var stm = new FileStream("schema.xml", FileMode.Open))
            {
                clone = (DatabaseSchema)f.ReadObject(stm);
            }

            Assert.AreEqual(schema.DataTypes.Count, clone.DataTypes.Count);
            Assert.AreEqual(schema.StoredProcedures.Count, clone.StoredProcedures.Count);
            Assert.AreEqual(schema.Tables.Count, clone.Tables.Count);
            Assert.AreEqual(schema.Tables[0].Columns.Count, clone.Tables[0].Columns.Count);
        }

        [TestMethod]
        public void UseEqualityTest()
        {

            //NamedSchemaObject = has a name and schemaOwner
            var schema = new DatabaseSchema(null, null);
            schema.Owner = "dbo";
            schema.AddTable("A").AddTable("A");
            var distinct = schema.Tables.Distinct().Count();
            Assert.AreEqual(1, distinct);

            //NamedObject = has a name
            var cols = new List<DatabaseColumn> {new DatabaseColumn {Name = "Id"}, new DatabaseColumn {Name = "Name"}};
            var cols2 = new List<DatabaseColumn> { new DatabaseColumn { Name = "Id" }, new DatabaseColumn { Name = "Name" } };

            var union = cols.Union(cols2);

            Assert.AreEqual(2, union.Count());

        }



    }
}
