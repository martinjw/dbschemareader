using System;
using System.Linq;
using System.Collections.Generic;
using DatabaseSchemaReader.Compare;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.Compare
{
    [TestClass]
    public class CompareViewsTest
    {
        [TestMethod]
        public void WhenViewsIdentical()
        {
            //arrange
            var sb = new List<CompareResult>();
            var writer = new ComparisonWriter(SqlType.SqlServer);
            var target = new CompareViews(sb, writer);

            var baseViews = new List<DatabaseView> { CreateView() };
            var compareViews = new List<DatabaseView> { CreateView() };

            //act
            target.Execute(baseViews, compareViews);
            var result = string.Join(Environment.NewLine, sb.Select(x => x.Script).ToArray());

            //assert
            Assert.IsTrue(string.IsNullOrEmpty(result));
        }

        [TestMethod]
        public void WhenViewDropped()
        {
            //arrange
            var sb = new List<CompareResult>();
            var writer = new ComparisonWriter(SqlType.SqlServer);
            var target = new CompareViews(sb, writer);

            var baseViews = new List<DatabaseView> { CreateView() };
            var compareViews = new List<DatabaseView>();

            //act
            target.Execute(baseViews, compareViews);
            var result = string.Join(Environment.NewLine, sb.Select(x => x.Script).ToArray());

            //assert
            Assert.IsTrue(result.Contains("DROP VIEW"));
        }

        [TestMethod]
        public void WhenViewAdded()
        {
            //arrange
            var sb = new List<CompareResult>();
            var writer = new ComparisonWriter(SqlType.SqlServer);
            var target = new CompareViews(sb, writer);

            var baseViews = new List<DatabaseView>();
            var compareViews = new List<DatabaseView> { CreateView() };

            //act
            target.Execute(baseViews, compareViews);
            var result = string.Join(Environment.NewLine, sb.Select(x => x.Script).ToArray());

            //assert
            Assert.IsTrue(result.Contains("CREATE VIEW"));
        }


        [TestMethod]
        public void WhenViewChanged()
        {
            //arrange
            var sb = new List<CompareResult>();
            var writer = new ComparisonWriter(SqlType.SqlServer);
            var target = new CompareViews(sb, writer);

            var baseViews = new List<DatabaseView> { CreateView() };
            var view = CreateView();
            const string orderByName = " ORDER BY NAME";
            view.Sql += orderByName;
            var compareViews = new List<DatabaseView> { view };

            //act
            target.Execute(baseViews, compareViews);
            var result = string.Join(Environment.NewLine, sb.Select(x => x.Script).ToArray());

            //assert
            Assert.IsTrue(result.Contains("DROP VIEW"));
            Assert.IsTrue(result.Contains("CREATE VIEW"));
            Assert.IsTrue(result.Contains(orderByName));
        }

        [TestMethod]
        public void WhenViewChangedInverse()
        {
            //arrange
            var sb = new List<CompareResult>();
            var writer = new ComparisonWriter(SqlType.SqlServer);
            var target = new CompareViews(sb, writer);

            var view = CreateView();
            const string orderByName = " ORDER BY NAME";
            view.Sql += orderByName;
            var baseViews = new List<DatabaseView> { view };
            var compareViews = new List<DatabaseView> { CreateView() };

            //act
            target.Execute(baseViews, compareViews);
            var result = string.Join(Environment.NewLine, sb.Select(x => x.Script).ToArray());

            //assert
            Assert.IsTrue(result.Contains("DROP VIEW"));
            Assert.IsTrue(result.Contains("CREATE VIEW"));
            Assert.IsFalse(result.Contains(orderByName));
        }
        private static DatabaseView CreateView()
        {
            return new DatabaseView { Name = "MyView", Sql = "CREATE VIEW MyView AS SELECT * FROM TABLE" };
        }



        [TestMethod]
        public void WhenViewsIdenticalWithDifferentFormatSql()
        {
            //arrange
            var sb = new List<CompareResult>();
            var writer = new ComparisonWriter(SqlType.SqlServer);
            var target = new CompareViews(sb, writer);

            var databaseView = CreateView();
            databaseView.Sql = @"
create view ""Alphabetical list of products"" AS
SELECT Products.*, Categories.CategoryName
FROM Categories INNER JOIN Products ON Categories.CategoryID = Products.CategoryID
WHERE (((Products.Discontinued)=0))
";
            var baseViews = new List<DatabaseView> { databaseView };
            var databaseView2 = CreateView();
            databaseView2.Sql = @"create view [dbo].[Alphabetical list of products] AS
SELECT Products.*, Categories.CategoryName
FROM Categories INNER JOIN Products ON Categories.CategoryID = Products.CategoryID
WHERE (((Products.Discontinued)=0))";
            var compareViews = new List<DatabaseView> { databaseView2 };

            //act
            target.Execute(baseViews, compareViews);
            var result = string.Join(Environment.NewLine, sb.Select(x=>x.Script).ToArray());

            //assert
            Assert.IsTrue(string.IsNullOrEmpty(result));
        }
    }
}
