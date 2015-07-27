using System;

namespace DatabaseSchemaReader.SqlGen.SqlServer
{
    class SqlFormatProvider : ISqlFormatProvider
    {
        public string Escape(string name)
        {
            return "[" + name + "]";
        }

        public virtual string LineEnding()
        {
            return ";";
        }


        public string RunStatements()
        {
            return @"
GO
"; ;
        }

        public int MaximumNameLength
        {
            get { return 128; }
        }
    }
}
