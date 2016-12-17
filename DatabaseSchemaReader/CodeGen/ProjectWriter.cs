using System;
using System.Linq;
//Reflection needed for GetTypeInfo()
// ReSharper disable once RedundantUsingDirective
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace DatabaseSchemaReader.CodeGen
{
    class ProjectWriter
    {
        private readonly XNamespace _xmlns = "http://schemas.microsoft.com/developer/msbuild/2003";
        private readonly XDocument _document;
        private readonly XContainer _itemGroup;
        private bool _hasOracle;
        private ProjectVersion _projectVersion;

        public ProjectWriter(string @namespace, ProjectVersion projectVersion)
        {
            _document = LoadProjectXml();
            //give it a unique guid
            var guid = "{" + Guid.NewGuid().ToString("D") + "}";
            var projectGuid = _document.Descendants(_xmlns + "ProjectGuid").First();
            projectGuid.SetValue(guid);

            var rootNamespace = _document.Descendants(_xmlns + "RootNamespace").First();
            rootNamespace.SetValue(@namespace ?? "Project");

            var assemblyName = _document.Descendants(_xmlns + "AssemblyName").First();
            assemblyName.SetValue(@namespace ?? "Project");

            _itemGroup = _document.Descendants(_xmlns + "ItemGroup").Last();
            Upgrade(projectVersion);
        }

        public void AddClass(string classFile)
        {
            var compile = new XElement(_xmlns + "Compile",
                new XAttribute("Include", classFile));
            _itemGroup.Add(compile);
        }

        public void AddMap(string fileName)
        {
            var embeddedResource = new XElement(_xmlns + "EmbeddedResource",
                new XAttribute("Include", fileName));
            _itemGroup.Add(embeddedResource);
        }

        private XElement FindSystemDataReference()
        {
            return _document
                .Descendants(_xmlns + "Reference")
                .First(r => (string)r.Attribute("Include") == "System.Data");
        }

        public void AddOracleReference()
        {
            if (_hasOracle) return;
            var reference = FindSystemDataReference();
            reference.AddAfterSelf(
                new XElement(_xmlns + "Reference",
                            new XAttribute("Include", "System.Data.OracleClient")));
            _hasOracle = true;
        }

        public void AddOracleManagedReference()
        {
            if (_hasOracle) return;
            var reference = FindSystemDataReference();
            reference.AddAfterSelf(
                new XElement(_xmlns + "Reference",
                            new XAttribute("Include", "Oracle.ManagedDataAccess, Version=4.121.2.0, Culture=neutral, PublicKeyToken=89b483f429c47342, processorArchitecture=MSIL")),
                new XElement(_xmlns + "Private", "True"),
                new XElement(_xmlns + "HintPath", @"..\packages\Oracle.ManagedDataAccess.12.1.24160719\lib\net40\Oracle.ManagedDataAccess.dll"));
            reference.AddAfterSelf(
                new XElement(_xmlns + "Reference",
                            new XAttribute("Include", "Oracle.ManagedDataAccess.EntityFramework, Version=6.121.2.0, Culture=neutral, PublicKeyToken=89b483f429c47342, processorArchitecture=MSIL")),
                new XElement(_xmlns + "Private", "True"),
                new XElement(_xmlns + "HintPath", @"..\Oracle.ManagedDataAccess.EntityFramework.12.1.2400\lib\net45\Oracle.ManagedDataAccess.EntityFramework.dll"));
            _hasOracle = true;
        }

        public void AddDevartOracleReference()
        {
            if (_hasOracle) return;
            var reference = FindSystemDataReference();
            reference.AddAfterSelf(
                new XElement(_xmlns + "Reference",
                            new XAttribute("Include",
                                "Devart.Data.Oracle.Entity.EF6, Version=9.2.162.0, Culture=neutral, PublicKeyToken=09af7300eec23701, processorArchitecture=MSIL"),
                new XElement(_xmlns + "Private", "True")));
            reference.AddAfterSelf(
                new XElement(_xmlns + "Reference",
                            new XAttribute("Include",
                                "Devart.Data.Oracle, Version=9.2.162.0, Culture=neutral, PublicKeyToken=09af7300eec23701, processorArchitecture=MSIL"),
                new XElement(_xmlns + "Private", "True")));
            reference.AddAfterSelf(
                new XElement(_xmlns + "Reference",
                            new XAttribute("Include",
                                "Devart.Data, Version=5.0.1586.0, Culture=neutral, PublicKeyToken=09af7300eec23701, processorArchitecture=MSIL"),
                new XElement(_xmlns + "Private", "True")));
            _hasOracle = true;
        }

        public void AddNHibernateReference()
        {
            //the hintpaths are for a hypothetical nuget folder

            var reference = FindSystemDataReference();
            var element = new XElement(_xmlns + "Reference", new XAttribute("Include", "FluentNHibernate, Version=2.0.3.0, Culture=neutral, processorArchitecture=MSIL"),
                new XElement(_xmlns + "Private", "True"),
                new XElement(_xmlns + "HintPath", (_projectVersion != ProjectVersion.Vs2008 ?
                    @"..\packages\FluentNHibernate.2.0.3.0\lib\net40\FluentNHibernate.dll" :
                    @"..\packages\FluentNHibernate.1.4.0.0\lib\net35\FluentNHibernate.dll")));

            reference.AddAfterSelf(element);
            element = new XElement(_xmlns + "Reference", new XAttribute("Include", "NHibernate, Version=4.0.0.4000, Culture=neutral, PublicKeyToken=aa95f207798dfdb4, processorArchitecture=MSIL"),
                new XElement(_xmlns + "Private", "True"),
                new XElement(_xmlns + "HintPath", (_projectVersion != ProjectVersion.Vs2008 ?
                    @"..\packages\NHibernate.4.0.4.4000\lib\Net40\NHibernate.dll" :
                    @"..\packages\NHibernate.3.4.0.4000\lib\Net35\NHibernate.dll")));
            reference.AddAfterSelf(element);

            element = new XElement(_xmlns + "Reference", new XAttribute("Include", "Iesi.Collections, Version=4.0.0.0, Culture=neutral, PublicKeyToken=aa95f207798dfdb4, processorArchitecture=MSIL"),
                new XElement(_xmlns + "Private", "True"),
                new XElement(_xmlns + "HintPath", (_projectVersion != ProjectVersion.Vs2008 ?
                    @"..\packages\Iesi.Collections.4.0.1.4000\lib\Net40\Iesi.Collections.dll" :
                    @"..\packages\Iesi.Collections.3.2.0.4000\lib\Net35\Iesi.Collections.dll")));
            reference.AddAfterSelf(element);
        }

        public void AddEntityFrameworkReference()
        {
            if (_projectVersion == ProjectVersion.Vs2008) Upgrade(ProjectVersion.Vs2015);
            //use the HintPath of the Nuget package
            var reference = FindSystemDataReference();
            var element = new XElement(_xmlns + "Reference", new XAttribute("Include", "EntityFramework, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089, processorArchitecture=MSIL"),
                new XElement(_xmlns + "Private", "True"),
                new XElement(_xmlns + "HintPath", (_projectVersion == ProjectVersion.Vs2010 ?
                @"..\packages\EntityFramework.6.1.3\lib\net40\EntityFramework.dll" :
                @"..\packages\EntityFramework.6.1.3\lib\net45\EntityFramework.dll")));
            reference.AddAfterSelf(element);
            var efSqlServer = new XElement(_xmlns + "Reference",
                new XAttribute("Include",
                    "EntityFramework.SqlServer, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089, processorArchitecture=MSIL"),
                new XElement(_xmlns + "Private", "True"),
                new XElement(_xmlns + "HintPath", (_projectVersion == ProjectVersion.Vs2010
                    ? @"..\packages\EntityFramework.6.1.3\lib\net40\EntityFramework.SqlServer.dll"
                    : @"..\packages\EntityFramework.6.1.3\lib\net45\EntityFramework.SqlServer.dll")));
            reference.AddAfterSelf(efSqlServer);
        }

        public void AddPackagesConfig()
        {
            var compile = new XElement(_xmlns + "None",
                new XAttribute("Include", "packages.config"));
            _itemGroup.Add(compile);
        }

        private void Upgrade(ProjectVersion projectVersion)
        {
            if (projectVersion == ProjectVersion.Vs2008) return;
            var projectElement = _document.Root;
            if (projectElement == null) return;
            projectElement.SetAttributeValue("ToolsVersion", projectVersion == ProjectVersion.Vs2015 ? "14.0" : "4.0");
            var target = projectElement.Descendants(_xmlns + "TargetFrameworkVersion").First();
            target.SetValue(projectVersion == ProjectVersion.Vs2015 ? "v4.6.1" : "v4.0");
            var systemCore = _document
                .Descendants(_xmlns + "Reference")
                .FirstOrDefault(r => (string)r.Attribute("Include") == "System.Core");
            if (systemCore != null)
            {
                if (projectVersion == ProjectVersion.Vs2010)
                {
                    systemCore.Descendants(_xmlns + "RequiredTargetFramework").First().Remove();
                }
                if (projectVersion == ProjectVersion.Vs2015)
                {
                    systemCore.Remove();
                }
            }

            _projectVersion = projectVersion;
        }

        public string Write()
        {
            return _document.ToString();
        }

        private static XDocument LoadProjectXml()
        {
            const string streamPath = "DatabaseSchemaReader.CodeGen.Project.xml";
#if !COREFX
            var executingAssembly = typeof(ProjectWriter).Assembly;
#else
            var executingAssembly = typeof(ProjectWriter).GetTypeInfo().Assembly;
#endif
            var stream = executingAssembly.GetManifestResourceStream(streamPath);
            if (stream == null) return null;
            return XDocument.Load(XmlReader.Create(stream));
        }

    }
}
