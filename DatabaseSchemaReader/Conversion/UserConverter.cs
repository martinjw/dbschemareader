using System.Collections.Generic;
using System.Data;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Conversion
{
    internal static class UserConverter
    {
        public static IList<DatabaseUser> Convert(DataTable table)
        {
            var list = new List<DatabaseUser>();
            //sql
            string key = "user_name";
            //oracle
            if (!table.Columns.Contains(key)) key = "name";
            //mysql
            if (!table.Columns.Contains(key)) key = "username";
            foreach (DataRow row in table.Rows)
            {
                var u = new DatabaseUser();
                u.Name = row[key].ToString();
                list.Add(u);
            }
            return list;
        }
    }
}
