namespace DatabaseSchemaReader.ProviderSchemaReaders.Converters.KeyMaps
{
    class ColumnsKeyMap
    {
        public ColumnsKeyMap()
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
        }

        public string DefaultKey { get; set; }

        public string DateTimePrecision { get; set; }

        public string PrecisionKey { get; set; }

        public string ScaleKey { get; set; }

        public string UniqueKey { get; set; }

        public string PrimaryKeyKey { get; set; }

        public string AutoIncrementKey { get; set; }

        public string OrdinalKey { get; set; }

        public string TableKey { get; set; }

        public string NullableKey { get; set; }

        public string LengthKey { get; set; }

        public string Key { get; set; }

        public string DatatypeKey { get; set; }

        public string DataLengthKey { get; set; }

        public string IsUnsignedKey { get; set; }

        public string SchemaKey { get; set; }
    }
}