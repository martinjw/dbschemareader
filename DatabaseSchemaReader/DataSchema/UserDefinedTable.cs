using System;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// A UDT/User defined table type
    /// </summary>
    [Serializable]
    public class UserDefinedTable : DatabaseTable
    {
        /// <summary>
        /// Indicates this is a collection type (Oracle VARRAY, TABLE)
        /// </summary>
        public bool IsCollectionType { get; set; }

        /// <summary>
        /// Name of collection Type
        /// </summary>
        public string CollectionTypeName { get; set; }

        /// <summary>
        /// Source (if available)
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SourceBody { get; set; }
    }
}
