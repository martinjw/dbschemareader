using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen.SqlServer;
#if !NUNIT
using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
using TestContext = System.Object;
#endif

namespace DatabaseSchemaReaderTest.SqlGen.SqlServer
{
    /// <summary>
    /// Summary description for AccessCheckConstraintTest
    /// </summary>
    [TestClass]
    public class AccessCheckConstraintTest
    {
        [TestMethod]
        public void TestAccessCheckConstraint()
        {
            //arrange
            var table = new DatabaseTable {Name = "Orders"};

            var check = new DatabaseConstraint
                            {
                                ConstraintType = ConstraintType.Check,
                                Expression = "> 0",
                                Name = "[Orders].[Quantity].ValidationRule"
                            };

            table.AddConstraint(check);

            var writer = new ConstraintWriter(table);

            //act
            var txt = writer.WriteCheckConstraints();

            //assert
            Assert.IsTrue(txt.Contains("ALTER TABLE [Orders] ADD CONSTRAINT [Orders_Quantity_ValidationRule] CHECK ([Orders].[Quantity] > 0);"));
        }

        [TestMethod]
        public void TestAccessDateCheckConstraint()
        {
            //arrange
            var table = new DatabaseTable {Name = "Orders"};

            var check = new DatabaseConstraint
                            {
                                ConstraintType = ConstraintType.Check,
                                Expression = ">Date()",
                                Name = "[Orders].[OrderDate].ValidationRule"
                            };

            table.AddConstraint(check);

            var writer = new TableGenerator(table);

            //act
            var txt = writer.Write();

            //assert
            Assert.IsTrue(txt.Contains("ALTER TABLE [Orders] ADD CONSTRAINT [Orders_OrderDate_ValidationRule] CHECK ([Orders].[OrderDate] >getdate());"));
        }

        [TestMethod]
        public void TestSqlServerCheckConstraint()
        {
            //arrange
            var table = new DatabaseTable {Name = "Orders"};

            var check = new DatabaseConstraint
                            {
                                ConstraintType = ConstraintType.Check,
                                Expression = "[Quantity] > 0",
                                Name = "ValidationRule"
                            };

            table.AddConstraint(check);

            var writer = new ConstraintWriter(table);

            //act
            var txt = writer.WriteCheckConstraints();

            //assert
            Assert.IsTrue(txt.Contains("ALTER TABLE [Orders] ADD CONSTRAINT [ValidationRule] CHECK ([Quantity] > 0);"));
        }


        [TestMethod]
        public void TestSqlServerGetDateCheckConstraint()
        {
            //arrange
            var table = new DatabaseTable {Name = "Orders"};

            var check = new DatabaseConstraint
                            {
                                ConstraintType = ConstraintType.Check,
                                Expression = "[OrderDate] >getDate()",
                                Name = "ValidationRule"
                            };

            table.AddConstraint(check);

            var writer = new TableGenerator(table);

            //act
            var txt = writer.Write();

            //assert
            Assert.IsTrue(txt.Contains("ALTER TABLE [Orders] ADD CONSTRAINT [ValidationRule] CHECK ([OrderDate] >getDate());"));
        }
    }
}
