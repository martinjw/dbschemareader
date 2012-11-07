using System.Data;
using System.Data.Common;

namespace DatabaseSchemaReader.ProviderSchemaReaders
{
    class SybaseAsaSchemaReader : SchemaExtendedReader
    {
        public SybaseAsaSchemaReader(string connectionString, string providerName)
            : base(connectionString, providerName)
        {
        }

        //sybase ASA 12 system views: http://dcx.sybase.com/1201/en/dbreference/rf-system-views.html
        //the only GetSchema constraint is foreign keys, and that without a fk name.
        //we should be able to get pks as well
        //SYSIDX and SYSIDXCOL contain indexes 
        //
        //apparently ASA does not have named parameters.

        protected override DataTable PrimaryKeys(string tableName, DbConnection connection)
        {
            const string sql = @"SELECT 
i.index_name AS constraint_name, 
t.table_name,
c.column_name,
ic.sequence AS ordinal_position
FROM SYSIDX i 
JOIN SYSIDXCOL ic ON i.index_id = ic.index_id AND ic.table_id = i.table_id
JOIN SYSTAB t ON i.table_id = t.table_id
JOIN SYSCOLUMN c ON (t.table_id = c.table_id and ic.column_id = c.column_id)
JOIN SYSUSER u ON t.creator = u.user_id
WHERE i.index_category = 1 --pk or 2 fk
AND t.table_type = 1 --base
AND (t.table_name = ? OR ? IS NULL)
AND (u.user_name = ? OR ? IS NULL)
AND t.creator <> 0
ORDER BY t.table_name, i.index_name, ic.sequence";

            return SybaseCommandForTable(connection, PrimaryKeysCollectionName, tableName, sql);
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
FROM SYSIDX i 
JOIN SYSFKEY fk ON i.index_id = fk.foreign_index_id AND 
    fk.foreign_table_id = i.table_id
JOIN SYSIDXCOL ic ON i.index_id = ic.index_id AND ic.table_id = i.table_id
JOIN SYSTAB t ON i.table_id = t.table_id
JOIN SYSTAB fkt ON fkt.table_id = fk.primary_table_id
JOIN SYSIDX fki ON fk.primary_index_id = fki.index_id AND
    fk.primary_table_id = fki.table_id
JOIN SYSCOLUMN c ON (t.table_id = c.table_id and ic.column_id = c.column_id)
JOIN SYSUSER u ON t.creator = u.user_id
WHERE i.index_category = 2
AND t.table_type = 1 --base
AND (t.table_name = ? OR ? IS NULL)
AND (u.user_name = ? OR ? IS NULL)
AND t.creator <> 0
ORDER BY t.table_name, i.index_name, ic.sequence";

            return SybaseCommandForTable(connection, ForeignKeysCollectionName, tableName, sql);
        }

        protected override DataTable CheckConstraints(string tableName, DbConnection connection)
        {
            const string sql = @"SELECT 
con.constraint_name AS constraint_name, 
t.table_name,
chk.check_defn AS Expression
FROM SYSCONSTRAINT con 
JOIN SYSCHECK chk ON chk.check_id = con.constraint_id
JOIN SYSTAB t ON t.table_id = con.table_object_id
JOIN SYSUSER u ON t.creator = u.user_id
WHERE con.constraint_type = 'C'
AND t.table_type = 1 --base
AND (t.table_name = ? OR ? IS NULL)
AND (u.user_name = ? OR ? IS NULL)
AND t.creator <> 0
ORDER BY t.table_name, con.constraint_name";

            return SybaseCommandForTable(connection, CheckConstraintsCollectionName, tableName, sql);
        }

        protected override DataTable UniqueKeys(string tableName, DbConnection connection)
        {
            const string sql = @"SELECT 
con.constraint_name AS constraint_name, 
t.table_name,
c.column_name
FROM SYSCONSTRAINT con 
JOIN SYSIDX i ON i.object_id = con.constraint_id
JOIN SYSIDXCOL ic ON i.index_id = ic.index_id AND ic.table_id = i.table_id
JOIN SYSTAB t ON t.table_id = con.table_object_id
JOIN SYSCOLUMN c ON (t.table_id = c.table_id and ic.column_id = c.column_id)
JOIN SYSUSER u ON t.creator = u.user_id
WHERE con.constraint_type = 'U'
AND t.table_type = 1 --base
AND (t.table_name = ? OR ? IS NULL)
AND (u.user_name = ? OR ? IS NULL)
AND t.creator <> 0
ORDER BY t.table_name, con.constraint_name";

            return SybaseCommandForTable(connection, UniqueKeysCollectionName, tableName, sql);
        }

        private DataTable SybaseCommandForTable(DbConnection connection, string dataTableName, string tableName, string sql)
        {
            DataTable dt = CreateDataTable(dataTableName);

            //create a dataadaptor and fill it
            using (DbDataAdapter da = Factory.CreateDataAdapter())
            {
                da.SelectCommand = connection.CreateCommand();
                da.SelectCommand.CommandText = sql;

                var parameter = AddDbParameter("tableName", tableName);
                parameter.DbType = DbType.String;
                da.SelectCommand.Parameters.Add(parameter);

                var parameter2 = AddDbParameter("tableName2", tableName);
                parameter2.DbType = DbType.String;
                da.SelectCommand.Parameters.Add(parameter2);

                var schemaParameter = AddDbParameter("schemaOwner", Owner);
                schemaParameter.DbType = DbType.String;
                da.SelectCommand.Parameters.Add(schemaParameter);

                var schemaParameter2 = AddDbParameter("schemaOwner2", Owner);
                schemaParameter2.DbType = DbType.String;
                da.SelectCommand.Parameters.Add(schemaParameter2);

                da.Fill(dt);
                return dt;
            }
        }
    }
}