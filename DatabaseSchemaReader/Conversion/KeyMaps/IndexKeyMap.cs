using System.Data;

namespace DatabaseSchemaReader.Conversion.KeyMaps
{
    internal class IndexKeyMap
    {
        public IndexKeyMap(DataTable dt)
        {
            UniqueKey = "UNIQUE";
            PrimaryKey = "PRIMARY";

            //sql server
            Key = "CONSTRAINT_NAME";
            TableKey = "TABLE_NAME";
            SchemaKey = "TABLE_SCHEMA";
            ColumnKey = "COLUMN_NAME";
            OrdinalKey = "ORDINAL_POSITION";
            //oracle
            Typekey = "INDEX_TYPE";

            if (!dt.Columns.Contains(SchemaKey)) SchemaKey = "INDEX_SCHEMA";
            if (!dt.Columns.Contains(SchemaKey)) SchemaKey = "OWNER";
            if (!dt.Columns.Contains(Key)) Key = "INDEX_NAME";
            if (!dt.Columns.Contains(SchemaKey)) SchemaKey = "INDEX_OWNER";
            if (!dt.Columns.Contains(OrdinalKey)) OrdinalKey = "COLUMN_POSITION";
            if (!dt.Columns.Contains(UniqueKey)) UniqueKey = "UNIQUENESS";
            //mysql
            if (!dt.Columns.Contains(SchemaKey)) SchemaKey = "INDEX_SCHEMA";
            //Devart.Data.Oracle
            if (!dt.Columns.Contains(Key)) Key = "INDEX"; //IndexColumns
            if (!dt.Columns.Contains(Key)) Key = "NAME"; //Indexes
            if (!dt.Columns.Contains(UniqueKey)) UniqueKey = "ISUNIQUE";
            if (!dt.Columns.Contains(SchemaKey)) SchemaKey = "SCHEMA";
            if (!dt.Columns.Contains(TableKey)) TableKey = "TABLE";
            if (!dt.Columns.Contains(OrdinalKey)) OrdinalKey = "POSITION";
            if (!dt.Columns.Contains(ColumnKey)) ColumnKey = "NAME";
            //devart.data.postgresql
            if (!dt.Columns.Contains(Key)) Key = "indexname";
            //sqlite
            if (!dt.Columns.Contains(PrimaryKey)) PrimaryKey = "PRIMARY_KEY";
            //postgresql
            if (!dt.Columns.Contains(OrdinalKey)) OrdinalKey = null;
            //sqlserver 2008 - HEAP CLUSTERED NONCLUSTERED XML SPATIAL
            //sys_indexes has is_unique but it's not exposed. 
            if (!dt.Columns.Contains(Typekey)) Typekey = "type_desc";

            //pre 2008 sql server
            if (!dt.Columns.Contains(Typekey)) Typekey = null;

            //indexes and not indexcolumns
            if (!dt.Columns.Contains(ColumnKey)) ColumnKey = null;
            if (!dt.Columns.Contains(UniqueKey)) UniqueKey = null;
            if (!dt.Columns.Contains(PrimaryKey)) PrimaryKey = null;
            if (!dt.Columns.Contains(SchemaKey)) SchemaKey = null;
        }

        public string Typekey { get; private set; }

        public string UniqueKey { get; private set; }

        public string PrimaryKey { get; private set; }

        public string ColumnKey { get; private set; }

        public string OrdinalKey { get; private set; }

        public string TableKey { get; private set; }

        public string SchemaKey { get; private set; }

        public string Key { get; private set; }
    }
}