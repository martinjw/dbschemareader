namespace DatabaseSchemaReader.Compare
{
    /// <summary>
    /// 
    /// </summary>
    public enum SchemaObjectType
    {
        /// <summary>
        /// table
        /// </summary>
        Table,
        /// <summary>
        /// view
        /// </summary>
        View,
        /// <summary>
        /// column
        /// </summary>
        Column,
        /// <summary>
        /// constraint
        /// </summary>
        Constraint,
        /// <summary>
        /// index
        /// </summary>
        Index,
        /// <summary>
        /// trigger
        /// </summary>
        Trigger,
        /// <summary>
        /// stored procedure
        /// </summary>
        StoredProcedure,
        /// <summary>
        /// function
        /// </summary>
        Function,
        /// <summary>
        /// sequence
        /// </summary>
        Sequence,
        /// <summary>
        /// package
        /// </summary>
        Package,
        /// <summary>
        /// user defined data type
        /// </summary>
        UserDataType,
        /// <summary>
        /// user defined table type
        /// </summary>
        UserTableType
    }
}