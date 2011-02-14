using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DatabaseSchemaReader.SqlGen
{
    interface ISqlFormatProvider
    {
        string Escape(string name);
        string LineEnding();
    }
}
