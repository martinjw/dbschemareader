using System.Data;

namespace DatabaseSchemaReader.Conversion.KeyMaps
{
    internal class SequenceKeyMap
    {
        public SequenceKeyMap(DataTable dt)
        {
            //oracle
            Key = "SEQUENCE_NAME";
            OwnerKey = "SEQUENCE_OWNER";
            MinValueKey = "MIN_VALUE";
            MaxValueKey = "MAX_VALUE";
            IncrementKey = "INCREMENT_BY";
            //DDTek.Oracle is different
            if (!dt.Columns.Contains(OwnerKey)) OwnerKey = "SEQUENCE_SCHEMA";
            //Devart.Data.Oracle
            if (!dt.Columns.Contains(Key)) Key = "NAME";
            if (!dt.Columns.Contains(OwnerKey)) OwnerKey = "SCHEMA";
            if (!dt.Columns.Contains(MinValueKey)) MinValueKey = "MINVALUE";
            if (!dt.Columns.Contains(MaxValueKey)) MaxValueKey = "MAXVALUE";
            if (!dt.Columns.Contains(IncrementKey)) IncrementKey = "INCREMENTBY";
            //Firebird generators
            if (!dt.Columns.Contains(OwnerKey)) OwnerKey = "GENERATOR_SCHEMA";
            if (!dt.Columns.Contains(Key)) Key = "GENERATOR_NAME";
            //Devart.Data.PostgreSql
            if (!dt.Columns.Contains(MinValueKey)) MinValueKey = null;
            if (!dt.Columns.Contains(MaxValueKey)) MaxValueKey = null;
            if (!dt.Columns.Contains(IncrementKey)) IncrementKey = null;

            MaxValueKey = MaxValueKey;
            MinValueKey = MinValueKey;
            IncrementKey = IncrementKey;
            OwnerKey = OwnerKey;
            Key = Key;
        }

        public string MaxValueKey { get; private set; }

        public string MinValueKey { get; private set; }

        public string IncrementKey { get; private set; }

        public string OwnerKey { get; private set; }

        public string Key { get; private set; }
    }
}