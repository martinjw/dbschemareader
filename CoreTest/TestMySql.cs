using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DatabaseSchemaReader.Procedures;

namespace CoreTest
{
    [TestClass]
    public class TestMySql
    {
        public static string MySql
        {
            get
            {
                if (string.Equals("True", Environment.GetEnvironmentVariable("APPVEYOR")))
                {
                    return @"Server=localhost;Uid=root;Pwd=Password12!;Database=sakila;Allow User Variables=True;";
                }
                //had to add SslMode=None because of a NotImplemented error
                return @"Server=localhost;Uid=root;Pwd=mysql;Database=sakila;Allow User Variables=True;SslMode=None";
            }
        }

        [TestMethod]
        public void RunTableList()
        {
            using (var connection = new MySql.Data.MySqlClient.MySqlConnection(MySql))
            {
                var dr = new DatabaseSchemaReader.DatabaseReader(connection);
                dr.Owner = "sakila";
                try
                {
                    var schema = dr.ReadAll();
                    var tableList = dr.TableList();
                    var tables = dr.AllTables();
                    var views = dr.AllViews();
                    Assert.IsTrue(tableList.Count > 0);
                }
                catch (System.Resources.MissingManifestResourceException)
                {
                    Console.WriteLine("MySql Core error");
                    //System.Resources.MissingManifestResourceException : 
                    //Could not find any resources appropriate for the specified culture or the neutral culture.  
                    //Make sure "MySql.Data.Resources.resources" was correctly embedded or linked into assembly "MySql.Data.Core" at compile time, 
                    //or that all the satellite assemblies required are loadable and fully signed.
                }
            }
        }

        //August 2017- MySql client is still at 1.6, so this won't work yet
        //[TestMethod]
        //public void ReadResultSets()
        //{
        //    using (var connection = new MySql.Data.MySqlClient.MySqlConnection(MySql))
        //    {
        //        connection.Open();
        //        var dr = new DatabaseSchemaReader.DatabaseReader(connection);
        //        dr.AllStoredProcedures();
        //        var schema = dr.DatabaseSchema;

        //        var rsr = new ResultSetReader(schema);
        //        rsr.Execute(connection);

        //        var sproc = schema.StoredProcedures.Find(x => x.Name == "SalesByCategory");
        //        Assert.IsNotNull(sproc);
        //        var rs = sproc.ResultSets.First();
        //        Assert.IsNotNull(rs, "Stored procedure should return a result");

        //    }
        //}
    }
}