using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DatabaseSchemaReader.CodeGen;
using Microsoft.CSharp;

namespace DatabaseSchemaReader.DataSchema
{
    public class EnumeratedDataType: DataType
    {
        public List<string> EnumerationValues { get; set; }
        public EnumeratedDataType(string typeName, string netDataType): base(typeName, netDataType) { }
    }
}
