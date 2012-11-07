using System.Data;
using System.Data.Common;

namespace DatabaseSchemaReader.ProviderSchemaReaders
{
    class SybaseAseSchemaReader : SchemaExtendedReader
    {
        public SybaseAseSchemaReader(string connectionString, string providerName)
            : base(connectionString, providerName)
        {
        }

        //sybase ASE 15 system tables: http://infocenter.sybase.com/help/index.jsp?topic=/com.sybase.infocenter.dc36274.1550/html/tables/tables7.htm

        protected override DataTable PrimaryKeys(string tableName, DbConnection connection)
        {
            //the sproc only operates on table-level, so return empty table for whole database
            if (string.IsNullOrEmpty(tableName)) return CreateDataTable(PrimaryKeysCollectionName);

            const string sql = @"sp_pkeys @tableName";

            return SybaseCommandForTable(connection, PrimaryKeysCollectionName, tableName, sql);
        }

        protected override DataTable ForeignKeys(string tableName, DbConnection connection)
        {
            //the standard provider calls sp_oledb_fkeys
            const string sql = @"select table_schema = user_name (t.uid),  constraint_name = cn.name, table_name = t.name, fk_table = ft.name, column_name = tc.name
from sysreferences r
join sysobjects t on t.id = r.tableid
join sysobjects ft on ft.id = r.reftabid
join sysobjects cn on cn.id = r.constrid
join sysconstraints c on c.constrid = r.constrid
join syscolumns tc on tc.id = r.tableid and tc.colid = c.colid
where (t.name = @tableName OR @tableName IS NULL)
order by t.name, ft.name, c.colid";
            return SybaseCommandForTable(connection, ForeignKeysCollectionName, tableName, sql);
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

                da.Fill(dt);
                return dt;
            }
        }

        protected override DataTable StoredProcedures(DbConnection connection)
        {
            var sprocs = base.StoredProcedures(connection);
            foreach (DataRow row in sprocs.Rows)
            {
                var name = row["ROUTINE_NAME"].ToString();
                row["ROUTINE_NAME"] = name.Trim('\0'); //for some reason they are null terminated
            }

            return sprocs;
        }
    }
}