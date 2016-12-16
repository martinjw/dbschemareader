namespace DatabaseSchemaReader.CodeGen
{
    class PackagesWriter
    {
        private readonly ProjectVersion _projectVersion;
        private string _providerReference;

        public PackagesWriter(ProjectVersion projectVersion)
        {
            _projectVersion = projectVersion;
        }

        public string WriteEntityFramework()
        {
            var targetFramework = _projectVersion == ProjectVersion.Vs2010 ? "net40" : "net461";
            return @"<?xml version=""1.0"" encoding=""utf-8""?>
<packages>
  <package id=""EntityFramework"" version=""6.1.3"" targetFramework=""" + targetFramework + @""" />
" + _providerReference + @"
</packages>";
        }

        public string WriteFluentNHibernate()
        {
            if (_projectVersion == ProjectVersion.Vs2008)
            {
                            return @"<?xml version=""1.0"" encoding=""utf-8""?>
<packages>
  <package id=""FluentNHibernate"" version=""1.4.0.0"" targetFramework=""net35"" />
  <package id=""Iesi.Collections"" version=""3.2.0.4000"" targetFramework=""net35"" />
  <package id=""NHibernate"" version=""3.3.3.4000"" targetFramework=""net35"" />
</packages>";
            }
            var targetFramework = _projectVersion == ProjectVersion.Vs2010 ? "net40" : "net461";
            return @"<?xml version=""1.0"" encoding=""utf-8""?>
<packages>
  <package id=""FluentNHibernate"" version=""2.0.3.0"" targetFramework=""" + targetFramework + @""" />
  <package id=""Iesi.Collections"" version=""4.0.1.4000"" targetFramework=""" + targetFramework + @""" />
  <package id=""NHibernate"" version=""4.0.4.4000"" targetFramework=""" + targetFramework + @""" />
" + _providerReference + @"
</packages>";
        }

        public void AddOracleManagedClient()
        {
            var targetFramework = _projectVersion == ProjectVersion.Vs2010 ? "net40" : "net461";
            _providerReference = @"
  <package id=""Oracle.ManagedDataAccess"" version=""12.1.24160719"" targetFramework=""" + targetFramework + @""" />
  <package id=""Oracle.ManagedDataAccess.EntityFramework"" version=""12.1.2400"" targetFramework=""" + targetFramework + @""" />";
        }
    }
}
