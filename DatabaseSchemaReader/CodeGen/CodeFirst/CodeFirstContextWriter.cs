using System.Collections.Generic;
using System.Linq;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.CodeGen.CodeFirst
{
    class CodeFirstContextWriter
    {
        private readonly CodeWriterSettings _codeWriterSettings;
        private readonly ClassBuilder _cb;
        private readonly string _contextName;

        public CodeFirstContextWriter(CodeWriterSettings codeWriterSettings)
        {
            _codeWriterSettings = codeWriterSettings;
            _cb = new ClassBuilder();
            _contextName = CreateContextName();
        }

        private string CreateContextName()
        {
            var name = "Domain";
            var ns = _codeWriterSettings.Namespace;
            if (!string.IsNullOrEmpty(ns))
            {
                var lastIndex = ns.LastIndexOf('.');
                name = lastIndex == -1 ? ns : ns.Substring(lastIndex + 1);
            }
            return name + "Context";
        }

        public string ContextName
        {
            get { return _contextName; }
        }

        public string Write(ICollection<DatabaseTable> tables)
        {
            _cb.AppendLine("using System;");
            _cb.AppendLine("using System.Data.Common;"); //DbConnection
            _cb.AppendLine("using System.Data.Entity;");
            _cb.AppendLine("using System.Data.Entity.Infrastructure;"); //IncludeMetadataConvention
            _cb.AppendLine("using " + _codeWriterSettings.Namespace + ".Mapping;");

            using (_cb.BeginNest("namespace " + _codeWriterSettings.Namespace))
            {
                using (_cb.BeginNest("public class " + ContextName + " : DbContext"))
                {
                    //ctors (esp string connectionName and DbConnection)
                    using (_cb.BeginNest("public " + ContextName + "()", "Constructor"))
                    {
                        _cb.AppendLine("//default ctor uses app.config connection named " + ContextName);
                    }
                    using (_cb.BeginNest("public " + ContextName + "(DbConnection connection) : base(connection,true)", "Constructor"))
                    {
                        _cb.AppendLine("//ctor for tracing");
                    }


                    var dbSetTables = tables
                        .Where(x => !x.IsManyToManyTable())
                        //doesn't support tables without a primary key
                        .Where(x => x.PrimaryKey != null)
                        .ToArray();

                    foreach (var table in dbSetTables)
                    {
                        var className = table.NetName;
                        var dbSetName = _codeWriterSettings.NameCollection(className);

                        //we won't pluralize, let's just suffix it "Set"
                        using (_cb.BeginNest("public IDbSet<" + className + "> " + dbSetName))
                        {
                            _cb.AppendLine("get { return Set<" + className + ">(); }");
                        }
                    }


                    using (_cb.BeginNest("protected override void OnModelCreating(DbModelBuilder modelBuilder)"))
                    {
                        _cb.AppendLine("Database.SetInitializer<" + ContextName + ">(null);");
                        _cb.AppendLine("//modelBuilder.Conventions.Remove<IncludeMetadataConvention>(); //EF 4.1-4.2 only, obsolete in EF 4.3");
                        foreach (var table in dbSetTables)
                        {
                            var className = table.NetName;

                            _cb.AppendLine("modelBuilder.Configurations.Add(new " + className + "Mapping());");
                        }

                    }
                }
            }
            return _cb.ToString();
        }
    }
}
