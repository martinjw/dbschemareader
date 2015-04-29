using System;
using System.Collections.Generic;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader
{
    /// <summary>
    /// Read database schema into schema objects
    /// </summary>
    public interface IDatabaseReader : IDisposable
    {
        /// <summary>
        /// Gets or sets the owner user. Always set it with Oracle (otherwise you'll get SYS, MDSYS etc...)
        /// </summary>
        /// <value>The user.</value>
        string Owner { get; set; }

        /// <summary>
        /// Gets the database schema. Only call AFTER calling <see cref="ReadAll"/> or one or more other methods such as <see cref="AllTables"/>. A collection of Tables, Views and StoredProcedures. Use <see cref="DataSchema.DatabaseSchemaFixer.UpdateReferences"/> to update object references after loaded. Use <see cref="DataSchema.DatabaseSchemaFixer.UpdateDataTypes"/> to add datatypes from DbDataType string after loaded.
        /// </summary>
        DatabaseSchema DatabaseSchema { get; }

        /// <summary>
        /// Gets all of the schema in one call. 
        /// </summary>
        /// <returns></returns>
        DatabaseSchema ReadAll();

        /// <summary>
        /// Gets the users (specifically for Oracle)
        /// </summary>
        IList<DatabaseUser> AllUsers();

        /// <summary>
        /// Gets all tables (just names, no columns).
        /// </summary>
        IList<DatabaseTable> TableList();

        /// <summary>
        /// Gets all tables (plus constraints, indexes and triggers).
        /// </summary>
        IList<DatabaseTable> AllTables();

        /// <summary>
        /// Gets all views.
        /// </summary>
        IList<DatabaseView> AllViews();

        /// <summary>
        /// Gets the table. If <see cref="DatabaseReader.Owner"/> is specified, it is used.
        /// </summary>
        /// <param name="tableName">Name of the table. Oracle names can be case sensitive.</param>
        DatabaseTable Table(string tableName);

        /// <summary>
        /// Gets all stored procedures (no arguments, for Oracle no packages)
        /// </summary>
        IList<DatabaseStoredProcedure> StoredProcedureList();

        /// <summary>
        /// Gets all stored procedures (and functions) with their arguments
        /// </summary>
        /// <remarks>
        /// <para>We also get the source (if available)</para>
        /// <para>We don't get functions here.</para>
        /// <para>In Oracle stored procedures are often in packages. We read the non-packaged stored procedures, then add packaged stored procedures if they have arguments. If they don't have arguments, they are not found.</para>
        /// </remarks>
        IList<DatabaseStoredProcedure> AllStoredProcedures();

        /// <summary>
        /// Gets all datatypes (and updates columns/arguments if already loaded)
        /// </summary>
        IList<DataType> DataTypes();
    }
}