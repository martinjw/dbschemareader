using System.Data;
using System.Data.Common;

namespace DatabaseSchemaReader.ProviderSchemaReaders
{
    class MySqlSchemaReader : SchemaExtendedReader
    {
        public MySqlSchemaReader(string connectionString, string providerName)
            : base(connectionString, providerName)
        {
        }

        /*
         * MySQL limitations - parameter arguments
         * 
         * MySql v5 has a problem with parameter arguments (which it parses from the string in SHOW CREATE).
         * Devart.Data.MySql doesn't return any parameter arguments (!)
         * MySql v6 seems okay (it uses INFORMATION_SCHEMA.PARAMETERS)
         * 
         * It's possible to grab out the parameter arguments and return type in select param_list, returns from mysql.proc
         * But there's just blobs of text with comments included - it needs to be parsed properly
         * 
         */

        protected DataTable FindKeys(string tableName, string constraintType, DbConnection conn)
        {
            DataTable dt = CreateDataTable(constraintType);

            string sqlCommand = GetKeySql();

            //create a dataadaptor and fill it
            using (DbDataAdapter da = Factory.CreateDataAdapter())
            {
                da.SelectCommand = conn.CreateCommand();
                da.SelectCommand.CommandText = sqlCommand;
                AddTableNameSchemaParameters(da.SelectCommand, tableName);

                DbParameter type = Factory.CreateParameter();
                type.ParameterName = "constraint_type";
                type.DbType = DbType.String;
                type.Value = constraintType;
                da.SelectCommand.Parameters.Add(type);

                da.Fill(dt);
                return dt;
            }
        }
        private static string GetKeySql()
        {
            //in MySQL, different constraints for different tables can have the same name (eg Primary)
            const string sqlCommand =
                @"SELECT DISTINCT
cons.constraint_name, 
keycolumns.table_name, 
column_name, 
ordinal_position, 
refs.unique_constraint_name, 
cons2.table_name AS fk_table,
refs.delete_rule AS delete_rule,
refs.update_rule AS update_rule
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS cons
    INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS keycolumns
        ON (cons.constraint_catalog = keycolumns.constraint_catalog
            OR cons.constraint_catalog IS NULL) AND
        cons.constraint_schema = keycolumns.constraint_schema AND
        cons.constraint_name = keycolumns.constraint_name AND
        cons.table_name = keycolumns.table_name
    LEFT OUTER JOIN INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS AS refs
        ON (cons.constraint_catalog = refs.constraint_catalog
            OR cons.constraint_catalog IS NULL) AND
        cons.constraint_schema = refs.constraint_schema AND
        cons.constraint_name = refs.constraint_name AND
        cons.table_name = refs.table_name
    LEFT OUTER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS cons2
        ON (cons2.constraint_catalog = refs.constraint_catalog
            OR cons2.constraint_catalog IS NULL) AND
        cons2.constraint_schema = refs.constraint_schema AND
        cons2.constraint_name = refs.unique_constraint_name AND
        cons2.table_name = refs.referenced_table_name
WHERE 
    (keycolumns.table_name = @tableName OR @tableName IS NULL) AND 
    (cons.constraint_schema = @schemaOwner OR @schemaOwner IS NULL) AND 
    cons.constraint_type = @constraint_type";
            return sqlCommand;
        }

        protected override DataTable IdentityColumns(string tableName, DbConnection conn)
        {
            const string sqlCommand = @"SELECT 
TABLE_SCHEMA AS 'SchemaOwner', 
TABLE_NAME AS 'TableName', 
COLUMN_NAME AS 'ColumnName'
FROM information_schema.columns
WHERE EXTRA = 'auto_increment' AND
(TABLE_NAME = @tableName OR @tableName IS NULL) AND 
(TABLE_SCHEMA = @schemaOwner OR @schemaOwner IS NULL)";

            return CommandForTable(tableName, conn, IdentityColumnsCollectionName, sqlCommand);
        }

        protected override DataTable PrimaryKeys(string tableName, DbConnection connection)
        {
            return FindKeys(tableName, GetPrimaryKeyType(), connection);
        }

        protected override DataTable ForeignKeys(string tableName, DbConnection connection)
        {
            return FindKeys(tableName, GetForeignKeyType(), connection);
        }
        protected override DataTable UniqueKeys(string tableName, DbConnection connection)
        {
            DataTable dt = FindKeys(tableName, GetUniqueKeyType(), connection);
            dt.TableName = UniqueKeysCollectionName;
            return dt;
        }
        public override DataTable ForeignKeyColumns(string tableName)
        {
            //we return this in ForeignKeys
            return CreateDataTable("ForeignKeyColumns");
        }
        private static string GetPrimaryKeyType()
        {
            return "PRIMARY KEY";
        }

        private static string GetForeignKeyType()
        {
            return "FOREIGN KEY";
        }
        private static string GetUniqueKeyType()
        {
            return "UNIQUE";
        }

        protected override DataTable Triggers(string tableName, DbConnection conn)
        {
            const string sqlCommand = @"SELECT 
  TRIGGER_SCHEMA AS 'OWNER',
  TRIGGER_NAME,
  EVENT_OBJECT_TABLE AS 'TABLE_NAME',
  ACTION_STATEMENT AS 'TRIGGER_BODY',
  EVENT_MANIPULATION AS 'TRIGGERING_EVENT',
  ACTION_TIMING AS 'TRIGGER_TYPE'
FROM information_schema.Triggers
WHERE 
(EVENT_OBJECT_TABLE = @tableName OR @tableName IS NULL) AND 
(TRIGGER_SCHEMA = @schemaOwner OR @schemaOwner IS NULL)";

            return CommandForTable(tableName, conn, TriggersCollectionName, sqlCommand);
        }
    }
}