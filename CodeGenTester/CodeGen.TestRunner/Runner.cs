using System;

namespace CodeGen.TestRunner
{
    /// <summary>
    /// Executes all the code
    /// </summary>
    public class Runner
    {
        /// <summary>
        /// Runs this instance.
        /// </summary>
        /// <returns></returns>
        public bool Run()
        {
            var ok = true;
            if (!CodeFirstRunner.Execute())
            {
                Console.WriteLine("Code First does not run");
                ok = false;
            }
            if (!NHibernateRunner.Execute())
            {
                Console.WriteLine("NHibernate does not run");
                ok = false;
            }
            if (!FluentNHibernateRunner.Execute())
            {
                Console.WriteLine("Fluent NHibernate does not run");
                ok = false;
            }
            return ok;
        }
    }
}
