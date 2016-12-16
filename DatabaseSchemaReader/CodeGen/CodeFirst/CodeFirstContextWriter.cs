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

        /// <summary>
        /// A flag if this is Oracle (Devart)
        /// </summary>
        public bool IsOracle { get; set; }

        public string Write(ICollection<DatabaseTable> tables)
        {
            _cb.AppendLine("using System;");
            if (_codeWriterSettings.CodeTarget == CodeTarget.PocoEfCore)
            {
                _cb.AppendLine("using Microsoft.EntityFrameworkCore;");
            }
            else
            {
                _cb.AppendLine("using System.Data.Common;"); //DbConnection
                _cb.AppendLine("using System.Data.Entity;");
                _cb.AppendLine("using System.Data.Entity.Infrastructure;"); //IncludeMetadataConvention
            }
            _cb.AppendLine("using " + _codeWriterSettings.Namespace + ".Mapping;");

            using (_cb.BeginNest("namespace " + _codeWriterSettings.Namespace))
            {
                using (_cb.BeginNest("public class " + ContextName + " : DbContext"))
                {
                    WriteConstructors();

                    var dbSetTables = tables;
                    if (_codeWriterSettings.CodeTarget == CodeTarget.PocoEntityCodeFirst)
                        dbSetTables = tables.Where(x => !x.IsManyToManyTable()).ToList();
                    dbSetTables = dbSetTables
                        //doesn't support tables without a primary key
                        .Where(x => x.PrimaryKey != null ||
                            //unless it's a view
                            (_codeWriterSettings.IncludeViews && x is DatabaseView))
                        .ToList();

                    WriteDbSets(dbSetTables);


                    WriteOnModelCreating(dbSetTables);
                }
            }
            return _cb.ToString();
        }

        private void WriteConstructors()
        {
            //ctors (esp string connectionName and DbConnection)
            using (_cb.BeginNest("public " + ContextName + "()", "Constructor"))
            {
                _cb.AppendLine("//default ctor uses app.config connection named " + ContextName);
            }
            if (_codeWriterSettings.CodeTarget == CodeTarget.PocoEfCore)
            {
                using (
                    _cb.BeginNest("public " + ContextName + "(DbContextOptions options) : base(options)",
                        "Constructor with options"))
                {
                    _cb.AppendLine("//ctor for integration testing");
                }
            }
            else
            {
                using (
                    _cb.BeginNest("public " + ContextName + "(DbConnection connection) : base(connection,true)",
                        "Constructor"))
                {
                    _cb.AppendLine("//ctor for tracing");
                }
            }
        }

        private void WriteDbSets(IEnumerable<DatabaseTable> dbSetTables)
        {
            var dbSet = _codeWriterSettings.CodeTarget == CodeTarget.PocoEfCore ?
                "public DbSet<" : "public IDbSet<";

            foreach (var table in dbSetTables)
            {
                var className = table.NetName;
                var dbSetName = _codeWriterSettings.Namer.NameCollection(className);

                //we won't pluralize, let's just suffix it "Set"
                using (_cb.BeginNest(dbSet + className + "> " + dbSetName))
                {
                    _cb.AppendLine("get { return Set<" + className + ">(); }");
                }
            }
        }

        private void WriteOnModelCreating(IEnumerable<DatabaseTable> dbSetTables)
        {
            var isCore = _codeWriterSettings.CodeTarget == CodeTarget.PocoEfCore;
            var onModelCreating = isCore
                ? "protected override void OnModelCreating(ModelBuilder modelBuilder)"
                : "protected override void OnModelCreating(DbModelBuilder modelBuilder)";
            using (_cb.BeginNest(onModelCreating))
            {
                if (!isCore)
                {
                    _cb.AppendLine("Database.SetInitializer<" + ContextName + ">(null);");
                }
                //_cb.AppendLine(
                //    "//modelBuilder.Conventions.Remove<IncludeMetadataConvention>(); //EF 4.1-4.2 only, obsolete in EF 4.3");
                if (IsOracle)
                {
                    //we comment them out for now
                    _cb.AppendLine("// Oracle devart configuration http://www.devart.com/dotconnect/oracle/docs/?EFProviderConfiguration.html");
                    _cb.AppendLine("//var config = Devart.Data.Oracle.Entity.Configuration.OracleEntityProviderConfig.Instance;");
                    _cb.AppendLine("//config.Workarounds.IgnoreSchemaName = true;");
                    // This option must be True for EF Code-First Migrations (EF v4.3.x and v5.x) to work correctly.
                    _cb.AppendLine("//config.Workarounds.ColumnTypeCasingConventionCompatibility = true;");
                    // if it is set to false, you must turn off ColumnTypeCasingConvention explicitly for each DbContext ie this...
                    //_cb.AppendLine("modelBuilder.Conventions.Remove<System.Data.Entity.ModelConfiguration.Conventions.ColumnTypeCasingConvention>();");
                }

                foreach (var table in dbSetTables)
                {
                    var className = table.NetName;

                    if (isCore)
                    {
                        _cb.AppendLine("modelBuilder.Entity<" + className + ">(" + className + "Mapping.Map);");
                    }
                    else
                    {
                        _cb.AppendLine("modelBuilder.Configurations.Add(new " + className + "Mapping());");
                    }
                }
            }
        }
    }
}
