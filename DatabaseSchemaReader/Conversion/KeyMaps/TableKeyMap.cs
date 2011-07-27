using System.Data;

namespace DatabaseSchemaReader.Conversion.KeyMaps
{
    class TableKeyMap
    {
        public TableKeyMap(DataTable dt)
        {
            //sql server
            TableName = "TABLE_NAME";
            OwnerKey = "TABLE_SCHEMA";
            TypeKey = "TABLE_TYPE";
            //oracle
            if (!dt.Columns.Contains(OwnerKey)) OwnerKey = "OWNER";
            if (!dt.Columns.Contains(TypeKey)) TypeKey = "TYPE";
            //Devart.Data.Oracle - TABLE_NAME is NAME
            if (!dt.Columns.Contains(TableName)) TableName = "NAME";
            if (!dt.Columns.Contains(OwnerKey)) OwnerKey = "SCHEMA";
            //Devart.Data.PostgreSql
            if (!dt.Columns.Contains(TypeKey)) TypeKey = "tabletype";
            //Devart.Data.MySQL
            if (!dt.Columns.Contains(OwnerKey)) OwnerKey = "DATABASE";
            IsDb2 = dt.Columns.Contains("REMARKS");
            //Intersystems Cache
            if (!dt.Columns.Contains(OwnerKey)) OwnerKey = "TABLE_SCHEM";
            //no schema
            if (!dt.Columns.Contains(OwnerKey)) OwnerKey = null;

        }
         public string TableName { get; private set; }
         public string OwnerKey { get; private set; }
         public string TypeKey { get; private set; }
         public bool IsDb2 { get; private set; }
    }
}
