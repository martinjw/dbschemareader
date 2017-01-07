using System;
using System.Collections.Generic;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Extenders
{
    /// <summary>
    /// Extend a database table
    /// </summary>
    public interface IExtendTable
    {
        /// <summary>
        /// Add additional information to database table.
        /// </summary>
        void Execute(DatabaseTable databaseTable, DbConnection connection);
    }
}
