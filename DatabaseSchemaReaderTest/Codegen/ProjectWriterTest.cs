using DatabaseSchemaReader.CodeGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.Codegen
{
    [TestClass]
    public class ProjectWriterTest
    {
        private ProjectWriter CreateProjectWriter(string @namespace, ProjectVersion projectVersion)
        {
            var pw = new ProjectWriter(@namespace, projectVersion);
            return pw;
        }

        [TestMethod]
        public void CreateVs2008Project()
        {
            //arrange
            var pw = CreateProjectWriter("Domain", ProjectVersion.Vs2008);
            pw.AddNHibernateReference();
            pw.AddClass("x.cs");
            pw.AddPackagesConfig();
            //act
            var xml = pw.Write();
            //assert
            Assert.IsTrue(xml.Contains("<Project ToolsVersion=\"3.5\""), "VS2008 tools");
            Assert.IsTrue(xml.Contains("<TargetFrameworkVersion>v3.5</TargetFrameworkVersion>"), "Targetting .net v3.5");
            Assert.IsTrue(xml.Contains("<Reference Include=\"NHibernate"),"NHib ref included");
            Assert.IsTrue(xml.Contains("<None Include=\"packages.config\" />"), "Packages.config included");
            Assert.IsTrue(xml.Contains("<Compile Include=\"x.cs\" "),"Class included");
        }

        [TestMethod]
        public void CreateVs2010Project()
        {
            //arrange
            var pw = CreateProjectWriter("Domain", ProjectVersion.Vs2010);
            pw.AddEntityFrameworkReference();
            pw.AddOracleReference();
            //act
            var xml = pw.Write();
            //assert
            Assert.IsTrue(xml.Contains("<Project ToolsVersion=\"4.0\""), "VS2010 tools");
            Assert.IsTrue(xml.Contains("<TargetFrameworkVersion>v4.0</TargetFrameworkVersion>"), "Targetting .net v4.0");
            Assert.IsTrue(xml.Contains("<Reference Include=\"System.Data.OracleClient\" />"), "System.Data.Oracle ref included");
            Assert.IsTrue(xml.Contains("\\net40\\EntityFramework.dll</HintPath>"), "Hintpath to .net4 version of EF");
        }

        [TestMethod]
        public void CreateVs2015Project()
        {
            //arrange
            var pw = CreateProjectWriter("Domain", ProjectVersion.Vs2015);
            pw.AddEntityFrameworkReference();
            pw.AddDevartOracleReference();
            //act
            var xml = pw.Write();
            //assert
            Assert.IsTrue(xml.Contains("<Project ToolsVersion=\"14.0\""), "VS2015 tools");
            Assert.IsTrue(xml.Contains("<TargetFrameworkVersion>v4.6.1</TargetFrameworkVersion>"), "Targetting .net v4.6.1");
            Assert.IsTrue(xml.Contains("<Reference Include=\"Devart.Data.Oracle.Entity.EF6,"), "Devart ref included");
        }


        [TestMethod]
        public void CreateVs2015ProjectWithManagedClient()
        {
            //arrange
            var pw = CreateProjectWriter("Domain", ProjectVersion.Vs2015);
            pw.AddEntityFrameworkReference();
            pw.AddOracleManagedReference();
            //act
            var xml = pw.Write();
            //assert
            Assert.IsTrue(xml.Contains("lib\\net45\\Oracle.ManagedDataAccess.EntityFramework.dll</HintPath>"), "Oracle Managed in net45 ref included");
        }
    }
}
