using System;

namespace CodeGen.TestRunner
{
    class Program
    {
        static void Main()
        {
            var ok = new Runner().Run();

            if (ok)
            {
                Console.WriteLine("All OK");
            }
            else
            {
                Console.WriteLine("FAIL");
                Console.ReadKey();
            }
        }
    }
}
