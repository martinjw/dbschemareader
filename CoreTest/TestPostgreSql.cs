﻿using System;
using Xunit;

namespace DatabaseSchemaReaderTest
{
    public class TestPostgreSql
    {
        public static string PostgreSql
        {
            get
            {
                if (string.Equals("True", Environment.GetEnvironmentVariable("APPVEYOR")))
                {
                    return @"Server=127.0.0.1;User id=postgres;Pwd=Password12!;database=world;";
                }
                return @"Server=127.0.0.1;User id=postgres;password=sql;database=world;";
            }
        }

        [Fact]
        public void RunTableList()
        {
            //couldn't get package to restore.
            using (var connection = new Npgsql.NpgsqlConnection(PostgreSql))
            {
                var dr = new DatabaseSchemaReader.DatabaseReader(connection);
                try
                {
                    //var schema = dr.ReadAll();
                    var tableList = dr.TableList();
                    //var tables = dr.AllTables();
                    //var views = dr.AllViews();
                    Assert.NotEmpty(tableList);
                }
                catch (System.Net.Sockets.SocketException)
                {
                    Console.WriteLine("PostgreSql not installed on this machine");
                }
            }
        }
    }
}