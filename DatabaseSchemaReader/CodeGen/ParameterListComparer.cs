using System.Collections.Generic;
using System.Linq;


// TODO: pluralize the collection properties and With methods that use them
// TODO: figure out how to properly overload the getlist methods for cao by parent and caoid
// TODO: figure out how to handle nullable parameters for the wither's

namespace DatabaseSchemaReader.CodeGen
{
    public class ParameterListComparer : IEqualityComparer<List<Parameter>>
    {
        public bool Equals(List<Parameter> x, List<Parameter> y)
        {
            return x.SequenceEqual(y);
        }

        public int GetHashCode(List<Parameter> obj)
        {
            if (obj == null) return 0;
            unchecked
            {
                int hash = 19;
                foreach (var w in obj)
                {
                    hash = hash * 31 + w.GetHashCode();
                }
                return hash;
            }
        }
    }
}
