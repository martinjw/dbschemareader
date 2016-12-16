using System;

namespace DatabaseSchemaReader.CodeGen
{
    /// <summary>
    /// Code generation settings. Customize the defaults.
    /// </summary>
    public class CodeWriterSettings
    {
        private INamer _namer;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeWriterSettings"/> class.
        /// </summary>
        public CodeWriterSettings()
        {
            Namespace = "Domain";
            CodeTarget = CodeTarget.Poco;
            var namer = new Namer();
            Namer = namer;
            CodeInserter = new CodeInserter();
        }

        /// <summary>
        /// Gets or sets the namespace.
        /// </summary>
        /// <value>The namespace.</value>
        public string Namespace { get; set; }

        /// <summary>
        /// Gets or sets the code target.
        /// </summary>
        /// <value>The code target.</value>
        public CodeTarget CodeTarget { get; set; }

        /// <summary>
        /// Gets or sets the namer, which translates table and column names to classes and properties.
        /// </summary>
        /// <value>
        /// The namer.
        /// </value>
        public INamer Namer
        {
            get { return _namer; }
            set
            {
                if (value == null) throw new ArgumentNullException();
                _namer = value;
            }
        }

        /// <summary>
        /// Gets or sets the code inserter.
        /// </summary>
        /// <value>
        /// The code inserter.
        /// </value>
        public CodeInserter CodeInserter { get; set; }

        /// <summary>
        /// Gets or sets the error message for the [Required] attribute. If null, none is written.
        /// </summary>
        /// <value>
        /// The Required error message.
        /// </value>
        /// <remarks>
        /// Format token 0 is replaced with a formatted version of the property name.
        /// </remarks>
        public string RequiredErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the error message for the [StringLength] attribute. If null, none is written
        /// </summary>
        /// <value>
        /// The StringLength error message.
        /// </value>
        /// <remarks>
        /// Format token 0 is replaced with the maximum length.
        /// Format token 1 is replaced with a formatted version of the property name.
        /// </remarks>
        public string StringLengthErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the error message for the [Range] attribute. If null, none is written
        /// </summary>
        /// <value>
        /// The Range error message.
        /// </value>
        /// <remarks>
        /// Format token 0 is replaced with the maximum value.
        /// Format token 1 is replaced with a formatted version of the property name.
        /// </remarks>
        public string RangeErrorMessage { get; set; }

        /// <summary>
        /// Indicates whether to write two properties for a foreign key: the instance and a shadow scalar id property. Default is <c>false</c> (just write the instance property)
        /// </summary>
        /// <remarks>
        /// Foreign key id properties are convenient in EF Code First. By using them, you can avoid extra database access.
        /// </remarks>
        /// <value>
        /// 	<c>true</c> if use foreign key properties; otherwise, <c>false</c>.
        /// </value>
        public bool UseForeignKeyIdProperties { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to write stored procedures.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if write stored procedures; otherwise, <c>false</c>.
        /// </value>
        public bool WriteStoredProcedures { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to write a sample unit test
        /// </summary>
        /// <value><c>true</c> if writes a unit test; otherwise, <c>false</c>.</value>
        public bool WriteUnitTest { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to write a csproj project file for .Net 4.0 (VS 2010/2012)
        /// </summary>
        /// <value><c>true</c> if to write project file; otherwise, <c>false</c>.</value>
        public bool WriteProjectFile { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to write a csproj project file for .Net 3.5 (VS 2008)
        /// </summary>
        /// <value><c>true</c> if to write project file; otherwise, <c>false</c>.</value>
        public bool WriteProjectFileNet35 { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to write a csproj project file for .Net 4.6 (VS 2015)
        /// </summary>
        /// <value><c>true</c> if to write project file; otherwise, <c>false</c>.</value>
        public bool WriteProjectFileNet46 { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include views.
        /// </summary>
        /// <value><c>true</c> if include views; otherwise, <c>false</c>.</value>
        public bool IncludeViews { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether properties may include the Code First index attribute.
        /// </summary>
        /// <value>
        /// <c>true</c> if properties are written with the index attribute; otherwise, <c>false</c>.
        /// </value>
        public bool WriteCodeFirstIndexAttribute { get; set; }
    }
}
