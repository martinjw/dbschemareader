namespace DatabaseSchemaReader.CodeGen
{
    /// <summary>
    /// The target code generation.
    /// </summary>
    public enum CodeTarget
    {
        /// <summary>
        /// Simple plain old CLR entities
        /// </summary>
        Poco,
        /// <summary>
        /// Entities with NHibernate hbm.xml mappings
        /// </summary>
        PocoNHibernateHbm,
        /// <summary>
        /// Entities with NHibernate fluent mappings
        /// </summary>
        PocoNHibernateFluent,
        /// <summary>
        /// Entities with Entity Framework Code First mapping
        /// </summary>
        PocoEntityCodeFirst,
        /// <summary>
        /// Entities for RIA Services (buddy classes)
        /// </summary>
        PocoRiaServices
    }
}
