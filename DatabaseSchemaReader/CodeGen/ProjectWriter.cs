using System;
using System.Linq;
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
        private bool _is2010;

        public ProjectWriter(string @namespace)
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

        public void AddDevartOracleReference()
        {
            if (_hasOracle) return;
            var reference = FindSystemDataReference();
            reference.AddAfterSelf(
                new XElement(_xmlns + "Reference",
                            new XAttribute("Include",
                                "Devart.Data.Oracle.Entity, Version=8.4.359.6, Culture=neutral, PublicKeyToken=09af7300eec23701, processorArchitecture=MSIL")));
            reference.AddAfterSelf(
                new XElement(_xmlns + "Reference",
                            new XAttribute("Include",
                                "Devart.Data.Oracle, Version=8.4.359.0, Culture=neutral, PublicKeyToken=09af7300eec23701, processorArchitecture=MSIL")));
            reference.AddAfterSelf(
                new XElement(_xmlns + "Reference",
                            new XAttribute("Include",
                                "Devart.Data, Version=5.0.1151.0, Culture=neutral, PublicKeyToken=09af7300eec23701, processorArchitecture=MSIL")));
            _hasOracle = true;
        }

        public void AddNHibernateReference()
        {
            //the hintpaths are for a hypothetical nuget folder

            var reference = FindSystemDataReference();
            var element = new XElement(_xmlns + "Reference", new XAttribute("Include", "FluentNHibernate"),
                new XElement(_xmlns + "SpecificVersion", "false"),
                new XElement(_xmlns + "HintPath", (_is2010 ?
                    @"..\packages\FluentNHibernate.2.0.1.0\lib\net40\FluentNHibernate.dll" :
                    @"..\packages\FluentNHibernate.1.4.0.0\lib\net35\FluentNHibernate.dll")));

            reference.AddAfterSelf(element);
            element = new XElement(_xmlns + "Reference", new XAttribute("Include", "NHibernate"),
                new XElement(_xmlns + "SpecificVersion", "false"),
                new XElement(_xmlns + "HintPath", (_is2010 ?
                    @"..\packages\NHibernate.4.0.3.4000\lib\Net40\NHibernate.dll" :
                    @"..\packages\NHibernate.3.4.0.4000\lib\Net35\NHibernate.dll")));
            reference.AddAfterSelf(element);

            element = new XElement(_xmlns + "Reference", new XAttribute("Include", "Iesi.Collections"),
                new XElement(_xmlns + "SpecificVersion", "false"),
                new XElement(_xmlns + "HintPath", (_is2010 ?
                    @"..\packages\Iesi.Collections.4.0.0.4000\lib\Net40\Iesi.Collections.dll" :
                    @"..\packages\Iesi.Collections.3.2.0.4000\lib\Net35\Iesi.Collections.dll")));
            reference.AddAfterSelf(element);
        }

        public void AddEntityFrameworkReference()
        {
            //use the HintPath of the Nuget package
            var reference = FindSystemDataReference();
            var element = new XElement(_xmlns + "Reference", new XAttribute("Include", "EntityFramework"),
                new XElement(_xmlns + "SpecificVersion", "False"),
                new XElement(_xmlns + "HintPath", @"..\packages\EntityFramework.6.1.3\lib\net40\EntityFramework.dll"));
            reference.AddAfterSelf(element);
            reference.AddAfterSelf(
                new XElement(_xmlns + "Reference",
                            new XAttribute("Include", "System.Data.Entity")));
        }

        public void AddPackagesConfig()
        {
            var compile = new XElement(_xmlns + "None",
                new XAttribute("Include", "packages.config"));
            _itemGroup.Add(compile);
        }

        public void UpgradeTo2010()
        {
            var projectElement = _document.Root;
            if (projectElement == null) return;
            projectElement.SetAttributeValue("ToolsVersion", "4.0");
            var target = projectElement.Descendants(_xmlns + "TargetFrameworkVersion").First();
            target.SetValue("v4.0");
            _is2010 = true;
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
