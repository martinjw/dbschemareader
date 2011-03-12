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

        public void AddOracleReference()
        {
            if (_hasOracle) return;
            var reference = _document
                .Descendants(_xmlns + "Reference")
                .Where(r => (string)r.Attribute("Include") == "System.Data")
                .First();
            reference.AddAfterSelf(
                new XElement(_xmlns + "Reference",
                            new XAttribute("Include", "System.Data.OracleClient")));
            _hasOracle = true;
        }

        public string Write()
        {
            return _document.ToString();
        }

        private static XDocument LoadProjectXml()
        {
            const string streamPath = "DatabaseSchemaReader.CodeGen.Project.xml";
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(streamPath);
            if (stream == null) return null;
            return XDocument.Load(XmlReader.Create(stream));
        }

    }
}
