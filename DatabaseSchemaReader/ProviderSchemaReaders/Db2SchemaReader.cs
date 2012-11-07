using System.Data;
using System.Data.Common;

namespace DatabaseSchemaReader.ProviderSchemaReaders
{
    class Db2SchemaReader : SchemaExtendedReader
    {
        public Db2SchemaReader(string connectionString, string providerName)
            : base(connectionString, providerName)
        {
        }

        protected override DataTable Sequences(DbConnection connection)
        {
            DataTable dt = CreateDataTable(SequencesCollectionName);

            const string sqlCommand = @"SELECT seqschema AS SCHEMA, seqname AS SEQUENCE_NAME, increment AS INCREMENTBY, minvalue, maxvalue 
FROM sysibm.syssequences 
WHERE seqschema <> 'SYSIBM' AND seqtype = 'S'";

            //create a dataadaptor and fill it
            using (DbDataAdapter da = Factory.CreateDataAdapter())
            {
                da.SelectCommand = connection.CreateCommand();
                da.SelectCommand.CommandText = sqlCommand;

                da.Fill(dt);
                return dt;
            }
        }

        protected override DataTable IdentityColumns(string tableName, DbConnection connection)
        {
            DataTable dt = CreateDataTable(IdentityColumnsCollectionName);
            const string sqlCommand = @"SELECT tabschema, tabname As TableName, colname As ColumnName
FROM syscat.colidentattributes
WHERE tabname = @tableName or @tableName Is NULL
AND tabschema = @schemaOwner or @schemaOwner Is NULL";

            //create a dataadaptor and fill it
            using (DbDataAdapter da = Factory.CreateDataAdapter())
            {
                da.SelectCommand = connection.CreateCommand();
                da.SelectCommand.CommandText = sqlCommand;
                AddTableNameSchemaParameters(da.SelectCommand, tableName);

                da.Fill(dt);
                return dt;
            }
        }

        protected override DataTable Triggers(string tableName, DbConnection conn)
        {
            const string sqlCommand = @"select tabschema as Owner, 
trigname as Trigger_Name, 
tabname as table_name, 
CASE trigevent 
WHEN 'I' THEN 'INSERT'
WHEN 'D' THEN 'DELETE'
WHEN 'U' THEN 'UPDATE'
END as TRIGGERING_EVENT,
CASE trigtime
WHEN 'A' THEN 'AFTER'
WHEN 'B' THEN 'BEFORE'
WHEN 'I' THEN 'INSTEAD OF'
END as TRIGGER_TYPE,
text as TRIGGER_BODY
from syscat.triggers
where tabschema <> 'SYSTOOLS'
AND valid= 'Y'
AND (tabname = @tableName OR @tableName IS NULL) 
AND (tabschema = @schemaOwner OR @schemaOwner IS NULL)";

            return CommandForTable(tableName, conn, TriggersCollectionName, sqlCommand);
        }

    }
}