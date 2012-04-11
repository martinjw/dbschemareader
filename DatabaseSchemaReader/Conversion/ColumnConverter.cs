using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using DatabaseSchemaReader.Conversion.KeyMaps;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Conversion
{
    class ColumnConverter
    {
        private readonly List<DatabaseColumn> _list;
        protected readonly DataTable ColumnsDataTable;

        public ColumnConverter(DataTable columnsDataTable)
        {
            ColumnsDataTable = columnsDataTable;
            _list = new List<DatabaseColumn>();
        }

        protected virtual ColumnsKeyMap LoadColumnsKeyMap()
        {
            return new ColumnsKeyMap(ColumnsDataTable);
        }

        private void ConvertDataTable()
        {
            var columnsKeyMap = new ColumnsKeyMap(ColumnsDataTable);

            foreach (DataRowView row in ColumnsDataTable.DefaultView)
            {
                var column = new DatabaseColumn();
                column.Name = row[columnsKeyMap.Key].ToString();
                column.TableName = row[columnsKeyMap.TableKey].ToString();
                if (!string.IsNullOrEmpty(columnsKeyMap.OrdinalKey))
                    column.Ordinal = Convert.ToInt32(row[columnsKeyMap.OrdinalKey], CultureInfo.CurrentCulture);
                if (!string.IsNullOrEmpty(columnsKeyMap.DatatypeKey))
                    column.DbDataType = row[columnsKeyMap.DatatypeKey].ToString();

                AddNullability(row, columnsKeyMap.NullableKey, column);
                //the length unless it's an OleDb blob or clob
                if (!string.IsNullOrEmpty(columnsKeyMap.LengthKey))
                    column.Length = GetNullableInt(row[columnsKeyMap.LengthKey]);
                if (!string.IsNullOrEmpty(columnsKeyMap.PrecisionKey))
                    column.Precision = GetNullableInt(row[columnsKeyMap.PrecisionKey]);
                if (!string.IsNullOrEmpty(columnsKeyMap.ScaleKey))
                    column.Scale = GetNullableInt(row[columnsKeyMap.ScaleKey]);
                if (columnsKeyMap.DateTimePrecision != null)
                {
                    column.DateTimePrecision = GetNullableInt(row[columnsKeyMap.DateTimePrecision]);
                }

                AddColumnDefault(row, columnsKeyMap.DefaultKey, column);
                if (!string.IsNullOrEmpty(columnsKeyMap.PrimaryKeyKey) && (bool)row[columnsKeyMap.PrimaryKeyKey])
                    column.IsPrimaryKey = true;
                if (!string.IsNullOrEmpty(columnsKeyMap.AutoIncrementKey) && (bool)row[columnsKeyMap.AutoIncrementKey])
                    column.IsIdentity = true;
                if (!string.IsNullOrEmpty(columnsKeyMap.UniqueKey) && CastToBoolean(row, columnsKeyMap.UniqueKey))
                    column.IsUniqueKey = true;

                _list.Add(column);
            }

            // Sort columns according to ordinal to get the original order in CREATE TABLE
            _list.Sort((x, y) => x.Ordinal.CompareTo(y.Ordinal));
        }

        /// <summary>
        /// Converts the "Columns" DataTable into <see cref="DatabaseColumn"/> objects for a specified table
        /// </summary>
        public IEnumerable<DatabaseColumn> Columns(string tableName)
        {
            if (_list.Count == 0) ConvertDataTable();

            return _list.Where(x => x.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Converts the "Columns" DataTable into <see cref="DatabaseColumn"/> objects 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DatabaseColumn> Columns()
        {
            if (_list.Count == 0) ConvertDataTable();
            return _list;
        }

        private static void AddColumnDefault(DataRowView row, string defaultKey, DatabaseColumn column)
        {
            if (string.IsNullOrEmpty(defaultKey)) return;
            string d = row[defaultKey].ToString();
            if (!string.IsNullOrEmpty(d)) column.DefaultValue = d.Trim(new[] { ' ', '\'', '=' });
        }

        private static bool CastToBoolean(DataRowView row, string key)
        {
            string nullable = row[key].ToString();
            //could be Y, YES, N, NO, true, false.
            if (nullable.StartsWith("Y", StringComparison.OrdinalIgnoreCase) || nullable.StartsWith("T", StringComparison.OrdinalIgnoreCase)) //Y or YES
                return true;
            if (nullable.StartsWith("N", StringComparison.OrdinalIgnoreCase) || nullable.StartsWith("F", StringComparison.OrdinalIgnoreCase)) //N or NO
                return false;
            if (nullable == "0") return false;
            if (nullable == "1") return true;
            //sqlite has a boolean type
            return ((bool)row[key]);
        }

        private static void AddNullability(DataRowView row, string nullableKey, DatabaseColumn column)
        {
            column.Nullable = CastToBoolean(row, nullableKey);
        }

        private static int? GetNullableInt(object o)
        {
            try
            {
                return (o != DBNull.Value) ? Convert.ToInt32(o, CultureInfo.CurrentCulture) : (int?)null;
            }
            catch (OverflowException)
            {
                //this occurs for blobs and clobs using the OleDb provider
                return -1;
            }
        }
    }
}
