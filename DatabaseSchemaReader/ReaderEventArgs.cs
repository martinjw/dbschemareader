using System;

namespace DatabaseSchemaReader
{
    /// <summary>
    /// Event Args during schema reading
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class ReaderEventArgs : EventArgs
    {
        private ReaderEventArgs(ProgressType progressType, SchemaObjectType schemaObjectType, string name, int? index, int? count)
        {
            ProgressType = progressType;
            SchemaObjectType = schemaObjectType;
            Name = name;
            Index = index;
            Count = count;
        }

        /// <summary>
        /// ProgressType (Reading, Processing)
        /// </summary>
        public ProgressType ProgressType { get; private set; }

        /// <summary>
        /// Gets the type of the schema object.
        /// </summary>
        public SchemaObjectType SchemaObjectType { get; private set; }

        /// <summary>
        /// Name of Tables/View/SP
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Index of current Name inside the collection if available
        /// </summary>
        public int? Index { get; private set; }

        /// <summary>
        /// Count of entries if available
        /// </summary>
        public int? Count { get; private set; }

        internal static void RaiseEvent(EventHandler<ReaderEventArgs> eventHandler,
            object sender,
            ProgressType progressType,
            SchemaObjectType schemaObjectType)
        {
            RaiseEvent(eventHandler, sender, progressType, schemaObjectType, null, null, null);
        }

        internal static void RaiseEvent(EventHandler<ReaderEventArgs> eventHandler,
            object sender,
            ProgressType progressType,
            SchemaObjectType schemaObjectType,
            string name, int? index, int? count)
        {
            var handler = eventHandler;
            if (handler != null)
            {
                handler(sender, new ReaderEventArgs(progressType, schemaObjectType, name, index, count));
            }
        }
    }

    /// <summary>
    /// Type of current operation
    /// </summary>
    public enum ProgressType
    {
        /// <summary>
        /// Reading from schema (ADO)
        /// </summary>
        ReadingSchema = 0,

        /// <summary>
        /// Processing
        /// </summary>
        Processing,
    }

    /// <summary>
    /// The type of schema object (such as table)
    /// </summary>
    public enum SchemaObjectType
    {
        /// <summary>
        /// The tables
        /// </summary>
        Tables = 0,
        /// <summary>
        /// The views
        /// </summary>
        Views,
        /// <summary>
        /// The columns
        /// </summary>
        Columns,
        /// <summary>
        /// The view columns
        /// </summary>
        ViewColumns,
        /// <summary>
        /// The constraints
        /// </summary>
        Constraints,
        /// <summary>
        /// The descriptions
        /// </summary>
        Descriptions,
        /// <summary>
        /// The stored procedure
        /// </summary>
        StoredProcedure,
        /// <summary>
        /// The functions
        /// </summary>
        Functions,
        /// <summary>
        /// The procedure arguments
        /// </summary>
        ProcedureArguments,
        /// <summary>
        /// The procedure source
        /// </summary>
        ProcedureSource,
        /// <summary>
        /// The users
        /// </summary>
        Users,
        /// <summary>
        /// The sequences
        /// </summary>
        Sequences,
        /// <summary>
        /// The schemas
        /// </summary>
        Schemas,
    }
}