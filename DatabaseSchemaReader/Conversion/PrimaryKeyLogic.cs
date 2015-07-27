using System.Collections.Generic;
using System.Linq;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Conversion
{
    static class PrimaryKeyLogic
    {
        public static void AddPrimaryKey(DatabaseTable table, IList<DatabaseConstraint> primaryKeys)
        {
            if (primaryKeys.Count > 0)
            {
                table.PrimaryKey = primaryKeys[0];
            }
            else if (table.PrimaryKeyColumn != null && table.Columns.Any(c => c.IsPrimaryKey))
            {
                //we couldn't find the primary key, but we can infer it from columns
                table.PrimaryKey = new DatabaseConstraint();
                table.PrimaryKey.ConstraintType = ConstraintType.PrimaryKey;
                table.PrimaryKey.Name = "PRIMARY";
                table.PrimaryKey.Columns.AddRange(table.Columns.Where(c => c.IsPrimaryKey).Select(c => c.Name));
                table.PrimaryKey.RefersToTable = table.Name;
                table.PrimaryKey.RefersToSchema = table.SchemaOwner;
            }
        }
    }
}
