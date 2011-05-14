using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Conversion
{
    /// <summary>
    /// Converts the DataTables returned by DbConnection.GetSchema into objects.
    /// Resolves differences between SqlServer and Oracle names
    /// </summary>
    /// <remarks>
    /// SqlServer: http://msdn.microsoft.com/en-us/library/ms254969.aspx
    /// Oracle: http://msdn.microsoft.com/en-us/library/cc716723.aspx
    /// Also supports OleDb, MySql, SQLite, DataDirect and Devart providers
    /// ODP: http://download.oracle.com/docs/html/E15167_01/appSpecificSchema.htm
    /// Devart: http://www.devart.com/dotconnect/oracle/docs/MetaData.html
    /// </remarks>
    internal static class SchemaConverter
    {

        /// <summary>
        /// Converts the "Tables" DataTable into <see cref="DatabaseTable"/> objects.
        /// </summary>
        /// <remarks>
        /// Note the SqlServer DataTable includes views, which we explicitly remove. 
        /// </remarks>
        public static List<DatabaseTable> Tables(DataTable dt)
        {
            List<DatabaseTable> list = new List<DatabaseTable>();
            //sql server
            string key = "TABLE_NAME";
            string ownerKey = "TABLE_SCHEMA";
            string typeKey = "TABLE_TYPE";
            //oracle
            if (!dt.Columns.Contains(ownerKey)) ownerKey = "OWNER";
            if (!dt.Columns.Contains(typeKey)) typeKey = "TYPE";
            //Devart.Data.Oracle - TABLE_NAME is NAME
            if (!dt.Columns.Contains(key)) key = "NAME";
            if (!dt.Columns.Contains(ownerKey)) ownerKey = "SCHEMA";
            //Devart.Data.PostgreSql
            if (!dt.Columns.Contains(typeKey)) typeKey = "tabletype";

            foreach (DataRow row in dt.Rows)
            {
                string type = row[typeKey].ToString();
                //Sql server has base tables and views. Oracle has system and user
                if (!type.Equals("TABLE", StringComparison.OrdinalIgnoreCase) &&
                    !type.Equals("BASE TABLE", StringComparison.OrdinalIgnoreCase) &&
                   !type.Equals("User", StringComparison.OrdinalIgnoreCase)) continue;
                DatabaseTable t = new DatabaseTable();
                t.Name = row[key].ToString();
                //exclude Oracle bin tables
                if (t.Name.StartsWith("BIN$", StringComparison.OrdinalIgnoreCase)) continue;
                t.SchemaOwner = row[ownerKey].ToString();
                list.Add(t);
            }
            return list;
        }

        /// <summary>
        /// Finds the first schema (usually COLUMNS table)
        /// </summary>
        public static string FindSchema(DataTable dt)
        {
            //sql server
            string ownerKey = "TABLE_SCHEMA";
            //oracle
            if (!dt.Columns.Contains(ownerKey)) ownerKey = "OWNER";
            //Devart.Data.Oracle - TABLE_NAME is NAME
            if (!dt.Columns.Contains(ownerKey)) ownerKey = "SCHEMA";

            //find the first table and return it
            foreach (DataRow row in dt.Rows)
            {
                return row[ownerKey].ToString();
            }
            return null;
        }

        /// <summary>
        /// Converts the "Views" DataTable into <see cref="DatabaseView"/> objects.
        /// </summary>
        public static List<DatabaseView> Views(DataTable dt)
        {
            List<DatabaseView> list = new List<DatabaseView>();
            string key = "TABLE_NAME"; //yep, it's Table_Name in SqlServer.
            string ownerKey = "TABLE_SCHEMA";
            string definition = "TEXT";
            //mysql
            if (!dt.Columns.Contains(definition)) definition = "VIEW_DEFINITION";
            //firebird
            if (!dt.Columns.Contains(ownerKey)) ownerKey = "VIEW_SCHEMA"; //always null
            if (!dt.Columns.Contains(definition)) definition = "DEFINITION";
            //oracle
            if (!dt.Columns.Contains(key)) key = "VIEW_NAME";
            if (!dt.Columns.Contains(ownerKey)) ownerKey = "OWNER";
            //Oracle does not expose ViewColumns, only the raw sql.
            bool hasSql = dt.Columns.Contains(definition);
            //Devart.Data.Oracle
            if (!dt.Columns.Contains(key)) key = "NAME";
            if (!dt.Columns.Contains(ownerKey)) ownerKey = "SCHEMA";

            foreach (DataRow row in dt.Rows)
            {
                DatabaseView t = new DatabaseView();
                t.Name = row[key].ToString();
                t.SchemaOwner = row[ownerKey].ToString();
                if (hasSql) t.Sql = row[definition].ToString();
                list.Add(t);
            }
            return list;
        }

        /// <summary>
        /// Converts the "Columns" DataTable into <see cref="DatabaseColumn"/> objects
        /// </summary>
        public static List<DatabaseColumn> Columns(DataTable dt)
        {
            return Columns(dt, null);
        }

        /// <summary>
        /// Converts the "Columns" DataTable into <see cref="DatabaseColumn"/> objects for a specified table
        /// </summary>
        public static List<DatabaseColumn> Columns(DataTable dt, string tableName)
        {
            List<DatabaseColumn> list = new List<DatabaseColumn>();
            //sql server
            string key = "column_name";
            string tableKey = "table_name";
            string ordinalKey = "ordinal_position";
            string datatypeKey = "data_type";
            string nullableKey = "is_nullable";
            string lengthKey = "character_maximum_length";
            string precisionKey = "numeric_precision";
            string scaleKey = "numeric_scale";
            string dateTimePrecision = "datetime_precision";
            string defaultKey = "column_default";
            //oracle
            if (!dt.Columns.Contains(ordinalKey)) ordinalKey = "id";
            if (!dt.Columns.Contains(datatypeKey)) datatypeKey = "datatype";
            if (!dt.Columns.Contains(nullableKey)) nullableKey = "nullable";
            if (!dt.Columns.Contains(lengthKey)) lengthKey = "length";
            if (!dt.Columns.Contains(precisionKey)) precisionKey = "precision";
            if (!dt.Columns.Contains(scaleKey)) scaleKey = "scale";
            if (!dt.Columns.Contains(dateTimePrecision)) dateTimePrecision = null;
            //sqlite
            string autoIncrementKey = "AUTOINCREMENT";
            string primaryKeyKey = "PRIMARY_KEY";
            string uniqueKey = "UNIQUE";
            //firebird
            if (!dt.Columns.Contains(datatypeKey)) datatypeKey = "column_data_type";
            if (!dt.Columns.Contains(lengthKey)) lengthKey = "COLUMN_SIZE";
            //devart.Data.PostgreSql
            if (!dt.Columns.Contains(ordinalKey)) ordinalKey = "position";
            if (!dt.Columns.Contains(tableKey)) tableKey = "table";
            if (!dt.Columns.Contains(key)) key = "name";
            if (!dt.Columns.Contains(datatypeKey)) datatypeKey = "typename";
            if (!dt.Columns.Contains(uniqueKey)) uniqueKey = "isunique";
            if (!dt.Columns.Contains(defaultKey)) defaultKey = "defaultvalue";

            if (!dt.Columns.Contains(defaultKey)) defaultKey = null; //not in Oracle catalog
            if (!dt.Columns.Contains(autoIncrementKey)) autoIncrementKey = null;
            if (!dt.Columns.Contains(primaryKeyKey)) primaryKeyKey = null;
            if (!dt.Columns.Contains(uniqueKey)) uniqueKey = null;

            CreateDefaultView(dt, ordinalKey, tableKey, tableName);

            foreach (DataRowView row in dt.DefaultView)
            {
                DatabaseColumn column = new DatabaseColumn();
                column.Name = row[key].ToString();
                column.TableName = row[tableKey].ToString();
                column.Ordinal = Convert.ToInt32(row[ordinalKey], CultureInfo.CurrentCulture);
                column.DbDataType = row[datatypeKey].ToString();

                AddNullability(row, nullableKey, column);
                //the length unless it's an OleDb blob or clob
                column.Length = GetNullableInt(row[lengthKey]);
                column.Precision = GetNullableInt(row[precisionKey]);
                column.Scale = GetNullableInt(row[scaleKey]);
                if (dateTimePrecision != null)
                {
                    column.DateTimePrecision = GetNullableInt(row[dateTimePrecision]);
                }

                AddColumnDefault(row, defaultKey, column);
                if (!string.IsNullOrEmpty(primaryKeyKey) && (bool)row[primaryKeyKey])
                    column.IsPrimaryKey = true;
                if (!string.IsNullOrEmpty(autoIncrementKey) && (bool)row[autoIncrementKey])
                    column.IsIdentity = true;
                if (!string.IsNullOrEmpty(uniqueKey) && CastToBoolean(row, uniqueKey))
                    column.IsUniqueKey = true;

                list.Add(column);
            }
            return list;
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
            //sqlite has a boolean type
            return ((bool)row[key]);
        }

        private static void AddNullability(DataRowView row, string nullableKey, DatabaseColumn column)
        {
            column.Nullable = CastToBoolean(row, nullableKey);
        }

        private static void CreateDefaultView(DataTable dt, string ordinalKey, string tableKey, string tableName)
        {
            dt.DefaultView.Sort = ordinalKey;
            //this could be more than one table, so filter the view
            if (!string.IsNullOrEmpty(tableName))
                dt.DefaultView.RowFilter = "[" + tableKey + "] = '" + tableName + "'";
        }

        public static List<DataType> DataTypes(DataTable dataTable)
        {
            List<DataType> list = new List<DataType>();
            if (dataTable == null || dataTable.Rows.Count == 0) return list;

            foreach (DataRow row in dataTable.Rows)
            {
                string typeName = row["TypeName"].ToString();
                string netDataType = row["DataType"].ToString();
                DataType d = new DataType(typeName, netDataType);
                d.ProviderDbType = Convert.ToInt32(row["ProviderDbType"], CultureInfo.InvariantCulture);
                d.LiteralPrefix = row["LiteralPrefix"].ToString();
                d.LiteralSuffix = row["LiteralSuffix"].ToString();
                d.CreateFormat = row["CreateFormat"].ToString();
                list.Add(d);
            }
            return list;
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
