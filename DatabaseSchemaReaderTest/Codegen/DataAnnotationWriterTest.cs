using System;
using System.Linq;
using DatabaseSchemaReader.CodeGen;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.Codegen
{


    /// <summary>
    ///Test the data annotations
    ///</summary>
    [TestClass]
    public class DataAnnotationWriterTest
    {
        [TestMethod]
        public void TestRequired()
        {
            //arrange
            var settings = new CodeWriterSettings();
            settings.RequiredErrorMessage = null;
            var classBuilder = new ClassBuilder();
            var column = new DatabaseColumn();
            column.Name = column.NetName = "Important";
            column.DataType = new DataType("NVARCHAR2", "System.String");
            column.Nullable = false;

            var target = new DataAnnotationWriter(true, settings);

            //act
            target.Write(classBuilder, column);
            var result = classBuilder.ToString().Trim(); //ignore lines

            //assert
            Assert.AreEqual("[Required]", result);
        }

        [TestMethod]
        public void TestRequiredWithErrorMessage()
        {
            //arrange
            var settings = new CodeWriterSettings();
            settings.RequiredErrorMessage = "This is mandatory";
            var classBuilder = new ClassBuilder();
            var column = new DatabaseColumn();
            column.Name = column.NetName = "Important";
            column.DataType = new DataType("NVARCHAR2", "System.String");
            column.Nullable = false;

            var target = new DataAnnotationWriter(true, settings);

            //act
            target.Write(classBuilder, column);
            var result = classBuilder.ToString().Trim(); //ignore lines

            //assert
            Assert.AreEqual("[Required(ErrorMessage=\"This is mandatory\")]", result);
        }


        [TestMethod]
        public void TestRequiredWithErrorMessageFormat()
        {
            //arrange
            var settings = new CodeWriterSettings();
            settings.RequiredErrorMessage = "{0} is mandatory";
            var classBuilder = new ClassBuilder();
            var column = new DatabaseColumn();
            column.Name = column.NetName = "Important";
            column.DataType = new DataType("NVARCHAR2", "System.String");
            column.Nullable = false;

            var target = new DataAnnotationWriter(true, settings);

            //act
            target.Write(classBuilder, column);
            var result = classBuilder.ToString().Trim(); //ignore lines

            //assert
            Assert.AreEqual("[Required(ErrorMessage=\"Important is mandatory\")]", result);
        }

        [TestMethod]
        public void TestStringLength()
        {
            //arrange
            var settings = new CodeWriterSettings();
            settings.StringLengthErrorMessage = null;
            var classBuilder = new ClassBuilder();
            var column = new DatabaseColumn();
            column.Name = column.NetName = "Name";
            column.DataType = new DataType("NVARCHAR2", "System.String");
            column.Nullable = true;
            column.Length = 10;

            var target = new DataAnnotationWriter(true, settings);

            //act
            target.Write(classBuilder, column);
            var result = classBuilder.ToString().Trim(); //ignore lines

            //assert
            Assert.AreEqual("[StringLength(10)]", result);
        }


        [TestMethod]
        public void TestStringLengthWithErrorMessage()
        {
            //arrange
            var settings = new CodeWriterSettings();
            settings.StringLengthErrorMessage = "Is not the correct length";
            var classBuilder = new ClassBuilder();
            var column = new DatabaseColumn();
            column.Name = column.NetName = "Name";
            column.DataType = new DataType("NVARCHAR2", "System.String");
            column.Nullable = true;
            column.Length = 10;

            var target = new DataAnnotationWriter(true, settings);

            //act
            target.Write(classBuilder, column);
            var result = classBuilder.ToString().Trim(); //ignore lines

            //assert
            Assert.AreEqual("[StringLength(10, ErrorMessage=\"Is not the correct length\")]", result);
        }

        [TestMethod]
        public void TestStringLengthWithErrorMessageFormat1()
        {
            //arrange
            var settings = new CodeWriterSettings();
            settings.StringLengthErrorMessage = "Maximum length is {0}";
            var classBuilder = new ClassBuilder();
            var column = new DatabaseColumn();
            column.Name = column.NetName = "Name";
            column.DataType = new DataType("NVARCHAR2", "System.String");
            column.Nullable = true;
            column.Length = 10;

            var target = new DataAnnotationWriter(true, settings);

            //act
            target.Write(classBuilder, column);
            var result = classBuilder.ToString().Trim(); //ignore lines

            //assert
            Assert.AreEqual("[StringLength(10, ErrorMessage=\"Maximum length is 10\")]", result);
        }

        [TestMethod]
        public void TestStringLengthWithErrorMessageFormat2()
        {
            //arrange
            var settings = new CodeWriterSettings();
            settings.StringLengthErrorMessage = "{1} has maximum length of {0}";
            var classBuilder = new ClassBuilder();
            var column = new DatabaseColumn();
            column.Name = column.NetName = "Name";
            column.DataType = new DataType("NVARCHAR2", "System.String");
            column.Nullable = true;
            column.Length = 10;

            var target = new DataAnnotationWriter(true, settings);

            //act
            target.Write(classBuilder, column);
            var result = classBuilder.ToString().Trim(); //ignore lines

            //assert
            Assert.AreEqual("[StringLength(10, ErrorMessage=\"Name has maximum length of 10\")]", result);
        }


        [TestMethod]
        public void TestRange()
        {
            //arrange
            var settings = new CodeWriterSettings();
            settings.RangeErrorMessage = null;
            var classBuilder = new ClassBuilder();
            var column = new DatabaseColumn();
            column.Name = column.NetName = "Name";
            column.DataType = new DataType("INT", "System.Int32");
            column.Nullable = true;
            column.Precision = 5;
            column.Scale = 0;

            var target = new DataAnnotationWriter(true, settings);

            //act
            target.Write(classBuilder, column);
            var result = classBuilder.ToString().Trim(); //ignore lines

            //assert
            Assert.AreEqual("[Range(0, 99999)]", result);
        }


        [TestMethod]
        public void TestRangeWithErrorMessage()
        {
            //arrange
            var settings = new CodeWriterSettings();
            settings.RangeErrorMessage = "Outside of range";
            var classBuilder = new ClassBuilder();
            var column = new DatabaseColumn();
            column.Name = column.NetName = "Name";
            column.DataType = new DataType("INT", "System.Int32");
            column.Nullable = true;
            column.Precision = 5;
            column.Scale = 0;

            var target = new DataAnnotationWriter(true, settings);

            //act
            target.Write(classBuilder, column);
            var result = classBuilder.ToString().Trim(); //ignore lines

            //assert
            Assert.AreEqual("[Range(0, 99999, ErrorMessage=\"Outside of range\")]", result);
        }


        [TestMethod]
        public void TestRangeWithErrorMessageFormat1()
        {
            //arrange
            var settings = new CodeWriterSettings();
            settings.RangeErrorMessage = "Must be less than {0}";
            var classBuilder = new ClassBuilder();
            var column = new DatabaseColumn();
            column.Name = column.NetName = "Name";
            column.DataType = new DataType("INT", "System.Int32");
            column.Nullable = true;
            column.Precision = 5;
            column.Scale = 0;

            var target = new DataAnnotationWriter(true, settings);

            //act
            target.Write(classBuilder, column);
            var result = classBuilder.ToString().Trim(); //ignore lines

            //assert
            Assert.AreEqual("[Range(0, 99999, ErrorMessage=\"Must be less than 99999\")]", result);
        }

        [TestMethod]
        public void TestRangeWithErrorMessageFormat2()
        {
            //arrange
            var settings = new CodeWriterSettings();
            settings.RangeErrorMessage = "{1} must be less than {0}";
            var classBuilder = new ClassBuilder();
            var column = new DatabaseColumn();
            column.Name = column.NetName = "Name";
            column.DataType = new DataType("INT", "System.Int32");
            column.Nullable = true;
            column.Precision = 5;
            column.Scale = 0;

            var target = new DataAnnotationWriter(true, settings);

            //act
            target.Write(classBuilder, column);
            var result = classBuilder.ToString().Trim(); //ignore lines

            //assert
            Assert.AreEqual("[Range(0, 99999, ErrorMessage=\"Name must be less than 99999\")]", result);
        }


        [TestMethod]
        public void TestDecimalRange()
        {
            //arrange
            var settings = new CodeWriterSettings();
            settings.RangeErrorMessage = null;
            var classBuilder = new ClassBuilder();
            var column = new DatabaseColumn();
            column.Name = column.NetName = "Name";
            column.DataType = new DataType("NUMBER", "System.Decimal");
            column.Nullable = true;
            column.Precision = 5;
            column.Scale = 1;

            var target = new DataAnnotationWriter(true, settings);

            //act
            target.Write(classBuilder, column);
            var result = classBuilder.ToString().Trim(); //ignore lines

            //assert
            Assert.AreEqual("[Range(typeof(decimal), \"0\", \"9999\")]", result);
        }


        [TestMethod]
        public void TestDecimalRangeWithErrorMessage()
        {
            //arrange
            var settings = new CodeWriterSettings();
            settings.RangeErrorMessage = "{1} must be less than {0}";
            var classBuilder = new ClassBuilder();
            var column = new DatabaseColumn();
            column.Name = column.NetName = "Name";
            column.DataType = new DataType("NUMBER", "System.Decimal");
            column.Nullable = true;
            column.Precision = 5;
            column.Scale = 1;

            var target = new DataAnnotationWriter(true, settings);

            //act
            target.Write(classBuilder, column);
            var result = classBuilder.ToString().Trim(); //ignore lines

            //assert
            Assert.AreEqual("[Range(typeof(decimal), \"0\", \"9999\", ErrorMessage=\"Name must be less than 9999\")]", result);
        }



        [TestMethod]
        public void TestIndex()
        {
            //arrange
            var settings = new CodeWriterSettings();
            settings.CodeTarget = CodeTarget.PocoEntityCodeFirst;
            settings.WriteCodeFirstIndexAttribute = true;

            var classBuilder = new ClassBuilder();
            var table = new DatabaseTable { Name = "Test" };
            var nameColumn = table.AddColumn<int>("Id").AddPrimaryKey()
                .AddColumn<string>("Name").AddNullable().AddIndex("IX_NAME");


            var target = new DataAnnotationWriter(true, settings);

            //act
            target.Write(classBuilder, nameColumn);
            var result = classBuilder.ToString().Trim(); //ignore lines

            //assert
            Assert.AreEqual("[Index(\"IX_NAME\")]", result);
        }


        [TestMethod]
        public void TestIndexUnique()
        {
            //arrange
            var settings = new CodeWriterSettings();
            settings.CodeTarget = CodeTarget.PocoEntityCodeFirst;
            settings.WriteCodeFirstIndexAttribute = true;

            var classBuilder = new ClassBuilder();
            var table = new DatabaseTable { Name = "Test" };
            var nameColumn = table.AddColumn<int>("Id").AddPrimaryKey()
                .AddColumn<string>("Name").AddNullable().AddIndex("IX_NAME");
            table.Indexes.First().IsUnique = true;

            var target = new DataAnnotationWriter(true, settings);

            //act
            target.Write(classBuilder, nameColumn);
            var result = classBuilder.ToString().Trim(); //ignore lines

            //assert
            Assert.AreEqual("[Index(\"IX_NAME\", IsUnique = true)]", result);
        }

        [TestMethod]
        public void TestIndexMultiColumn()
        {
            //arrange
            var settings = new CodeWriterSettings();
            settings.CodeTarget = CodeTarget.PocoEntityCodeFirst;
            settings.WriteCodeFirstIndexAttribute = true;

            var classBuilder = new ClassBuilder();
            var table = new DatabaseTable { Name = "Test" };
            var nameColumn = table.AddColumn<int>("Id").AddPrimaryKey()
                .AddColumn<string>("Category").AddNullable().AddIndex("IX_NAME")
                .AddColumn<string>("Name").AddNullable().AddIndex("IX_NAME")
                ;

            var target = new DataAnnotationWriter(true, settings);

            //act
            target.Write(classBuilder, nameColumn);
            var result = classBuilder.ToString().Trim(); //ignore lines

            //assert
            Assert.AreEqual("[Index(\"IX_NAME\", 2)]", result);
        }

        [TestMethod]
        public void TestIndexMultiColumnUnique()
        {
            //arrange
            var settings = new CodeWriterSettings();
            settings.CodeTarget = CodeTarget.PocoEntityCodeFirst;
            settings.WriteCodeFirstIndexAttribute = true;

            var classBuilder = new ClassBuilder();
            var table = new DatabaseTable { Name = "Test" };
            var nameColumn = table.AddColumn<int>("Id").AddPrimaryKey()
                .AddColumn<string>("Category").AddNullable().AddIndex("IX_NAME")
                .AddColumn<string>("Name").AddNullable().AddIndex("IX_NAME")
                ;
            table.Indexes.First().IsUnique = true;

            var target = new DataAnnotationWriter(true, settings);

            //act
            target.Write(classBuilder, nameColumn);
            var result = classBuilder.ToString().Trim(); //ignore lines

            //assert
            Assert.AreEqual("[Index(\"IX_NAME\", 2, IsUnique = true)]", result);
        }

        [TestMethod]
        public void TestIndexNotNeededForPrimaryKey()
        {
            //arrange
            var settings = new CodeWriterSettings();
            settings.CodeTarget = CodeTarget.PocoEntityCodeFirst;
            settings.WriteCodeFirstIndexAttribute = true;

            var classBuilder = new ClassBuilder();
            var table = new DatabaseTable { Name = "Test" };
            table.AddColumn<int>("Id").AddPrimaryKey().AddIndex("PK_TEST")
                .AddColumn<string>("Category").AddNullable()
                .AddColumn<string>("Name").AddNullable().AddIndex("IX_NAME")
                ;
            var idColumn = table.PrimaryKeyColumn;

            var target = new DataAnnotationWriter(true, settings);

            //act
            target.Write(classBuilder, idColumn);
            var result = classBuilder.ToString().Trim(); //ignore lines

            //assert
            Assert.IsTrue(result.IndexOf("[Index", StringComparison.OrdinalIgnoreCase) == -1, "Should be just[Key]");
        }
    }
}
