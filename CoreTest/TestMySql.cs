using System;
using Xunit;

namespace DatabaseSchemaReaderTest
{
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

        [Fact]
        public void RunTableList()
        {
            using (var connection = new MySql.Data.MySqlClient.MySqlConnection(MySql))
            {
                var dr = new DatabaseSchemaReader.DatabaseReader(connection) {Owner = "sakila"};
                try
                {
                    //var schema = dr.ReadAll();
                    var tableList = dr.TableList();
                    //var tables = dr.AllTables();
                    //var views = dr.AllViews();
                    Assert.NotEmpty(tableList);
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
    }
}