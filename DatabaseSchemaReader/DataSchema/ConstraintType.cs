namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// Type of constraint (primary key, foreign key...)
    /// </summary>
    public enum ConstraintType
    {
        /// <summary>
        /// 
        /// </summary>
        PrimaryKey,
        /// <summary>
        /// 
        /// </summary>
        ForeignKey,
        /// <summary>
        /// 
        /// </summary>
        UniqueKey,
        /// <summary>
        /// 
        /// </summary>
        Check,
        /// <summary>
        /// Default constraints (SQLServer)
        /// </summary>
        Default
    }
}