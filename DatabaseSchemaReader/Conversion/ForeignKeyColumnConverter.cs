using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Conversion
{
    class ForeignKeyColumnConverter
    {
        private readonly IList<DatabaseConstraint> _foreignKeys = new List<DatabaseConstraint>();

        public ForeignKeyColumnConverter(DataTable dt)
        {
            ConvertColumns(dt);
        }

        public void AddForeignKeyColumns(ICollection<DatabaseConstraint> foreignKeys)
        {
            if (foreignKeys == null)
                throw new ArgumentNullException("foreignKeys");
            if (foreignKeys.Count == 0) return; //no fks to match
            foreach (var foreignKey in foreignKeys)
            {
                var fk = _foreignKeys.FirstOrDefault(x => x.Name == foreignKey.Name);
                if (fk == null)
                {
                    continue; //throw here?
                }
                //copy the columns across
                foreignKey.Columns.AddRange(fk.Columns);
            }
        }

        private void ConvertColumns(DataTable dt)
        {
            if (dt == null) return;
            if (dt.Rows.Count == 0) return; //no rows to add
            string key = "CONSTRAINT_NAME";
            string tableKey = "TABLE_NAME";
            string columnKey = "COLUMN_NAME";
            if (!dt.Columns.Contains(key)) key = "foreignkey";
            if (!dt.Columns.Contains(tableKey)) tableKey = "table";
            if (!dt.Columns.Contains(columnKey)) columnKey = "name";
            if (!dt.Columns.Contains(columnKey)) columnKey = "FKEY_FROM_COLUMN"; //VistaDB

            //this could be more than one table, so filter the view
            foreach (DataRowView row in dt.DefaultView)
            {
                string name = row[key].ToString();
                string tableName = row[tableKey].ToString();
                var fk = _foreignKeys.FirstOrDefault(x => x.Name == name);
                if (fk == null)
                {
                    fk = CreateNewForeignKey(tableName, name);
                }

                string col = row[columnKey].ToString();
                //only add it once
                if (!fk.Columns.Contains(col))
                    fk.Columns.Add(col);
            }
        }

        private static DatabaseConstraint CreateNewForeignKey(string tableName, string name)
        {
            return new DatabaseConstraint
                            {
                                ConstraintType = ConstraintType.ForeignKey,
                                Name = name,
                                TableName = tableName
                            };
        }
    }
}
