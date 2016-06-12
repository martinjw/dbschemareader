using DatabaseSchemaReader.ProviderSchemaReaders.Converters.KeyMaps;
using DatabaseSchemaReader.DataSchema;
using System;
using System.Data;
using System.Globalization;
using DatabaseSchemaReader.ProviderSchemaReaders.Databases;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Converters.RowConverters
{
    class ColumnRowConverter
    {
        private readonly ColumnsKeyMap _keyMap;

        public ColumnRowConverter(ColumnsKeyMap keyMap)
        {
            _keyMap = keyMap;
        }

        public DatabaseColumn Convert(IDataRecord row)
        {
            var column = new DatabaseColumn();
            var columnsKeyMap = _keyMap;
            var hasIsUnsigned = !string.IsNullOrEmpty(columnsKeyMap.IsUnsignedKey);
            column.Name = row[columnsKeyMap.Key].ToString();
            column.TableName = row[columnsKeyMap.TableKey].ToString();

            if (!string.IsNullOrEmpty(columnsKeyMap.SchemaKey))
                column.SchemaOwner = row[columnsKeyMap.SchemaKey].ToString();
            if (string.Equals("sqlite_default_schema", column.SchemaOwner, StringComparison.OrdinalIgnoreCase))
                column.SchemaOwner = string.Empty;

            if (!string.IsNullOrEmpty(columnsKeyMap.OrdinalKey))
                column.Ordinal = System.Convert.ToInt32(row[columnsKeyMap.OrdinalKey], CultureInfo.CurrentCulture);
            if (!string.IsNullOrEmpty(columnsKeyMap.DatatypeKey))
                column.DbDataType = row[columnsKeyMap.DatatypeKey].ToString();
            if (hasIsUnsigned && row.GetBoolean(columnsKeyMap.IsUnsignedKey))
                column.DbDataType += " unsigned";

            column.Nullable = row.GetBoolean(columnsKeyMap.NullableKey);
            //the length unless it's an OleDb blob or clob
            if (!string.IsNullOrEmpty(columnsKeyMap.LengthKey))
                column.Length = row.GetNullableInt(columnsKeyMap.LengthKey);
            if (!string.IsNullOrEmpty(columnsKeyMap.DataLengthKey))
            {
                //oracle only
                var dataLength = row.GetNullableInt(columnsKeyMap.DataLengthKey);
                //column length already set for char/varchar. For other data types, get data length
                if (column.Length < 1)
                    column.Length = dataLength;
            }
            if (!string.IsNullOrEmpty(columnsKeyMap.PrecisionKey))
                column.Precision = row.GetNullableInt(columnsKeyMap.PrecisionKey);
            if (!string.IsNullOrEmpty(columnsKeyMap.ScaleKey))
                column.Scale = row.GetNullableInt(columnsKeyMap.ScaleKey);
            if (columnsKeyMap.DateTimePrecision != null)
            {
                column.DateTimePrecision = row.GetNullableInt(columnsKeyMap.DateTimePrecision);
            }

            AddColumnDefault(row, columnsKeyMap.DefaultKey, column);
            if (!string.IsNullOrEmpty(columnsKeyMap.PrimaryKeyKey) && (bool)row[columnsKeyMap.PrimaryKeyKey])
                column.IsPrimaryKey = true;
            if (!string.IsNullOrEmpty(columnsKeyMap.AutoIncrementKey) && (bool)row[columnsKeyMap.AutoIncrementKey])
                column.IsAutoNumber = true;
            if (!string.IsNullOrEmpty(columnsKeyMap.UniqueKey) && row.GetBoolean(columnsKeyMap.UniqueKey))
                column.IsUniqueKey = true;

            return column;
        }

        private static void AddColumnDefault(IDataRecord row, string defaultKey, DatabaseColumn column)
        {
            if (string.IsNullOrEmpty(defaultKey)) return;
            string d = row[defaultKey].ToString();
            if (!string.IsNullOrEmpty(d)) column.DefaultValue = d.Trim(new[] { ' ', '\'', '=' });
        }
    }
}