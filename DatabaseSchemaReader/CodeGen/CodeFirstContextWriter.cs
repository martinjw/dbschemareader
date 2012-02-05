using System.Collections.Generic;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.CodeGen
{
    class CodeFirstContextWriter
    {
        private readonly string _ns;
        private readonly ClassBuilder _cb;
        private readonly string _contextName;

        public CodeFirstContextWriter(string ns)
        {
            _ns = ns;
            _cb = new ClassBuilder();
            _contextName = CreateContextName();
        }

        private string CreateContextName()
        {
            var name = "Domain";
            if (!string.IsNullOrEmpty(_ns))
            {
                var lastIndex = _ns.LastIndexOf('.');
                name = lastIndex == -1 ? _ns : _ns.Substring(lastIndex + 1);
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
            _cb.AppendLine("using System.Data.Entity;");
            _cb.AppendLine("using System.Data.Entity.Infrastructure;");
            _cb.AppendLine("using " + _ns + ".Mapping;");

            using (_cb.BeginNest("namespace " + _ns))
            {
                using (_cb.BeginNest("public class " + ContextName + " : DbContext"))
                {
                    //consider specifying ctors (esp string connectionName and DbConnection)


                    foreach (var table in tables)
                    {
                        if (table.IsManyToManyTable()) continue;

                        var className = table.NetName;
                        //we won't pluralize, let's just suffix it "Set"

                        _cb.AppendLine("public IDbSet<" + className + "> " + className + "Set { get; set; }");
                    }


                    using (_cb.BeginNest("protected override void OnModelCreating(DbModelBuilder modelBuilder)"))
                    {
                        _cb.AppendLine("Database.SetInitializer<" + ContextName + ">(null);");
                        _cb.AppendLine("modelBuilder.Conventions.Remove<IncludeMetadataConvention>();");
                        foreach (var table in tables)
                        {
                            if (table.IsManyToManyTable()) continue;

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
