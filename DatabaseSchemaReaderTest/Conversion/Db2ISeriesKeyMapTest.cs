using System.Data;
using System.Globalization;
using DatabaseSchemaReader.Conversion.KeyMaps;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.Conversion
{
    [TestClass]
    public class Db2ISeriesKeyMapTest
    {
        [TestMethod]
        public void KeyMapForColumnSchemaTest()
        {
            //arrange
            var dataTable = new DataTable("Columns") { Locale = CultureInfo.InvariantCulture };
            dataTable.Columns.Add("CatalogName", typeof(string));
            dataTable.Columns.Add("SchemaName", typeof(string));
            dataTable.Columns.Add("TableName", typeof(string));
            dataTable.Columns.Add("ColumnName", typeof(string));
            dataTable.Columns.Add("OrdinalPosition", typeof(int));
            dataTable.Columns.Add("ColumnDefault", typeof(string));
            dataTable.Columns.Add("IsNullable", typeof(bool));
            dataTable.Columns.Add("DataType", typeof(string));
            dataTable.Columns.Add("CharacterMaximumLength", typeof(int));
            dataTable.Columns.Add("CharacterOctetLength", typeof(int));
            dataTable.Columns.Add("NumericPrecision", typeof(int));
            dataTable.Columns.Add("NumericPrecisionRadix", typeof(int));
            dataTable.Columns.Add("NumericScale", typeof(int));

            //act
            var keymap = new ColumnsKeyMap(dataTable);

            //assert
            Assert.AreEqual("SchemaName", keymap.SchemaKey);
            Assert.AreEqual("TableName", keymap.TableKey);
            Assert.AreEqual("ColumnName", keymap.Key);
            Assert.AreEqual("OrdinalPosition", keymap.OrdinalKey);
            Assert.AreEqual("ColumnDefault", keymap.DefaultKey);
            Assert.AreEqual("IsNullable", keymap.NullableKey);
            Assert.AreEqual("datatype", keymap.DatatypeKey);
            Assert.AreEqual("CharacterMaximumLength", keymap.LengthKey);
            Assert.AreEqual("NumericPrecision", keymap.PrecisionKey);
            Assert.AreEqual("NumericScale", keymap.ScaleKey);
        }

        [TestMethod]
        public void KeyMapForIndexSchemaTest()
        {
            //arrange
            var dataTable = new DataTable("Indexes") { Locale = CultureInfo.InvariantCulture };
            dataTable.Columns.Add("ConstraintCatalog", typeof(string));
            dataTable.Columns.Add("ConstraintSchema", typeof(string));
            dataTable.Columns.Add("ConstraintName", typeof(string));
            dataTable.Columns.Add("CatalogName", typeof(string));
            dataTable.Columns.Add("SchemaName", typeof(string));
            dataTable.Columns.Add("TableName", typeof(string));
            dataTable.Columns.Add("ConstraintType", typeof(string));

            //act
            var keymap = new IndexKeyMap(dataTable);

            //assert
            Assert.AreEqual("SchemaName", keymap.SchemaKey);
            Assert.AreEqual("TableName", keymap.TableKey);
            Assert.AreEqual("ConstraintName", keymap.Key);
            Assert.AreEqual("ConstraintType", keymap.Typekey);
        }
    }
}
