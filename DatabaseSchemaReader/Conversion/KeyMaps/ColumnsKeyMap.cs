using System.Data;

namespace DatabaseSchemaReader.Conversion.KeyMaps
{
    internal class ColumnsKeyMap
    {
        public ColumnsKeyMap(DataTable dt)
        {
            //sql server
            Key = "column_name";
            TableKey = "table_name";
            SchemaKey = "table_schema";
            OrdinalKey = "ordinal_position";
            DatatypeKey = "data_type";
            NullableKey = "is_nullable";
            LengthKey = "character_maximum_length";
            PrecisionKey = "numeric_precision";
            ScaleKey = "numeric_scale";
            DateTimePrecision = "datetime_precision";
            DefaultKey = "column_default";
            //oracle
            if (!dt.Columns.Contains(OrdinalKey)) OrdinalKey = "id";
            if (!dt.Columns.Contains(DatatypeKey)) DatatypeKey = "datatype";
            if (!dt.Columns.Contains(NullableKey)) NullableKey = "nullable";
            if (!dt.Columns.Contains(LengthKey)) LengthKey = "length";
            if (!dt.Columns.Contains(PrecisionKey)) PrecisionKey = "precision";
            if (!dt.Columns.Contains(ScaleKey)) ScaleKey = "scale";
            if (!dt.Columns.Contains(DateTimePrecision)) DateTimePrecision = null;
            if (dt.Columns.Contains("DATALENGTH")) DataLengthKey = "DATALENGTH";
            if (!dt.Columns.Contains(SchemaKey)) SchemaKey = "owner";
            //sqlite
            AutoIncrementKey = "AUTOINCREMENT";
            PrimaryKeyKey = "PRIMARY_KEY";
            UniqueKey = "UNIQUE";
            //firebird
            if (!dt.Columns.Contains(DatatypeKey)) DatatypeKey = "column_data_type";
            if (!dt.Columns.Contains(LengthKey)) LengthKey = "COLUMN_SIZE";
            //devart.Data.PostgreSql
            CheckDevartPostgreSql(dt);
            //db2
            CheckDb2(dt);
            //Intersystems Cache
            if (dt.Columns.Contains("TYPE_NAME")) DatatypeKey = "TYPE_NAME";
            //Get​Schema​Table
            CheckGet​Schema​Table(dt);
            //sybase ultralite
            if (!dt.Columns.Contains(DefaultKey)) DefaultKey = "default";
            if (!dt.Columns.Contains(NullableKey)) NullableKey = "nulls";
            if (!dt.Columns.Contains(LengthKey)) LengthKey = null;
            if (!dt.Columns.Contains(PrecisionKey)) PrecisionKey = null;
            if (!dt.Columns.Contains(ScaleKey)) ScaleKey = null;
            //mysql
            if (dt.Columns.Contains("COLUMN_TYPE")) DatatypeKey = "COLUMN_TYPE";
            if (dt.Columns.Contains("IsUnsigned")) IsUnsignedKey = "IsUnsigned";
            if (!dt.Columns.Contains(SchemaKey)) SchemaKey = "TABLE_SCHEMA";
            //Devart.Data.MySQL
            if (!dt.Columns.Contains(SchemaKey)) SchemaKey = "DATABASE";
            //odbc
            if (!dt.Columns.Contains(SchemaKey)) SchemaKey = "TABLE_SCHEM";

            if (!dt.Columns.Contains(DefaultKey)) DefaultKey = null; //not in Oracle catalog
            if (!dt.Columns.Contains(AutoIncrementKey)) AutoIncrementKey = null;
            if (!dt.Columns.Contains(PrimaryKeyKey)) PrimaryKeyKey = null;
            if (!dt.Columns.Contains(UniqueKey)) UniqueKey = null;
            if (!dt.Columns.Contains(OrdinalKey)) OrdinalKey = null;
            if (!dt.Columns.Contains(DatatypeKey)) DatatypeKey = null;
            if (!dt.Columns.Contains(SchemaKey)) SchemaKey = null;
        }

