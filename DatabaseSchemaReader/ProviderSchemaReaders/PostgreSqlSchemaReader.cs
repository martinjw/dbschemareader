using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen.PostgreSql;

namespace DatabaseSchemaReader.ProviderSchemaReaders
{
    class PostgreSqlSchemaReader : SchemaExtendedReader
    {
        public PostgreSqlSchemaReader(string connectionString, string providerName)
            : base(connectionString, providerName)
        {
        }

        protected override DataTable PrimaryKeys(string tableName, DbConnection connection)
        {
            var dt = FindKeys(tableName, "PRIMARY KEY", connection);
            dt.TableName = PrimaryKeysCollectionName;
            return dt;
        }
        protected override DataTable ForeignKeys(string tableName, DbConnection connection)
        {
            var dt = FindKeys(tableName, "FOREIGN KEY", connection);
            dt.TableName = ForeignKeysCollectionName;
            return dt;
        }
        protected override DataTable UniqueKeys(string tableName, DbConnection connection)
        {
            var dt = FindKeys(tableName, "UNIQUE", connection);
            dt.TableName = UniqueKeysCollectionName;
            return dt;
        }
        protected override DataTable CheckConstraints(string tableName, DbConnection connection)
        {
            string sqlCommand = GetCheckSql();
            return CommandForTable(tableName, connection, CheckConstraintsCollectionName, sqlCommand);
        }
        private static string GetCheckSql()
        {
            //information_schema.check_constraints doesn't have table, so we join to table constraints
            const string sqlCommand = @"SELECT 
cons.constraint_name, 
cons.table_name, 
cons2.check_clause AS Expression
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS cons
INNER JOIN INFORMATION_SCHEMA.CHECK_CONSTRAINTS AS cons2
 ON cons2.constraint_catalog = cons.constraint_catalog AND
  cons2.constraint_schema = cons.constraint_schema AND
  cons2.constraint_name = cons.constraint_name
WHERE 
    (cons.table_name = :tableName OR :tableName IS NULL) AND 
    (cons.constraint_catalog = :schemaOwner OR :schemaOwner IS NULL) AND 
     cons.constraint_type = 'CHECK'
ORDER BY cons.table_name, cons.constraint_name";
            return sqlCommand;
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

                DbParameter type = da.SelectCommand.CreateParameter();
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
        cons.constraint_name = refs.constraint_name
    LEFT OUTER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS cons2
        ON (cons2.constraint_catalog = refs.constraint_catalog
            OR cons2.constraint_catalog IS NULL) AND
        cons2.constraint_schema = refs.constraint_schema AND
        cons2.constraint_name = refs.unique_constraint_name
WHERE 
    (keycolumns.table_name = :tableName OR :tableName IS NULL) AND 
    (cons.constraint_schema = :schemaOwner OR :schemaOwner IS NULL) AND 
    cons.constraint_type = :constraint_type";
            return sqlCommand;
        }

        protected override DataTable Triggers(string tableName, DbConnection conn)
        {
            const string sqlCommand = @"SELECT 
  TRIGGER_SCHEMA AS OWNER,
  TRIGGER_NAME,
  EVENT_OBJECT_TABLE AS TABLE_NAME,
  ACTION_STATEMENT AS TRIGGER_BODY,
  EVENT_MANIPULATION AS TRIGGERING_EVENT,
  CONDITION_TIMING AS TRIGGER_TYPE
FROM information_schema.Triggers
WHERE 
(EVENT_OBJECT_TABLE = :tableName OR :tableName IS NULL) AND 
(TRIGGER_SCHEMA = :schemaOwner OR :schemaOwner IS NULL)";

            return CommandForTable(tableName, conn, TriggersCollectionName, sqlCommand);
        }

        protected override DataTable Functions(DbConnection connection)
        {
            string collectionName = FunctionsCollectionName;
            if (SchemaCollectionExists(connection, collectionName))
            {
                return base.Functions(connection);
            }
            //Npgsql doesn't have a functions collection, so this is a simple substitute
            //based on http://www.alberton.info/postgresql_meta_info.html 
            var dt = CreateDataTable(collectionName);
            const string sqlCommand = @"SELECT 
ns.nspname AS SCHEMA, 
pr.proname AS NAME, 
tp.typname AS RETURNTYPE, 
lng.lanname AS LANGUAGE,
pr.prosrc AS BODY
  FROM pg_proc pr
LEFT OUTER JOIN pg_type tp ON tp.oid = pr.prorettype
INNER JOIN pg_namespace ns ON pr.pronamespace = ns.oid
INNER JOIN pg_language lng ON lng.oid = pr.prolang
 WHERE pr.proisagg = FALSE
  AND tp.typname <> 'trigger'
  AND ns.nspname NOT LIKE 'pg_%'
  AND ns.nspname != 'information_schema'
  AND (ns.nspname = :schemaOwner OR :schemaOwner IS NULL)
 ORDER BY pr.proname";
            using (DbDataAdapter da = Factory.CreateDataAdapter())
            {
                da.SelectCommand = connection.CreateCommand();
                da.SelectCommand.CommandText = sqlCommand;
                var schemaParameter = AddDbParameter("schemaOwner", Owner);
                schemaParameter.DbType = DbType.String;
                da.SelectCommand.Parameters.Add(schemaParameter);

                da.Fill(dt);
                return dt;
            }
        }

