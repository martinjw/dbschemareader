using System.Data;

namespace DatabaseSchemaReader.Conversion.KeyMaps
{
    internal class ViewKeyMap
    {
        public ViewKeyMap(DataTable dt)
        {
            Key = "TABLE_NAME"; //yep, it's Table_Name in SqlServer.
            OwnerKey = "TABLE_SCHEMA";
            Definition = "TEXT";
            TypeKey = "TABLE_TYPE";
            //mysql
            if (!dt.Columns.Contains(Definition)) Definition = "VIEW_DEFINITION";
            //firebird
            if (!dt.Columns.Contains(OwnerKey)) OwnerKey = "VIEW_SCHEMA"; //always null
            if (!dt.Columns.Contains(Definition)) Definition = "DEFINITION";
            //oracle
            if (!dt.Columns.Contains(Key)) Key = "VIEW_NAME";
            if (!dt.Columns.Contains(OwnerKey)) OwnerKey = "OWNER";
            //Oracle does not expose ViewColumns, only the raw sql.
            HasSql = dt.Columns.Contains(Definition);
            //Devart.Data.Oracle
            if (!dt.Columns.Contains(Key)) Key = "NAME";
            if (!dt.Columns.Contains(OwnerKey)) OwnerKey = "SCHEMA";
            //Devart.Data.MySQL
            if (!dt.Columns.Contains(OwnerKey)) OwnerKey = "DATABASE";

            if (!dt.Columns.Contains(TypeKey)) TypeKey = null;
        }

        public string Key { get; private set; }

        public string OwnerKey { get; private set; }

        public string TypeKey { get; private set; }

        public bool HasSql { get; private set; }

        public string Definition { get; private set; }
    }
}