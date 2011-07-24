using System;
using System.Collections.Generic;
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
            const string sql = @"select i.index_name AS constraint_name, 
t.table_name,
c.column_name,
ic.sequence AS ordinal_position
from SYSIDX i 
join SYSIDXCOL ic on i.index_id = ic.index_id AND ic.table_id = i.table_id
join SYSTAB t on i.table_id = t.table_id
join SYSCOLUMN c on (t.table_id = c.table_id and ic.column_id = c.column_id)
join SYSUSER u on t.creator = u.user_id
WHERE index_category = 1 --pk or 2 fk
AND t.table_type = 1 --base
AND (t.table_name = ? OR ? IS NULL)
AND (u.user_name = ? OR ? IS NULL)
AND t.creator <> 0
ORDER BY t.table_name, i.index_name, ic.sequence";

            DataTable dt = CreateDataTable("PrimaryKey");

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


        //protected override DataTable ForeignKeys(string tableName, DbConnection connection)
        //{

        //}
    }
}