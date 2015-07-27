﻿using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders
{
    class SqlServerSchemaReader : SchemaExtendedReader
    {
        public SqlServerSchemaReader(string connectionString, string providerName)
            : base(connectionString, providerName)
        {
        }

        /// <summary>
        /// returns the SqlServer version (10 is SqlServer 2008). 
        /// </summary>
        /// <param name="connection">The connection (must be OPEN).</param>
        /// <returns>9 is SqlServer 2005, 10 is SqlServer 2008, 11 is SqlServer 2012, 12 is SqlServer 2014</returns>
        public int SqlServerVersion(DbConnection connection)
        {
            //an open connection contains a server version
            //SqlServer 2014 = 12.00.2000
            //SqlAzure (as of 201407 it's SqlServer 2012) = 11.0.9216.62
            //SqlServer 2012 SP2 = 11.0.5058.0
            //SqlServer 2008 R2 SP2 = 10.50.4000.0
            //2005 = 9.00.5000.00 , 2000 = 8.00.2039
            int serverVersion;
            var version = connection.ServerVersion;
            if (string.IsNullOrEmpty(version) || !int.TryParse(version.Substring(0, 2), out serverVersion))
            {
                serverVersion = 9; //SqlServer 2005
            }
            return serverVersion;
        }

        public override DataTable CheckConstraints(string tableName)
        {
            //open a connection
            using (DbConnection conn = Factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                return CheckConstraints(tableName, conn);
            }
        }
        protected override DataTable CheckConstraints(string tableName, DbConnection conn)
        {
            string sqlCommand = GetCheckSql();
            var dt = CommandForTable(tableName, conn, "Checks", sqlCommand);
            dt.TableName = CheckConstraintsCollectionName;
            return dt;
        }
        private static string GetCheckSql()
        {
            //information_schema.check_constraints doesn't have table, so we join to table constraints
            const string sqlCommand = @"SELECT 
cons.constraint_name, 
cons.constraint_schema,
cons.table_name, 
cons2.check_clause AS Expression
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS cons
INNER JOIN INFORMATION_SCHEMA.CHECK_CONSTRAINTS AS cons2
 ON cons2.constraint_catalog = cons.constraint_catalog AND
  cons2.constraint_schema = cons.constraint_schema AND
  cons2.constraint_name = cons.constraint_name
WHERE 
    (cons.table_name = @tableName OR @tableName IS NULL) AND 
    (cons.constraint_catalog = @schemaOwner OR @schemaOwner IS NULL) AND 
     cons.constraint_type = 'CHECK'
ORDER BY cons.table_name, cons.constraint_name";
            return sqlCommand;
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
            const string sqlCommand = @"SELECT DISTINCT
cons.constraint_schema,
cons.constraint_name, 
keycolumns.table_name, 
column_name, 
ordinal_position, 
refs.unique_constraint_name, 
cons2.table_name AS fk_table,
cons2.table_schema AS fk_schema,
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
        cons.constraint_name = refs.constraint_name
    LEFT OUTER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS cons2
        ON (cons2.constraint_catalog = refs.constraint_catalog
            OR cons2.constraint_catalog IS NULL) AND
        cons2.constraint_schema = refs.unique_constraint_schema AND
        cons2.constraint_name = refs.unique_constraint_name
WHERE 
    (keycolumns.table_name = @tableName OR @tableName IS NULL) AND 
    (cons.constraint_schema = @schemaOwner OR @schemaOwner IS NULL) AND 
    cons.constraint_type = @constraint_type";
            return sqlCommand;
        }

        protected override DataTable IdentityColumns(string tableName, DbConnection connection)
        {
            const string sqlCommand = @"SELECT 
SchemaOwner = s.name, 
TableName = o.name, 
ColumnName = c.name,
IdentitySeed = seed_value,
IdentityIncrement = increment_value
FROM sys.identity_columns c
INNER JOIN sys.all_objects o ON c.object_id = o.object_id
INNER JOIN sys.schemas s ON s.schema_id = o.schema_id
WHERE 
(o.name = @tableName OR @tableName IS NULL) AND 
(s.name = @schemaOwner OR @schemaOwner IS NULL) AND 
o.type= 'U' 
ORDER BY o.name, c.name";

            return CommandForTable(tableName, connection, IdentityColumnsCollectionName, sqlCommand);
        }

        protected override DataTable ComputedColumns(string tableName, DbConnection connection)
        {
            const string sqlCommand = @"SELECT 
SchemaOwner = s.name, 
TableName = o.name, 
ColumnName = c.name,
ComputedDefinition = c.definition
FROM sys.computed_columns c
INNER JOIN sys.all_objects o ON c.object_id = o.object_id
INNER JOIN sys.schemas s ON s.schema_id = o.schema_id
WHERE 
(o.name = @tableName OR @tableName IS NULL) AND 
(s.name = @schemaOwner OR @schemaOwner IS NULL) AND 
o.type= 'U' 
ORDER BY o.name, c.name";

            return CommandForTable(tableName, connection, ComputedColumnsCollectionName, sqlCommand);
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

        public override DataTable ProcedureSource(string name)
        {
            DataTable dt = CreateDataTable(ProcedureSourceCollectionName);
            using (DbConnection conn = Factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();
                //create a dataadaptor and fill it
                using (DbDataAdapter da = Factory.CreateDataAdapter())
                {
                    //NB: sql_modules in SQLServer 2005+.
                    //sqlServer2000 InformationSchema cuts the source after 4k, so use sq_help
                    //http://msdn.microsoft.com/en-us/library/ms178618.aspx type is sproc, function or CLR procedure
                    const string sqlCommand = @"SELECT
    OBJECT_SCHEMA_NAME(o.object_id) AS ""OWNER"",
    OBJECT_NAME(sm.object_id) AS ""NAME"",
    o.type AS ""TYPE"", 
    sm.definition As ""TEXT""
FROM sys.sql_modules AS sm
    JOIN sys.objects AS o 
        ON sm.object_id = o.object_id
WHERE (o.type = N'P' OR o.type = N'FN' OR o.type = N'TF' OR o.type='PC' OR o.type='V')
    AND (OBJECT_SCHEMA_NAME(o.object_id) = @schemaOwner OR @schemaOwner IS NULL)
    AND (OBJECT_NAME(sm.object_id) = @name OR @name IS NULL)
ORDER BY o.type;";


                    da.SelectCommand = conn.CreateCommand();
                    da.SelectCommand.CommandText = sqlCommand;
                    da.SelectCommand.Parameters.Add(
                        AddDbParameter("schemaOwner", Owner));
                    da.SelectCommand.Parameters.Add(
                        AddDbParameter("name", name));
                    try
                    {
                        da.Fill(dt);
                    }
                    catch (DbException exception)
                    {
                        //1. Security does not allow access
                        //2. OBJECT_SCHEMA_NAME is only available from SQLServer 2005 SP2
                        Trace.TraceError("Handled: " + exception);
                        //continue without the source
                    }

                    return dt;
                }
            }
        }

        protected override DataTable Triggers(string tableName, DbConnection conn)
        {
            //thanks to jamesholwell https://dbschemareader.codeplex.com/workitem/1432
            const string sqlCommand = @"SELECT SCHEMA_NAME(o1.uid) AS 'OWNER',
    o1.NAME AS 'TRIGGER_NAME',
    o2.NAME AS 'TABLE_NAME',
    c.definition AS 'TRIGGER_BODY',
    CASE 
        WHEN OBJECTPROPERTY(o1.id, 'ExecIsInsertTrigger') = 1 THEN 'INSERT'
        WHEN OBJECTPROPERTY(o1.id, 'ExecIsUpdateTrigger') = 1 THEN 'UPDATE'
        WHEN OBJECTPROPERTY(o1.id, 'ExecIsDeleteTrigger') = 1 THEN 'DELETE'
    END 'TRIGGERING_EVENT',
    CASE 
        WHEN OBJECTPROPERTY(o1.id, 'ExecIsInsteadOfTrigger') = 1
            THEN 'INSTEAD OF' ELSE 'AFTER'
        END 'TRIGGER_TYPE'
FROM sysobjects o1
    INNER JOIN sysobjects o2 ON o1.parent_obj = o2.id
    INNER JOIN sys.all_sql_modules c on o1.id = c.object_id
WHERE
    o1.XTYPE = 'TR'
    AND (o2.NAME = @tableName OR @tableName IS NULL)
    AND (SCHEMA_NAME(o1.uid) = @schemaOwner OR @schemaOwner IS NULL)";

            return CommandForTable(tableName, conn, TriggersCollectionName, sqlCommand);
        }

        protected override DataTable Sequences(DbConnection connection)
        {
            //future compatibility- if they support Sequences, use that
            if (SchemaCollectionExists(connection, SequencesCollectionName))
                return base.Sequences(connection);

            //2 steps - check if have any sequences (SqlServer 2012+), then load them
            var dt = CreateDataTable(SequencesCollectionName);
            using (var conn = Factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                var cmd = conn.CreateCommand();
                //step 1- check if there are any sequences (backwards compatible)
                cmd.CommandText = @"SELECT COUNT(*) 
FROM sys.objects 
WHERE type= 'SO' AND
(Schema_name(schema_id) = @schemaOwner OR @schemaOwner IS NULL)";
                cmd.Parameters.Add(
                        AddDbParameter("schemaOwner", Owner));
                var hasSequences = (int)cmd.ExecuteScalar() > 0;
                if (!hasSequences)
                {
                    return dt;
                }
                //step 2- they have them
                //we can use the SqlServer 2012 sys.sequences catalog view
                //renamed for compatibility with Oracle's ALL_SEQUENCES
                using (DbDataAdapter da = Factory.CreateDataAdapter())
                {
                    da.SelectCommand = conn.CreateCommand();
                    da.SelectCommand.CommandText = @"
SELECT Schema_name(schema_id) AS sequence_owner,
       name                   AS sequence_name,
       start_value            AS min_value,
       increment              AS increment_by,
       is_cycling             AS cycle_flag
FROM   sys.sequences
WHERE  
(Schema_name(schema_id) = @schemaOwner OR @schemaOwner IS NULL)";
                    da.SelectCommand.Parameters.Add(
                        AddDbParameter("schemaOwner", Owner));
                    da.Fill(dt);

                    return dt;
                }

            }
        }

        public override DataTable TableDescription(string tableName)
        {
            const string sqlCommand = @"SELECT 
    SchemaOwner = s.name, 
    TableName = o.name, 
    TableDescription = p.value
FROM sysobjects o
INNER JOIN  sys.schemas s
    ON s.schema_id = o.uid
INNER JOIN sys.extended_properties p 
    ON p.major_id = o.id
    AND p.minor_id = 0
    AND p.name = 'MS_Description'
WHERE 
    (o.name = @tableName OR @tableName IS NULL) AND 
    (s.name = @schemaOwner OR @schemaOwner IS NULL) AND 
    o.type= 'U' 
ORDER BY s.name, o.name";

            using (DbConnection connection = Factory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;
                connection.Open();

                return CommandForTable(tableName, connection, TableDescriptionCollectionName, sqlCommand);
            }
        }

        public override DataTable ColumnDescription(string tableName)
        {
            const string sqlCommand = @"SELECT 
    SchemaOwner = s.name, 
    TableName = o.name,
    ColumnName = c.name,
    ColumnDescription = p.value
FROM sysobjects o
INNER JOIN syscolumns c
    ON o.id = c.id
INNER JOIN  sys.schemas s
    ON s.schema_id = o.uid
INNER JOIN sys.extended_properties p 
    ON p.major_id = c.id
    AND	p.minor_id = c.colid
    AND	p.name = 'MS_Description'
WHERE 
    (o.name = @tableName OR @tableName IS NULL) AND 
    (s.name = @schemaOwner OR @schemaOwner IS NULL) AND 
o.type= 'U' 
ORDER BY s.name, o.name";

            using (DbConnection connection = Factory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;
                connection.Open();

                return CommandForTable(tableName, connection, ColumnDescriptionCollectionName, sqlCommand);
            }
        }

        public override DataTable DefaultConstraints(string tableName)
        {
            const string sqlCommand = @"SELECT 
    s.name AS SCHEMA_NAME, 
    o.name AS TABLE_NAME,
    c.name AS COLUMN_NAME,
    d.name AS CONSTRAINT_NAME,
    d.[definition] AS EXPRESSION
FROM sys.[default_constraints] d
INNER JOIN sys.objects o
    ON o.object_id = d.parent_object_id
INNER JOIN sys.columns c
    ON c.default_object_id = d.object_id
INNER JOIN  sys.schemas s
    ON s.schema_id = o.schema_id
WHERE 
    (o.name = @tableName OR @tableName IS NULL) AND 
    (s.name = @schemaOwner OR @schemaOwner IS NULL) AND 
o.type= 'U' 
ORDER BY s.name, o.name";

            using (DbConnection connection = Factory.CreateConnection())
            {
                connection.ConnectionString = ConnectionString;
                connection.Open();

                return CommandForTable(tableName, connection, DefaultConstraintCollectionName, sqlCommand);
            }
        }

        protected override DataTable Indexes(string tableName, DbConnection connection)
        {
            var serverVersion = SqlServerVersion(connection);
            if (serverVersion < 10)
                return base.Indexes(tableName, connection);
            //SqlServer 2008 uses EXEC sys.sp_indexes_managed @Catalog, @Owner, @Table, @Name
            const string sqlCommand = @"select
    s.name AS constraint_schema,
    si.name AS constraint_name,
    st.name AS table_schema,
    t.name AS table_name,
    si.name AS index_name,
    si.type_desc AS type_desc,
    si.is_unique AS isunique,
    si.is_unique_constraint AS is_unique_constraint
from
    sys.indexes si
    INNER JOIN sys.objects o ON o.object_id = si.object_id
    INNER JOIN sys.tables t ON si.object_id = t.object_id 
    INNER JOIN  sys.schemas s  ON s.schema_id = o.schema_id
    INNER JOIN  sys.schemas st  ON st.schema_id = t.schema_id
where
    (st.name = @schemaOwner or (@schemaOwner is null)) and 
    (t.name = @tableName or (@tableName is null))
order by t.name, si.name ";
            return CommandForTable(tableName, connection, IndexesCollectionName, sqlCommand);
        }

        public override void PostProcessing(DatabaseTable databaseTable)
        {
            if (databaseTable == null) return;
            //look at default values to see if uses a sequence
            LookForAutoGeneratedId(databaseTable);
        }

        private static void LookForAutoGeneratedId(DatabaseTable databaseTable)
        {
            var pk = databaseTable.PrimaryKeyColumn;
            if (pk == null) return;
            if (databaseTable.HasAutoNumberColumn) return;
            if (string.IsNullOrEmpty(pk.DefaultValue)) return;
            if (pk.DefaultValue.IndexOf("NEXT VALUE FOR ", StringComparison.OrdinalIgnoreCase) != -1)
                pk.IsAutoNumber = true;
        }
    }
}