        private void CheckDb2(DataTable dt)
        {
            if (dt.Columns.Contains("data_type_name")) DatatypeKey = "data_type_name";
            if (!dt.Columns.Contains(PrecisionKey)) PrecisionKey = "column_size";
            if (!dt.Columns.Contains(ScaleKey)) ScaleKey = "decimal_digits";
            if (!dt.Columns.Contains(DefaultKey)) DefaultKey = "column_def";
            //iSeries
            if (!dt.Columns.Contains(SchemaKey)) SchemaKey = "SchemaName";
            if (!dt.Columns.Contains(TableKey)) TableKey = "TableName";
            if (!dt.Columns.Contains(Key)) Key = "ColumnName";
            if (!dt.Columns.Contains(OrdinalKey)) OrdinalKey = "OrdinalPosition";
            if (!dt.Columns.Contains(DefaultKey)) DefaultKey = "ColumnDefault";
            if (!dt.Columns.Contains(NullableKey)) NullableKey = "IsNullable";
            if (!dt.Columns.Contains(DatatypeKey)) DatatypeKey = "DataType";
            if (!dt.Columns.Contains(LengthKey)) LengthKey = "CharacterMaximumLength";
            if (!dt.Columns.Contains(PrecisionKey)) PrecisionKey = "NumericPrecision";
            if (!dt.Columns.Contains(ScaleKey)) ScaleKey = "NumericScale";
        }

        private void CheckDevartPostgreSql(DataTable dt)
        {
            if (!dt.Columns.Contains(OrdinalKey)) OrdinalKey = "position";
            if (!dt.Columns.Contains(TableKey)) TableKey = "table";
            if (!dt.Columns.Contains(Key)) Key = "name";
            if (!dt.Columns.Contains(DatatypeKey)) DatatypeKey = "typename";
            if (!dt.Columns.Contains(UniqueKey)) UniqueKey = "isunique";
            if (!dt.Columns.Contains(DefaultKey)) DefaultKey = "defaultvalue";
            if (!dt.Columns.Contains(SchemaKey)) SchemaKey = "schema";
        }
        private void CheckGet​Schema​Table(DataTable dt)
        {
            if (string.IsNullOrEmpty(SchemaKey) || !dt.Columns.Contains(SchemaKey)) SchemaKey = "BaseSchemaName";
            if (string.IsNullOrEmpty(TableKey) || !dt.Columns.Contains(TableKey)) TableKey = "BaseTableName";
            if (string.IsNullOrEmpty(Key) || !dt.Columns.Contains(Key)) Key = "ColumnName";
            if (string.IsNullOrEmpty(OrdinalKey) || !dt.Columns.Contains(OrdinalKey)) OrdinalKey = "ColumnOrdinal";
            // ProviderType is a number which corresponds to the ProviderDbType 
            // returned by connection.GetSchema(DbMetaDataCollectionNames.DataTypes)
            if (dt.Columns.Contains("ProviderType")) DatatypeKey = "ProviderType";
            if (!dt.Columns.Contains(NullableKey)) NullableKey = "AllowDBNull";
            if (string.IsNullOrEmpty(LengthKey) || !dt.Columns.Contains(LengthKey)) LengthKey = "ColumnSize";
            if (!dt.Columns.Contains(PrecisionKey)) PrecisionKey = "NumericPrecision";
            if (!dt.Columns.Contains(ScaleKey)) ScaleKey = "NumericScale";
        }

        public string DefaultKey { get; private set; }

        public string DateTimePrecision { get; private set; }

        public string PrecisionKey { get; private set; }

        public string ScaleKey { get; private set; }

        public string UniqueKey { get; private set; }

        public string PrimaryKeyKey { get; private set; }

        public string AutoIncrementKey { get; private set; }

        public string OrdinalKey { get; private set; }

        public string TableKey { get; set; }

        public string NullableKey { get; private set; }

        public string LengthKey { get; private set; }

        public string Key { get; private set; }

        public string DatatypeKey { get; private set; }

        public string DataLengthKey { get; private set; }

        public string IsUnsignedKey { get; set; }

        public string SchemaKey { get; set; }
    }
}