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
                var dr = new DatabaseSchemaReader.DatabaseReader(connection);
                dr.Owner = "sakila";
                var schema = dr.ReadAll();
                var tableList = dr.TableList();
                var tables = dr.AllTables();
                var views = dr.AllViews();
                Assert.NotEmpty(tableList);
            }
        }
    }
}