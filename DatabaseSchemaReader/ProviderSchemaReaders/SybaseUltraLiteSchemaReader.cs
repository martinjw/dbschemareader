using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace DatabaseSchemaReader.ProviderSchemaReaders
{
    class SybaseUltraLiteSchemaReader : SchemaExtendedReader
    {
        public SybaseUltraLiteSchemaReader(string connectionString, string providerName)
            : base(connectionString, providerName)
        {
        }

        //sybase UltraLite 12 system views: http://dcx.sybase.com/1201/en/uladmin/fo-db-internals.html
        //it's like Sybase Anywhere only more primitive.
        //(table_name = ? OR ? IS NULL) didn't work, and neither did null/dbnull parameters so there's a hacky string concat. Sorry.

        protected override DataTable Columns(string tableName, DbConnection connection)
        {
            //the GetSchema collection doesn't include datatypes.
            //But it seems to be there in the syscolumn table (as "domain")

            const string sql =
                @"SELECT
t.""table_name"",
c.""column_name"",
c.""default"",
c.""nulls"",
c.""domain"",
c.""domain_info""
FROM syscolumn c, systable t 
WHERE 
c.table_id = t.object_id";

            var columns = (string.IsNullOrEmpty(tableName))
                              ? SybaseCommandForTable(connection, ColumnsCollectionName, sql)
                              : SybaseCommandForTable(connection, ColumnsCollectionName, tableName,
                                                      sql + " AND (t.table_name = ?)");

            //The numbers in syscolumn.domain don't correspond to the ProviderDbType inthe DataTypes collection
            //So we have to create our own mapping.
            var dataTypes = new Dictionary<int, string>();
            dataTypes.Add(1, "SMALLINT");
            dataTypes.Add(2, "INT");
            dataTypes.Add(3, "NUMERIC");
            dataTypes.Add(4, "FLOAT"); //==REAL
            dataTypes.Add(5, "DOUBLE");
            dataTypes.Add(6, "DATE");
            dataTypes.Add(9, "VARCHAR");
            dataTypes.Add(10, "LONG VARCHAR");
            dataTypes.Add(11, "VARBINARY"); //and apparently binary too
            dataTypes.Add(13, "TIMESTAMP"); //==DATETIME
            dataTypes.Add(14, "TIME");
            dataTypes.Add(20, "BIGINT");
            dataTypes.Add(24, "BIT");
            dataTypes.Add(29, "UNIQUEIDENTIFIER");

            columns.Columns.Add("data_type", typeof(string));
            columns.Columns.Add("length", typeof(int));
            columns.Columns.Add("precision", typeof(int));
            foreach (DataRow row in columns.Rows)
            {
                int dataType;
                if (!int.TryParse(row["domain"].ToString(), out dataType)) continue;
                if (!dataTypes.ContainsKey(dataType)) continue;
                var typeName = dataTypes[dataType];
                row["data_type"] = typeName;
                int length;
                if (!int.TryParse(row["domain_info"].ToString(), out length)) continue;
                if (dataType == 9 || dataType == 11) //varchar and varbinary have length
                    row["length"] = length;
                else if (dataType == 3 || dataType == 4 || dataType == 5) //numerics and double have precision
                    row["precision"] = length; //not sure how to get scale?
            }
            return columns;
        }

        protected override DataTable PrimaryKeys(string tableName, DbConnection connection)
        {
            const string sql =
                @"SELECT 
i.index_name AS constraint_name, 
t.table_name,
c.column_name,
ic.""sequence"" AS ordinal_position
FROM sysindex i 
JOIN sysixcol ic ON i.object_id = ic.index_id AND ic.table_id = i.table_id
JOIN systable t ON i.table_id = t.object_id
JOIN syscolumn c ON c.table_id = t.object_id AND ic.column_id = c.object_id
WHERE i.type  = 'primary'";

            var data = (string.IsNullOrEmpty(tableName))
                              ? SybaseCommandForTable(connection, PrimaryKeysCollectionName, sql)
                              : SybaseCommandForTable(connection, PrimaryKeysCollectionName, tableName,
                                                      sql + " AND (t.table_name = ?)");
            return data;
        }

        protected override DataTable ForeignKeys(string tableName, DbConnection connection)
        {
            const string sql = @"SELECT 
i.index_name AS constraint_name, 
t.table_name,
c.column_name,
fkt.table_name AS FK_TABLE,
fki.index_name AS UNIQUE_CONSTRAINT_NAME,
ic.sequence AS ordinal_position
FROM sysindex i
JOIN sysixcol ic 
    ON i.object_id = ic.index_id AND ic.table_id = i.table_id
JOIN systable t 
    ON i.table_id = t.object_id
JOIN syscolumn c 
    ON c.table_id = t.object_id AND ic.column_id = c.object_id
JOIN systable fkt 
    ON fkt.object_id = i.primary_table_id
JOIN sysindex fki
    ON i.primary_index_id = fki.object_id AND i.primary_table_id = fki.table_id
WHERE i.type  = 'foreign'";

            var data = (string.IsNullOrEmpty(tableName))
                              ? SybaseCommandForTable(connection, ForeignKeysCollectionName, sql)
                              : SybaseCommandForTable(connection, ForeignKeysCollectionName, tableName,
                                                      sql + " AND (t.table_name = ?)");
            return data;
        }

        protected override DataTable UniqueKeys(string tableName, DbConnection connection)
        {
            const string sql = @"SELECT 
i.index_name AS constraint_name, 
t.table_name,
c.column_name
FROM sysindex i
JOIN sysixcol ic ON i.object_id = ic.index_id AND ic.table_id = i.table_id
JOIN systable t ON i.table_id = t.object_id
JOIN syscolumn c ON c.table_id = t.object_id AND ic.column_id = c.object_id
WHERE i.type  = 'unique'";

            var data = (string.IsNullOrEmpty(tableName))
                              ? SybaseCommandForTable(connection, UniqueKeysCollectionName, sql)
                              : SybaseCommandForTable(connection, UniqueKeysCollectionName, tableName,
                                                      sql + " AND (t.table_name = ?)");
            return data;
        }
        private DataTable SybaseCommandForTable(DbConnection connection, string dataTableName, string sql)
        {
            DataTable dt = CreateDataTable(dataTableName);

            //create a dataadaptor and fill it
            using (DbDataAdapter da = Factory.CreateDataAdapter())
            {
                da.SelectCommand = connection.CreateCommand();
                da.SelectCommand.CommandText = sql;

                da.Fill(dt);
                return dt;
            }
        }
        private DataTable SybaseCommandForTable(DbConnection connection, string dataTableName, string tableName, string sql)
        {
            DataTable dt = CreateDataTable(dataTableName);

            //create a dataadaptor and fill it
            using (DbDataAdapter da = Factory.CreateDataAdapter())
            {
                da.SelectCommand = connection.CreateCommand();
                da.SelectCommand.CommandText = sql;

                var parameter = AddDbParameter(string.Empty, tableName);
                da.SelectCommand.Parameters.Add(parameter);

                da.Fill(dt);
                return dt;
            }
        }
    }
}