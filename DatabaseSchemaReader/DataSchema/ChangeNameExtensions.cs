using System;
using System.Collections.Generic;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// Extensions to change names
    /// </summary>
    public static class ChangeNameExtensions
    {
        /// <summary>
        /// Change the name of a table and references to it. (But not views or sprocs!)
        /// </summary>
        /// <param name="table"></param>
        /// <param name="newName">The new name of the table</param>
        public static void ChangeName(this DatabaseTable table, string newName)
        {
            if (table == null) return;
            if (string.IsNullOrEmpty(newName)) return;

            var oldName = table.Name;
            table.Name = newName;

            //most of these are convenience
            foreach (var column in table.Columns)
            {
                column.TableName = newName;
            }

            if (table.PrimaryKey != null) table.PrimaryKey.TableName = newName;
            foreach (var chk in table.CheckConstraints)
            {
                chk.TableName = newName;
            }
            foreach (var uniqueKey in table.UniqueKeys)
            {
                uniqueKey.TableName = newName;
            }
            foreach (var def in table.DefaultConstraints)
            {
                def.TableName = newName;
            }
            foreach (var index in table.Indexes)
            {
                index.TableName = newName;
            }

            foreach (var fk in table.ForeignKeys)
            {
                fk.TableName = newName;
            }
            //important
            foreach (var dep in table.ForeignKeyChildren)
            {
                foreach (var fk in dep.ForeignKeys)
                {
                    if (string.Equals(oldName, fk.RefersToTable, StringComparison.OrdinalIgnoreCase))
                    {
                        fk.RefersToTable = newName;
                    }
                }
            }
        }

        /// <summary>
        /// Change the name of a table and references to it. (But not views or sprocs!)
        /// </summary>
        /// <param name="column"></param>
        /// <param name="newName">The new name of the column</param>
        public static void ChangeName(this DatabaseColumn column, string newName)
        {
            if (column == null) return;
            if (string.IsNullOrEmpty(newName)) return;

            var oldName = column.Name;
            column.Name = newName;

            var table = column.Table;
            if (table == null) return; //disconnected - can't do more

            if (table.PrimaryKey != null)
            {
                ChangeConstraint(table.PrimaryKey.Columns, oldName, newName);
            }
            foreach (var fk in table.ForeignKeys)
            {
                ChangeConstraint(fk.Columns, oldName, newName);
            }
            foreach (var uk in table.UniqueKeys)
            {
                ChangeConstraint(uk.Columns, oldName, newName);
            }
        }

        private static void ChangeConstraint(List<string> columns, string oldName, string newName)
        {
            var index = columns.FindIndex(x => x == oldName);
            if (index == -1) return;
            columns[index] = newName;
        }
    }
}