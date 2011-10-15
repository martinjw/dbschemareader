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
            //sybase ultralite
            if (!dt.Columns.Contains(DefaultKey)) DefaultKey = "default";
            if (!dt.Columns.Contains(NullableKey)) NullableKey = "nulls";
            if (!dt.Columns.Contains(LengthKey)) LengthKey = null;
            if (!dt.Columns.Contains(PrecisionKey)) PrecisionKey = null;
            if (!dt.Columns.Contains(ScaleKey)) ScaleKey = null;


            if (!dt.Columns.Contains(DefaultKey)) DefaultKey = null; //not in Oracle catalog
            if (!dt.Columns.Contains(AutoIncrementKey)) AutoIncrementKey = null;
            if (!dt.Columns.Contains(PrimaryKeyKey)) PrimaryKeyKey = null;
            if (!dt.Columns.Contains(UniqueKey)) UniqueKey = null;
            if (!dt.Columns.Contains(OrdinalKey)) OrdinalKey = null;
            if (!dt.Columns.Contains(DatatypeKey)) DatatypeKey = null;

        }

        private void CheckDb2(DataTable dt)
        {
            if (dt.Columns.Contains("data_type_name")) DatatypeKey = "data_type_name";
            if (!dt.Columns.Contains(PrecisionKey)) PrecisionKey = "column_size";
            if (!dt.Columns.Contains(ScaleKey)) ScaleKey = "decimal_digits";
            if (!dt.Columns.Contains(DefaultKey)) DefaultKey = "column_def";
        }

        private void CheckDevartPostgreSql(DataTable dt)
        {
            if (!dt.Columns.Contains(OrdinalKey)) OrdinalKey = "position";
            if (!dt.Columns.Contains(TableKey)) TableKey = "table";
            if (!dt.Columns.Contains(Key)) Key = "name";
            if (!dt.Columns.Contains(DatatypeKey)) DatatypeKey = "typename";
            if (!dt.Columns.Contains(UniqueKey)) UniqueKey = "isunique";
            if (!dt.Columns.Contains(DefaultKey)) DefaultKey = "defaultvalue";
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
    }
}