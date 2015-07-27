using System;
using System.Linq;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Criterion;
using NHibernate.Linq;
using Northwind.FluentNHib;
using Northwind.FluentNHib.Mapping;

namespace CodeGen.TestRunner
{
    static class FluentNHibernateRunner
    {

        public static bool Execute()
        {
            var factory = FluentlyConfigure();
            using (var session = factory.OpenSession())
            {
                var cat1 = session.Get<Category>(1);
                Console.WriteLine(cat1.CategoryName);

                var activeProductsByLinq =
                    session.Query<Product>()
                           .Where(x => x.Discontinued == false);

                var activeProductsByCriteria =
                    session.CreateCriteria<Product>()
                           .Add(Restrictions.Eq("Discontinued", false))
                           .List<Product>();

                Console.WriteLine(activeProductsByLinq.Count());
                Console.WriteLine(activeProductsByCriteria.Count);
            }
            return true;
        }


        private static ISessionFactory FluentlyConfigure()
        {
            return Fluently.Configure()
                //which database
                .Database(
                    MsSqlConfiguration.MsSql2008
                        .ConnectionString(cs => cs.FromConnectionStringWithKey("CodeFirstContext"))
                        //.ShowSql()
                        )
                //2nd level cache
                //.Cache(
                //    c => c.UseQueryCache()
                //        .UseSecondLevelCache()
                //        .ProviderClass<NHibernate.Caches.SysCache.SysCacheProvider>())
                //find the mappings
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<CategoryMapping>())
                .BuildSessionFactory();
        }
    }
}