        protected override  DataTable StoredProcedureArguments(string storedProcedureName, DbConnection connection)
        {
            var argReader = new PostgreSqlArgumentReader(Factory, Owner);
            var dt = argReader.StoredProcedureArguments(storedProcedureName, connection);
            dt.TableName = ProcedureParametersCollectionName;
            return dt;
        }

        public override void PostProcessing(DatabaseTable databaseTable)
        {
            if (databaseTable == null) return;
            //the devart providers GetSchema are a little weird so we fix them up here
            var typeWriter = new DataTypeWriter();

            foreach (var databaseColumn in databaseTable.Columns)
            {
                var santizedType = typeWriter.WriteDataType(databaseColumn);
                //all the different native types are reworked
                if ((santizedType.StartsWith("VARCHAR", StringComparison.OrdinalIgnoreCase)
                    || santizedType.StartsWith("CHAR", StringComparison.OrdinalIgnoreCase)))
                {
                    if (databaseColumn.Length == -1 && databaseColumn.Precision > 0)
                    {
                        databaseColumn.Length = databaseColumn.Precision;
                        databaseColumn.Precision = -1;
                    }
                }
                if ((santizedType.StartsWith("NUMERIC", StringComparison.OrdinalIgnoreCase)
                     || santizedType.StartsWith("DECIMAL", StringComparison.OrdinalIgnoreCase)
                     || santizedType.StartsWith("INTEGER", StringComparison.OrdinalIgnoreCase)))
                {
                    if (databaseColumn.Length > 0 && databaseColumn.Precision == -1)
                    {
                        databaseColumn.Precision = databaseColumn.Length;
                        databaseColumn.Length = -1;
                    }
                }
                //if it's a varchar or char, and the length is -1 but the precision is positive, swap them
                //and vice versa for numerics.

                var defaultValue = databaseColumn.DefaultValue;
                if (!string.IsNullOrEmpty(defaultValue) && defaultValue.StartsWith("nextval('", StringComparison.OrdinalIgnoreCase))
                {
                    databaseColumn.IsIdentity = true;
                    databaseColumn.IsPrimaryKey = true;
                }
                //if defaultValue looks like the nextval from a sequence, it's a pk
                //change the type to serial (or bigserial), ensure it's the primary key
            }
        }

        public override List<DataType> SchemaDataTypes()
        {
            var list = new List<DataType>();
            list.Add(new DataType("bigint", typeof(long).FullName));
            list.Add(new DataType("bigserial", typeof(long).FullName));
            list.Add(new DataType("binary", typeof(Byte[]).FullName));
            list.Add(new DataType("bit varying", typeof(long).FullName));
            list.Add(new DataType("bit", typeof(long).FullName));
            list.Add(new DataType("bool", typeof(bool).FullName));
            list.Add(new DataType("boolean", typeof(bool).FullName));
            list.Add(new DataType("bytea", typeof(Byte[]).FullName));
            list.Add(new DataType("char", typeof(string).FullName));
            list.Add(new DataType("character varying", typeof(string).FullName));
            list.Add(new DataType("character", typeof(string).FullName));
            list.Add(new DataType("date", typeof(DateTime).FullName));
            list.Add(new DataType("dec", typeof(decimal).FullName));
            list.Add(new DataType("decimal", typeof(decimal).FullName));
            list.Add(new DataType("double precision", typeof(double).FullName));
            list.Add(new DataType("double", typeof(double).FullName));
            list.Add(new DataType("float", typeof(Single).FullName));
            list.Add(new DataType("float4", typeof(Single).FullName));
            list.Add(new DataType("int", typeof(int).FullName));
            list.Add(new DataType("int4", typeof(int).FullName));
            list.Add(new DataType("int8", typeof(long).FullName));
            list.Add(new DataType("integer", typeof(int).FullName));
            list.Add(new DataType("interval", typeof(TimeSpan).FullName));
            list.Add(new DataType("line", typeof(string).FullName));
            list.Add(new DataType("money", typeof(double).FullName));
            list.Add(new DataType("numeric", typeof(decimal).FullName));
            list.Add(new DataType("real", typeof(Single).FullName));
            list.Add(new DataType("serial", typeof(int).FullName));
            list.Add(new DataType("serial4", typeof(int).FullName));
            list.Add(new DataType("serial8", typeof(long).FullName));
            list.Add(new DataType("smallint", typeof(short).FullName));
            list.Add(new DataType("text", typeof(string).FullName));
            list.Add(new DataType("time", typeof(TimeSpan).FullName));
            list.Add(new DataType("timestamp", typeof(DateTime).FullName));
            list.Add(new DataType("timestamptz", typeof(DateTime).FullName));
            list.Add(new DataType("timetz", typeof(TimeSpan).FullName));
            list.Add(new DataType("varbit", typeof(long).FullName));
            list.Add(new DataType("varchar", typeof(string).FullName));
            return list;
        }

    }
}