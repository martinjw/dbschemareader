using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders
{
    class OracleSchemaReader : SchemaExtendedReader
    {

        public OracleSchemaReader(string connectionString, string providerName)
            : base(connectionString, providerName)
        {
        }

        /// <summary>
        /// The database version.
        /// </summary>
        private int? _version;

        /// <summary>
        /// Parse out the server version (9, 10 or 11, hopefully)
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns></returns>
        private int? Version(DbConnection connection)
        {
            if (!_version.HasValue)
            {
                var version = connection.ServerVersion;
                var match = Regex.Match(version, @"\b(\d+)(?=\D)");
                _version = int.Parse(match.Value);
            }
            return _version;
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
            return CommandForTable(tableName, conn, CheckConstraintsCollectionName, sqlCommand);
        }
        private static string GetCheckSql()
        {
            //all_constraints includes NULL constraints. They have generated names- so we exclude them.
            //Hmm, search_condition is an Oracle LONG which we can't read.
            //TO_LOB can only be used on create table as select, xml fails on < in there... 
            const string sqlCommand = @"SELECT 
cons.constraint_name, 
cons.table_name,
cons.search_condition AS Expression
FROM all_constraints cons
 WHERE 
    (cons.table_name = :tableName OR :tableName IS NULL) AND 
    (cons.owner = :schemaOwner OR :schemaOwner IS NULL) AND 
     cons.constraint_type = 'C' AND 
     cons.generated <> 'GENERATED NAME'
ORDER BY cons.table_name, cons.constraint_name";
            return sqlCommand;
        }

        protected override DataTable Columns(string tableName, DbConnection connection)
        {
            //for Oracle, we do our own thing
            var dt = CreateDataTable(ColumnsCollectionName);
            using (DbDataAdapter da = Factory.CreateDataAdapter())
            {
                //this is almost exactly the same sql as the System.Data.OracleClient uses, plus data_default. We use Char_Length (chars) rather than Data_Length (bytes)
                const string sqlCommand = @"SELECT OWNER,
  TABLE_NAME,
  COLUMN_NAME,
  COLUMN_ID      AS ID,
  DATA_TYPE      AS DataType,
  CHAR_LENGTH    AS LENGTH,
  DATA_PRECISION AS PRECISION,
  DATA_SCALE     AS Scale,
  NULLABLE       AS Nullable,
  DATA_DEFAULT   AS Column_default
FROM ALL_TAB_COLUMNS
WHERE 
TABLE_NAME NOT LIKE 'BIN$%'
AND (OWNER     = :OWNER
OR :OWNER       IS NULL)
AND (TABLE_NAME  = :TABLENAME
OR :TABLENAME   IS NULL)
ORDER BY OWNER,
  TABLE_NAME,
  ID";
                using (DbCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText = sqlCommand;

                    EnsureOracleBindByName(cmd);

                    cmd.Parameters.Add(
                        AddDbParameter("OWNER", Owner));
                    cmd.Parameters.Add(
                        AddDbParameter("TABLENAME", tableName));
                    da.SelectCommand = cmd;

                    da.Fill(dt);
                }
            }

            return dt;
        }

        public override DataTable Functions()
        {
            DataTable dt = CreateDataTable(FunctionsCollectionName);

            using (DbConnection conn = Factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                //create a dataadaptor and fill it
                using (DbDataAdapter da = Factory.CreateDataAdapter())
                {
                    const string sqlCommand = @"SELECT OWNER,
  OBJECT_NAME,
  DBMS_METADATA.GET_DDL('FUNCTION', OBJECT_NAME, :OWNER) AS ""SQL""
FROM ALL_OBJECTS
WHERE (OWNER     = :OWNER
OR :OWNER       IS NULL)
AND OBJECT_TYPE  = 'FUNCTION'";

                    da.SelectCommand = conn.CreateCommand();
                    da.SelectCommand.CommandText = sqlCommand;
                    EnsureOracleBindByName(da.SelectCommand);
                    da.SelectCommand.CommandText = sqlCommand;
                    da.SelectCommand.Parameters.Add(
                        AddDbParameter("OWNER", Owner));

                    try
                    {
                        da.Fill(dt);
                    }
                    catch (DbException ex)
                    {
                        System.Diagnostics.Trace.WriteLine("Error reading oracle functions " + ex.Message);
                    }
                    return dt;
                }
            }
        }

        private DataTable FindKeys(string tableName, string constraintType, DbConnection conn)
        {
            DataTable dt = CreateDataTable(constraintType);

            string sqlCommand = GetKeySql();

            //create a dataadaptor and fill it
            using (DbDataAdapter da = Factory.CreateDataAdapter())
            {
                da.SelectCommand = conn.CreateCommand();
                EnsureOracleBindByName(da.SelectCommand);
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
            //Oracle doesn't have INFORMATION_SCHEMA
            const string sqlCommand = @"SELECT cols.constraint_name, 
cols.table_name, 
cols.column_name, 
cols.position AS ordinal_position, 
cons.r_constraint_name AS unique_constraint_name, 
cons2.table_name AS fk_table,
cons.delete_rule,
NULL AS update_rule
FROM all_constraints cons
INNER JOIN all_cons_columns cols 
  ON cons.constraint_name = cols.constraint_name
  AND cons.owner = cols.owner
LEFT OUTER JOIN all_constraints cons2 
  ON cons.r_constraint_name = cons2.constraint_name
  AND cons.owner = cons2.owner
WHERE 
   (cols.table_name = :tableName OR :tableName IS NULL) AND 
   (cols.owner = :schemaOwner OR :schemaOwner IS NULL) AND 
    cons.constraint_type = :constraint_type
ORDER BY cols.table_name, cols.position";
            return sqlCommand;
        }

        public override DataTable IdentityColumns(string tableName)
        {
            return CreateDataTable(IdentityColumnsCollectionName);
        }

        protected override DataTable PrimaryKeys(string tableName, DbConnection connection)
        {
            var dt = FindKeys(tableName, "P", connection);
            dt.TableName = PrimaryKeysCollectionName;
            return dt;
        }
        protected override DataTable ForeignKeys(string tableName, DbConnection connection)
        {
            var dt = FindKeys(tableName, "R", connection);
            dt.TableName = ForeignKeysCollectionName;
            return dt;
        }
        protected override DataTable UniqueKeys(string tableName, DbConnection connection)
        {
            var dt = FindKeys(tableName, "U", connection);
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
                    //if you don't have security to view source, you get no rows back
                    const string sqlCommand = @"SELECT 
OWNER, 
NAME, 
TYPE, 
LINE, 
TEXT
FROM ALL_SOURCE 
WHERE 
OWNER = :schemaOwner AND    
TYPE IN ('PROCEDURE', 'FUNCTION', 'PACKAGE', 'PACKAGE BODY') AND 
(NAME = :name OR :name IS NULL)
ORDER BY OWNER, NAME, TYPE, LINE";

                    da.SelectCommand = conn.CreateCommand();
                    da.SelectCommand.CommandText = sqlCommand;
                    EnsureOracleBindByName(da.SelectCommand);
                    da.SelectCommand.Parameters.Add(
                        AddDbParameter("schemaOwner", Owner));
                    da.SelectCommand.Parameters.Add(
                        AddDbParameter("name", name));
                    da.Fill(dt);

                    return dt;
                }
            }
        }

        protected override DataTable ComputedColumns(string tableName, DbConnection connection)
        {
            if (Version(connection) < 11)
            {
                //only supported in 11g+
                return base.ComputedColumns(tableName);
            }
            const string sqlCommand = @"SELECT 
OWNER,
TABLE_NAME, 
COLUMN_NAME, 
DATA_DEFAULT AS COMPUTEDDEFINITION 
FROM all_tab_cols
WHERE 
VIRTUAL_COLUMN = 'YES' AND
(TABLE_NAME = :tableName OR :tableName IS NULL) AND 
(OWNER = :schemaOwner OR :schemaOwner IS NULL) 
ORDER BY TABLE_NAME, COLUMN_NAME";

            return CommandForTable(tableName, connection, ComputedColumnsCollectionName, sqlCommand);
        }


        protected override DataTable Triggers(string tableName, DbConnection conn)
        {
            const string sqlCommand = @"SELECT OWNER,
  TRIGGER_NAME,
  TABLE_NAME,
  TRIGGER_BODY,
  TRIGGERING_EVENT,
  TRIGGER_TYPE
FROM ALL_TRIGGERS
WHERE STATUS = 'ENABLED' AND 
(TABLE_NAME = :tableName OR :tableName IS NULL) AND 
(OWNER = :schemaOwner OR :schemaOwner IS NULL) AND 
TRIGGER_NAME NOT IN ( SELECT object_name FROM USER_RECYCLEBIN )";

            return CommandForTable(tableName, conn, TriggersCollectionName, sqlCommand);
        }


        protected override DataTable CommandForTable(string tableName, DbConnection conn, string collectionName, string sqlCommand)
        {
            DataTable dt = CreateDataTable(collectionName);

            //create a dataadaptor and fill it
            using (DbDataAdapter da = Factory.CreateDataAdapter())
            {
                da.SelectCommand = conn.CreateCommand();
                EnsureOracleBindByName(da.SelectCommand);
                da.SelectCommand.CommandText = sqlCommand;
                AddTableNameSchemaParameters(da.SelectCommand, tableName);

                da.Fill(dt);
                return dt;
            }
        }

        private static void EnsureOracleBindByName(DbCommand cmd)
        {
            //Oracle.DataAccess.Client only binds first parameter match unless BindByName=true
            //so we violate LiskovSP (in reflection to avoid dependency on ODP)
            if (cmd.GetType().GetProperty("BindByName") != null)
            {
                cmd.GetType().GetProperty("BindByName").SetValue(cmd, true, null);
            }
        }

        public override void PostProcessing(DataSchema.DatabaseTable databaseTable)
        {
            if (databaseTable == null) return;
            //look at Oracle triggers which suggest the primary key id is autogenerated (in SQLServer terms, Identity)
            LookForAutoGeneratedId(databaseTable);
        }

        private static void LookForAutoGeneratedId(DatabaseTable databaseTable)
        {
            var pk = databaseTable.PrimaryKeyColumn;
            if (pk == null) return;
            var triggers = databaseTable.Triggers;
            if (triggers.Count == 0) return;
            //the trigger body will look something like "SELECT MYSEQ.NEXTVAL INTO :NEW.ID FROM DUAL;"
            var pattern = ".NEXTVAL\\s+?INTO\\s+?:NEW.\"?" + pk.Name;
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            foreach (var databaseTrigger in triggers)
            {
                var body = databaseTrigger.TriggerBody;
                if (regex.IsMatch(body))
                {
                    pk.IsIdentity = true;
                    return;
                }
            }
        }
    }
}