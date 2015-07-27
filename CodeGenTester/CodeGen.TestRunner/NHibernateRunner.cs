using System;
using System.Linq;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Criterion;
using NHibernate.Linq;
using Northwind.NHib;

namespace CodeGen.TestRunner
{
    static class NHibernateRunner
    {
        public static bool Execute()
        {
            //initialize nhibernate
            var factory = Configure();

            //use a session
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

        private static ISessionFactory Configure()
        {
            var configuration = new Configuration();
            configuration.Configure(); //configure from the app.config
            configuration.AddAssembly(typeof(Category).Assembly);
            return configuration.BuildSessionFactory();
        }
    }
}
