using System;
using System.Collections.Generic;
using System.Text;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.CodeGen
{
    public interface IWriter
    {
        

        //private readonly DatabaseSchema _schema;
        //private string _mappingPath;
        //private MappingNamer _mappingNamer;
        //private readonly CodeWriterSettings _codeWriterSettings;
        //private readonly ProjectVersion _projectVersion;

        ///// <summary>
        ///// Initializes a new instance of the <see cref="CodeWriter"/> class.
        ///// </summary>
        ///// <param name="schema">The schema.</param>
        //public CodeWriter(DatabaseSchema schema)
        //    : this(schema, new CodeWriterSettings())
        //{
        //}

        ///// <summary>
        ///// Initializes a new instance of the <see cref="CodeWriter"/> class.
        ///// </summary>
        ///// <param name="schema">The schema.</param>
        ///// <param name="codeWriterSettings">The code writer settings.</param>
        //public CodeWriter(DatabaseSchema schema, CodeWriterSettings codeWriterSettings)
        //{
        //    if (schema == null) throw new ArgumentNullException("schema");
        //    if (codeWriterSettings == null) throw new ArgumentNullException("codeWriterSettings");

        //    _schema = schema;
        //    _codeWriterSettings = codeWriterSettings;

        //    var vs2010 = _codeWriterSettings.WriteProjectFile;
        //    var vs2015 = _codeWriterSettings.WriteProjectFileNet46;
        //    _projectVersion = vs2015 ? ProjectVersion.Vs2015 : vs2010 ? ProjectVersion.Vs2010 : ProjectVersion.Vs2008;
        //    //cannot be .net 3.5
        //    if (IsCodeFirst() && _projectVersion == ProjectVersion.Vs2008) _projectVersion = ProjectVersion.Vs2015;

        //    PrepareSchemaNames.Prepare(schema, codeWriterSettings.Namer);
        //}
    }
}
