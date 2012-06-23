namespace DatabaseSchemaReader.CodeGen
{
    /// <summary>
    /// Code generation settings. Customize the defaults.
    /// </summary>
    public class CodeWriterSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeWriterSettings"/> class.
        /// </summary>
        public CodeWriterSettings()
        {
            Namespace = "Domain";
            CodeTarget = CodeTarget.Poco;
            CollectionNamer = new CollectionNamer();
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
        /// Gets or sets the collection namer.
        /// </summary>
        /// <value>
        /// The collection namer.
        /// </value>
        public ICollectionNamer CollectionNamer { get; set; }

        internal string NameCollection(string name)
        {
            if (CollectionNamer == null) return name + "Collection";
            return CollectionNamer.NameCollection(name);
        }

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
        /// Gets or sets a value indicating whether to write a csproj project file.
        /// </summary>
        /// <value><c>true</c> if to write project files; otherwise, <c>false</c>.</value>
        public bool WriteProjectFile { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include views.
        /// </summary>
        /// <value><c>true</c> if include views; otherwise, <c>false</c>.</value>
        public bool IncludeViews { get; set; }
    }
}
