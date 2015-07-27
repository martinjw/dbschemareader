using System.Linq;
using Northwind.CodeFirst;

namespace CodeGen.TestRunner
{
    static class CodeFirstRunner
    {
        public static bool Execute()
        {
            using (var contxt = new CodeFirstContext())
            {
                var categories = contxt.CategoryCollection.ToList();
                return categories.Any();
            }

        }
    }
}
