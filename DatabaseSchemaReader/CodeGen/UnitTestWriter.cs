using System;
using System.Collections.Generic;
using System.Linq;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.CodeGen
{
    class UnitTestWriter
    {
        /*
         * It is possible to generate CRUD operations for a simple smoke test
         * Here we just take the first entity with no dependencies.
         */

        private readonly DatabaseSchema _schema;
        private readonly string _namespace;
        private readonly ClassBuilder _cb;

        public UnitTestWriter(DatabaseSchema schema, string ns)
        {
            _namespace = ns;
            _schema = schema;
            _cb = new ClassBuilder();
        }

        public string ClassName { get; private set; }

        public string Write()
        {
            //find first table with no dependencies (foreign keys) and a .Net name.
            var entity = _schema.Tables
                .FirstOrDefault(t => t.ForeignKeys.Count == 0 && !string.IsNullOrEmpty(t.NetName));
            if (entity == null) return null;

            //we'll also run a sproc if we find one
            var sproc = _schema.StoredProcedures.FirstOrDefault(p => p.ResultSets.Count > 0);

            ClassName = entity.NetName + "Test";

            WriteNamespaces(sproc != null);

            using (_cb.BeginNest("namespace " + _namespace + ".Tests"))
            {
                _cb.AppendLine("[TestClass]");
                using (_cb.BeginNest("public class " + ClassName))
                {
                    WriteStaticConstructor(entity);
                    WriteOpenSession();
                    WriteGenerateString();
                    WriteCreateEntity(entity);
                    WriteCrudTest(entity);
                    WriteSproc(sproc);
                }
            }

            return _cb.ToString();
        }

        private void WriteSproc(DatabaseStoredProcedure sproc)
        {
            if (sproc == null) return;
            using (_cb.BeginNest("private static DbConnection CreateConnection()"))
            {
                _cb.AppendLine(@"const string connectionString = @""" + _schema.ConnectionString + "\";");
                _cb.AppendLine(@"const string provider = @""" + _schema.Provider + "\";");
                _cb.AppendLine("var factory = DbProviderFactories.GetFactory(provider);");
                _cb.AppendLine("var connection = factory.CreateConnection();");
                _cb.AppendLine("connection.ConnectionString = connectionString;");
                _cb.AppendLine("return connection;");
            }

            _cb.AppendLine("[TestMethod]");
            using (_cb.BeginNest("public void Test" + sproc.NetName + "()"))
            {
                using (_cb.BeginNest("using (var connection = CreateConnection())"))
                {
                    _cb.AppendLine("var cmd = new " + sproc.NetName + "(connection);");
                    var list = new List<string>();
                    foreach (var arg in sproc.Arguments)
                    {
                        if (arg.DataType == null)
                        {
                            list.Add("null");
                        }
                        else if (arg.DataType.IsString)
                        {
                            list.Add("a");
                        }
                        else if (arg.DataType.IsNumeric)
                        {
                            list.Add("1");
                        }
                        else if (arg.DataType.GetNetType() == typeof(DateTime))
                        {
                            list.Add("DateTime.Now");
                        }
                    }
                    _cb.AppendLine("var result = cmd.Execute(" + string.Join(", ", list.ToArray()) + ");");
                    _cb.AppendLine("//test whether it worked!");
                }
            }
        }

        private void WriteCrudTest(DatabaseTable entity)
        {
            _cb.AppendLine("[TestMethod]");
            using (_cb.BeginNest("public void Test" + entity.NetName + "()"))
            {
                _cb.AppendLine("var entity = Create" + entity.NetName + "();");
                using (_cb.BeginNest("using (new TransactionScope()) //not committed, so rolls back"))
                {
                    using (_cb.BeginNest("using (ISession session = OpenSession())"))
                    {
                        _cb.AppendLine("session.SaveOrUpdate(entity);");
                        _cb.AppendLine("session.Delete(entity);");
                    }
                }
            }
        }

        private void WriteCreateEntity(DatabaseTable entity)
        {
            using (_cb.BeginNest("public " + entity.NetName + " Create" + entity.NetName + "()"))
            {
                _cb.AppendLine("var entity = new " + entity.NetName + "();");
                foreach (var column in entity.Columns)
                {
                    if (column.IsIdentity) continue;
                    if (column.DataType == null) continue;
                    var name = column.NetName;
                    //we can't parse check constraints, so this may go wrong
                    string value = null;
                    if (column.DataType.IsNumeric)
                        value = "0";
                    else if (column.DataType.IsString)
                        value = "GenerateString(" + column.Length + ")";
                    else if (column.DataType.GetNetType() == typeof(DateTime))
                        value = "DateTime.Now";
                    if (value != null)
                        _cb.AppendLine("entity." + name + " = " + value + ";");
                }
                _cb.AppendLine("return entity;");
            }
        }

        private void WriteOpenSession()
        {
            using (_cb.BeginNest("private static ISession OpenSession()"))
            {
                _cb.AppendLine("return SessionFactory.OpenSession();");
            }
        }

        private void WriteGenerateString()
        {
            using (_cb.BeginNest("private static string GenerateString(int length)"))
            {
                _cb.AppendLine("var s = Guid.NewGuid().ToString();");
                _cb.AppendLine("if (length > 8000) return s; //clobs or varchar(max)");
                _cb.AppendLine("if (36 > length) return s.Substring(0, length);");
                _cb.AppendLine("return s + new string('s', length - 36);");
            }
        }

        private void WriteStaticConstructor(DatabaseTable entity)
        {
            _cb.AppendLine("private static readonly ISessionFactory SessionFactory;");

            using (_cb.BeginNest("static " + ClassName + "()"))
            {
                _cb.AppendLine("var configuration = new Configuration();");
                _cb.AppendLine("configuration.Configure(); //configure from the app.config");
                _cb.AppendLine("configuration.AddAssembly(typeof(" + entity.NetName + ").Assembly);");
                _cb.AppendLine("SessionFactory = configuration.BuildSessionFactory();");
            }
        }


        private void WriteNamespaces(bool includeProcedures)
        {
            _cb.AppendLine("using System;");
            _cb.AppendLine("using System.Data.Common;");
            _cb.AppendLine("using System.Transactions;");
            _cb.AppendLine("using Microsoft.VisualStudio.TestTools.UnitTesting;");
            _cb.AppendLine("using NHibernate;");
            _cb.AppendLine("using NHibernate.Cfg;");
            _cb.AppendLine("using " + _namespace + ";");
            if (includeProcedures)
                _cb.AppendLine("using " + _namespace + ".Procedures;");
        }
    }
}
