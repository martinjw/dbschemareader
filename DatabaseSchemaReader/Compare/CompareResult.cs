using System;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Compare
{
    /// <summary>
    /// Comparison Result, comprising a <see cref="ResultType"/> (Add/Delete/Change) and a <see cref="SchemaObjectType"/> (Table, Column, Constraint etc)
    /// </summary>
    public class CompareResult
    {
        /// <summary>
        /// Gets or sets the type of the schema object  (Table, Column, Constraint etc).
        /// </summary>
        /// <value>
        /// The type of the schema object.
        /// </value>
        public SchemaObjectType SchemaObjectType { get; set; }
        /// <summary>
        /// Gets or sets the type of the result  (Add/Delete/Change).
        /// </summary>
        /// <value>
        /// The type of the result.
        /// </value>
        public ResultType ResultType { get; set; }
        /// <summary>
        /// Gets or sets the name of the object. For certain types (eg Columns) you also need the <see cref="TableName"/>.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the name of the parent table (if applicable- required for Columns, Constraints, Indexes, Triggers)
        /// </summary>
        /// <value>
        /// The name of the table.
        /// </value>
        public string TableName { get; set; }

        /// <summary>
        /// Gets or sets the schema owner.
        /// </summary>
        /// <value>
        /// The schema owner.
        /// </value>
        public string SchemaOwner { get; set; }

        /// <summary>
        /// Gets or sets the SQL script.
        /// </summary>
        /// <value>
        /// The script.
        /// </value>
        public string Script { get; set; }
        /// <summary>
        /// Finds the object in the specified database schema, or NULL
        /// </summary>
        /// <param name="databaseSchema">The database schema.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">databaseSchema</exception>
        /// <exception cref="System.InvalidOperationException">
        /// Name required
        /// or
        /// TableName required
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        public INamedObject Find(DatabaseSchema databaseSchema)
        {
            if (databaseSchema == null) throw new ArgumentNullException("databaseSchema");
            if (string.IsNullOrEmpty(Name)) throw new InvalidOperationException("Name required");
            DatabaseTable table;
            switch (SchemaObjectType)
            {
                case SchemaObjectType.Table:
                    return databaseSchema.FindTableByName(Name, SchemaOwner);
                case SchemaObjectType.View:
                    return databaseSchema.Views.Find(v => Name.Equals(v.Name, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(v.SchemaOwner, SchemaOwner, StringComparison.OrdinalIgnoreCase));
                case SchemaObjectType.Column:
                    if (string.IsNullOrEmpty(TableName)) throw new InvalidOperationException("TableName required");
                    table = databaseSchema.FindTableByName(TableName, SchemaOwner);
                    return table.FindColumn(Name);
                case SchemaObjectType.Constraint:
                    if (string.IsNullOrEmpty(TableName)) throw new InvalidOperationException("TableName required");
                    table = databaseSchema.FindTableByName(TableName, SchemaOwner);
                    if (table.PrimaryKey != null && table.PrimaryKey.Name == Name) return table.PrimaryKey;
                    var constraint = table.ForeignKeys.FindByName(Name);
                    if (constraint != null) return constraint;
                    constraint = table.CheckConstraints.FindByName(Name);
                    if (constraint != null) return constraint;
                    constraint = table.UniqueKeys.FindByName(Name);
                    if (constraint != null) return constraint;
                    //shouldn't fall through to here
                    return null;
                case SchemaObjectType.Index:
                    if (string.IsNullOrEmpty(TableName)) throw new InvalidOperationException("TableName required");
                    table = databaseSchema.FindTableByName(TableName, SchemaOwner);
                    return table.Indexes.Find(x => Name.Equals(x.Name, StringComparison.OrdinalIgnoreCase));
                case SchemaObjectType.Trigger:
                    if (string.IsNullOrEmpty(TableName)) throw new InvalidOperationException("TableName required");
                    table = databaseSchema.FindTableByName(TableName, SchemaOwner);
                    return table.Triggers.Find(x => Name.Equals(x.Name, StringComparison.OrdinalIgnoreCase));
                case SchemaObjectType.StoredProcedure:
                    return databaseSchema.StoredProcedures.Find(x => Name.Equals(x.Name, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(x.SchemaOwner, SchemaOwner, StringComparison.OrdinalIgnoreCase));
                case SchemaObjectType.Function:
                    return databaseSchema.Functions.Find(x => Name.Equals(x.Name, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(x.SchemaOwner, SchemaOwner, StringComparison.OrdinalIgnoreCase));
                case SchemaObjectType.Sequence:
                    return databaseSchema.Sequences.Find(x => Name.Equals(x.Name, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(x.SchemaOwner, SchemaOwner, StringComparison.OrdinalIgnoreCase));
                case SchemaObjectType.Package:
                    return databaseSchema.Packages.Find(x => Name.Equals(x.Name, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(x.SchemaOwner, SchemaOwner, StringComparison.OrdinalIgnoreCase));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
