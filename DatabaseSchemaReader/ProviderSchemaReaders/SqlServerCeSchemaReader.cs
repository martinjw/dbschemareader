using System.Data;
using System.Data.Common;

namespace DatabaseSchemaReader.ProviderSchemaReaders
{
    class SqlServerCeSchemaReader : SchemaExtendedReader
    {
        public SqlServerCeSchemaReader(string connectionString, string providerName)
            : base(connectionString, providerName)
        {
        }

        protected override DataTable IdentityColumns(string tableName, DbConnection conn)
        {
            const string sqlCommand = @"SELECT 
    NULL SchemaOwner, TABLE_NAME TableName, COLUMN_NAME ColumnName 
FROM 
    INFORMATION_SCHEMA.COLUMNS 
WHERE 
    (@tableName IS NULL OR TABLE_NAME = @tableName) AND 
    (@schemaOwner IS NOT NULL OR @schemaOwner IS NULL) AND 
    AUTOINC_NEXT IS NOT NULL";

            return CommandForTable(tableName, conn, IdentityColumnsCollectionName, sqlCommand);
        }

        private DataTable FindKeys(string tableName, string constraintType, DbConnection conn)
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
            const string sqlCommand =
                @"SELECT
    KEYCOLUMNS.CONSTRAINT_NAME, 
    KEYCOLUMNS.TABLE_NAME, 
    KEYCOLUMNS.COLUMN_NAME, 
    KEYCOLUMNS.ORDINAL_POSITION,
    REFS.UNIQUE_CONSTRAINT_NAME, 
    REFS.UNIQUE_CONSTRAINT_TABLE_NAME AS FK_TABLE,
    REFS.DELETE_RULE,
    REFS.UPDATE_RULE
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS CONS
    INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS KEYCOLUMNS
     ON CONS.CONSTRAINT_NAME = KEYCOLUMNS.CONSTRAINT_NAME
    LEFT OUTER JOIN INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS AS REFS
     ON CONS.CONSTRAINT_NAME = REFS.CONSTRAINT_NAME
WHERE 
    (CONS.TABLE_NAME = @tableName OR @tableName IS NULL) AND 
    (@schemaOwner IS NOT NULL OR @schemaOwner IS NULL) AND 
    CONS.CONSTRAINT_TYPE = @constraint_type";
            return sqlCommand;
        }

        protected override DataTable Triggers(string tableName, DbConnection conn)
        {
            //no triggers in CE
            return CreateDataTable(TriggersCollectionName);
        }

        protected override DataTable PrimaryKeys(string tableName, DbConnection connection)
        {
            DataTable dt = FindKeys(tableName, GetPrimaryKeyType(), connection);
            dt.TableName = PrimaryKeysCollectionName;
            return dt;
        }
        protected override DataTable ForeignKeys(string tableName, DbConnection connection)
        {
            DataTable dt = FindKeys(tableName, GetForeignKeyType(), connection);
            dt.TableName = ForeignKeysCollectionName;
            return dt;
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
            return CreateDataTable(ForeignKeyColumnsCollectionName);
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
    }
}