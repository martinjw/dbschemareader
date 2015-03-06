namespace DatabaseSchemaReader.CodeGen
{
    static class PackagesWriter
    {
        public static string WriteEntityFrameworkNet4()
        {
            return @"<?xml version=""1.0"" encoding=""utf-8""?>
<packages>
  <package id=""EntityFramework"" version=""6.1.2"" targetFramework=""net40"" />
</packages>";
        }

        public static string WriteFluentNHibernateNet35()
        {
            return @"<?xml version=""1.0"" encoding=""utf-8""?>
<packages>
  <package id=""FluentNHibernate"" version=""1.4.0.0"" targetFramework=""net35"" />
  <package id=""Iesi.Collections"" version=""3.2.0.4000"" targetFramework=""net35"" />
  <package id=""NHibernate"" version=""3.3.3.4000"" targetFramework=""net35"" />
</packages>";
        }

        public static string WriteFluentNHibernateNet4()
        {
            return @"<?xml version=""1.0"" encoding=""utf-8""?>
<packages>
  <package id=""FluentNHibernate"" version=""2.0.1.0"" targetFramework=""net40"" />
  <package id=""Iesi.Collections"" version=""4.0.0.4000"" targetFramework=""net40"" />
  <package id=""NHibernate"" version=""4.0.3.4000"" targetFramework=""net40"" />
</packages>";
        }
    }
}
