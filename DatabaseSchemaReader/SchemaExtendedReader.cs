using System;
using System.Data;
using System.Data.Common;
using System.Globalization;
using DatabaseSchemaReader.Conversion;


namespace DatabaseSchemaReader
{
    /// <summary>
    /// Extended schema information beyond that included in GetSchema.
    /// </summary>
    internal class SchemaExtendedReader : SchemaReader
    {
        /// <summary>
        /// Constructor with connectionString and ProviderName
        /// </summary>
        /// <param name="connectionString">Eg "Data Source=localhost;Integrated Security=SSPI;Initial Catalog=Northwind;"</param>
        /// <param name="providerName">ProviderInvariantName for the provider (eg System.Data.SqlClient or System.Data.OracleClient)</param>
        public SchemaExtendedReader(string connectionString, string providerName)
            : base(connectionString, providerName)
        {
        }

        /// <summary>
        /// Gets a value indicating whether this provider is SQL server.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is SQL server; otherwise, <c>false</c>.
        /// </value>
        internal bool IsSqlServer
        {
            get
            {
                //SqlClient
                return (ProviderName.Equals("System.Data.SqlClient", StringComparison.OrdinalIgnoreCase));
            }
        }
        /// <summary>
        /// Gets a value indicating whether this instance is SQL Server CE 4.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is SQL Server CE 4; otherwise, <c>false</c>.
        /// </value>
        internal bool IsSqlServerCe4
        {
            get
            {
                //System.Data.SqlServerCe.4.0
                return (ProviderName.Equals("System.Data.SqlServerCe.4.0", StringComparison.OrdinalIgnoreCase));
            }
        }

        /// <summary>
        /// Get all data for a specified table name.
        /// </summary>
        /// <param name="tableName">Name of the table. Oracle names can be case sensitive.</param>
        /// <returns>A dataset containing the tables: Columns, Primary_Keys, Foreign_Keys, Unique_Keys (only filled for Oracle), Indexes, IndexColumns, Triggers</returns>
        public override DataSet Table(string tableName)
        {
            if (!IsSqlServer && !IsSqlServerCe4 && !IsOracle && !IsMySql)
                return base.Table(tableName);
            //more information from sqlserver, oracle and mysql
            var ds = new DataSet();
            using (DbConnection conn = Factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                //uses friend access to schemaReader
                LoadTable(tableName, ds, conn);
                if (ds.Tables.Count == 0) return null; //no data found
                if (string.IsNullOrEmpty(Owner))
                {
                    //we need schema for constraint look ups
                    Owner = SchemaConverter.FindSchema(ds.Tables["Columns"]);
                }

                DataTable pks = FindKeys(tableName, GetPrimaryKeyType(), conn);
                pks.TableName = "PRIMARY_KEYS";
                ds.Tables.Add(pks);

                DataTable fks = FindKeys(tableName, GetForeignKeyType(), conn);
                fks.TableName = "FOREIGN_KEYS";
                ds.Tables.Add(fks);

                ds.Tables.Add(ForeignKeyColumns(tableName));

                DataTable uks = FindKeys(tableName, GetUniqueKeyType(), conn);
                uks.TableName = "UNIQUE_KEYS";
                ds.Tables.Add(uks);

                DataTable cks = FindChecks(tableName, conn);
                ds.Tables.Add(cks);

                ds.Tables.Add(IdentityColumns(tableName, conn));

            }
            return ds;
        }

        protected override DataTable Columns(string tableName, DbConnection connection)
        {
            if (!IsOracle)
                return base.Columns(tableName, connection);

            //for Oracle, we do our own thing
            var dt = new DataTable("Columns");
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

        public DataTable IdentityColumns(string tableName)
        {
            using (DbConnection conn = Factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                return IdentityColumns(tableName, conn);
            }
        }
        private DataTable IdentityColumns(string tableName, DbConnection conn)
        {
            DataTable dt = new DataTable("IdentityColumns");
            dt.Locale = CultureInfo.InvariantCulture;

            if (!IsSqlServerCe4 && !IsSqlServer && !IsMySql) return dt; //Oracle has sequences instead

            //create a dataadaptor and fill it
            using (DbDataAdapter da = Factory.CreateDataAdapter())
            {
                string sqlCommand;
                if (IsSqlServer) sqlCommand = @"SELECT 
SchemaOwner = s.name, 
TableName = o.name, 
ColumnName = c.name 
FROM sys.identity_columns c
INNER JOIN sys.all_objects o ON c.object_id = o.object_id
INNER JOIN sys.schemas s ON s.schema_id = o.schema_id
WHERE 
(o.name = @tableName OR @tableName IS NULL) AND 
(s.name = @schemaOwner OR @schemaOwner IS NULL) AND 
o.type= 'U' 
ORDER BY o.name, c.name";
                else if (IsSqlServerCe4)
                {
                    sqlCommand = @"SELECT 
    NULL SchemaOwner, TABLE_NAME TableName, COLUMN_NAME ColumnName 
FROM 
    INFORMATION_SCHEMA.COLUMNS 
WHERE 
    (@tableName IS NULL OR TABLE_NAME = @tableName) AND 
    (@schemaOwner IS NOT NULL OR @schemaOwner IS NULL) AND 
    AUTOINC_NEXT IS NOT NULL";
                }
                else
                    //MySql version using information_schema
                    sqlCommand = @"SELECT 
TABLE_SCHEMA AS 'SchemaOwner', 
TABLE_NAME AS 'TableName', 
COLUMN_NAME AS 'ColumnName'
FROM information_schema.columns
WHERE EXTRA = 'auto_increment' AND
(TABLE_NAME = @tableName OR @tableName IS NULL) AND 
(TABLE_SCHEMA = @schemaOwner OR @schemaOwner IS NULL)";

                da.SelectCommand = conn.CreateCommand();
                da.SelectCommand.CommandText = sqlCommand;

                AddTableNameSchemaParameters(da.SelectCommand, tableName);

                da.Fill(dt);
                return dt;
            }
        }


        public DataTable Triggers(string tableName)
        {
            using (DbConnection conn = Factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();

                if (!IsOracle && !IsSqlServer && !IsMySql)
                {
                    return GenericCollection("Triggers", conn, tableName);
                }

                return Triggers(tableName, conn);
            }
        }

        private DataTable Triggers(string tableName, DbConnection conn)
        {
            const string collectionName = "Triggers";
            DataTable dt = new DataTable(collectionName);
            dt.Locale = CultureInfo.InvariantCulture;

            //create a dataadaptor and fill it
            using (DbDataAdapter da = Factory.CreateDataAdapter())
            {
                string sqlCommand;
                if (IsOracle)
                {
                    sqlCommand = @"SELECT OWNER,
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
                }
                else if (IsMySql)
                {
                    sqlCommand = @"SELECT 
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
                }
                else
                {
                    //trigger event could be INSERT OR UPDATE
                    sqlCommand = @"SELECT SCHEMA_NAME(o1.uid) AS 'OWNER', 
o1.NAME AS 'TRIGGER_NAME',
o2.NAME AS 'TABLE_NAME',
c.TEXT AS 'TRIGGER_BODY',
CASE
    WHEN OBJECTPROPERTY(o1.id, 'ExecIsInsertTrigger') = 1 THEN 'INSERT'
    WHEN OBJECTPROPERTY(o1.id, 'ExecIsUpdateTrigger') = 1 THEN 'UPDATE'
    WHEN OBJECTPROPERTY(o1.id, 'ExecIsDeleteTrigger') = 1 THEN 'DELETE'
END 'TRIGGERING_EVENT',
CASE WHEN OBJECTPROPERTY(o1.id, 'ExecIsInsteadOfTrigger') = 1
    THEN 'INSTEAD OF' ELSE 'AFTER'
END 'TRIGGER_TYPE'
FROM sysobjects o1
INNER JOIN sysobjects o2 ON o1.parent_obj = o2.id
INNER JOIN syscomments c ON o1.id = c.id
WHERE o1.XTYPE = 'TR' AND 
(o1.NAME = @tableName OR @tableName IS NULL) AND 
(SCHEMA_NAME(o1.uid) = @schemaOwner OR @schemaOwner IS NULL)";
                }

                da.SelectCommand = conn.CreateCommand();
                EnsureOracleBindByName(da.SelectCommand);
                da.SelectCommand.CommandText = sqlCommand;
                AddTableNameSchemaParameters(da.SelectCommand, tableName);

                da.Fill(dt);
                return dt;
            }
        }

        /// <summary>
        /// Find the functions. In SqlServer, they are mixed in with sprocs.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.Data.Common.DbException">Thrown when there is no security access to read DDL</exception>
        public override DataTable Functions()
        {
            DataTable dt = new DataTable("Functions");
            if (!IsOracle) return dt; //in sql server, functions are in the sprocs collection.

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

        public DataTable ProcedureSource(string name)
        {
            DataTable dt = new DataTable("ProcedureSource");
            DbProviderFactory factory = DbProviderFactories.GetFactory(ProviderName);
            using (DbConnection conn = factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                conn.Open();
                //create a dataadaptor and fill it
                using (DbDataAdapter da = factory.CreateDataAdapter())
                {
                    string sqlCommand = null;
                    if (IsSqlServer)
                    {
                        //NB: sql_modules in SQLServer 2005+.
                        //sqlServer2000 InformationSchema cuts the source after 4k, so use sq_help
                        //http://msdn.microsoft.com/en-us/library/ms178618.aspx type is sproc, function or CLR procedure
                        sqlCommand = @"SELECT
	OBJECT_SCHEMA_NAME(o.object_id) AS ""OWNER"",
	OBJECT_NAME(sm.object_id) AS ""NAME"",
	o.type AS ""TYPE"", 
	sm.definition As ""TEXT""
FROM sys.sql_modules AS sm
	JOIN sys.objects AS o 
		ON sm.object_id = o.object_id
WHERE (o.type = N'P' OR o.type = N'FN' OR o.type='PC' OR o.type='V')
	AND (OBJECT_SCHEMA_NAME(o.object_id) = @schemaOwner OR @schemaOwner IS NULL)
	AND (OBJECT_NAME(sm.object_id) = @name OR @name IS NULL)
ORDER BY o.type;";
                    }
                    else if (IsOracle)
                    {
                        //if you don't have security to view source, you get no rows back
                        sqlCommand = @"SELECT 
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
                    }
                    if (sqlCommand == null) return dt;

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

        private DbParameter AddDbParameter(string parameterName, object value)
        {
            DbParameter parameter = Factory.CreateParameter();
            parameter.ParameterName = parameterName;
            //C# null should be DBNull
            parameter.Value = value ?? DBNull.Value;
            return parameter;
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

        #region Constraints
        /// <summary>
        /// The PK columns for a specific table (if tableName is null or empty, all constraints are returned)
        /// </summary>
        /// <param name="tableName">Name of the table. Oracle names can be case sensitive.</param>
        /// <returns>DataTable with constraint_name, table_name, column_name, ordinal_position</returns>
        public override DataTable PrimaryKeys(string tableName)
        {
            if (!IsMySql && !IsOracle && !IsSqlServer && !IsSqlServerCe4)
                return base.PrimaryKeys(tableName);
            return FindKeys(tableName, GetPrimaryKeyType());
        }
        /// <summary>
        /// The Foreign Key columns for a specific table  (if tableName is null or empty, all constraints are returned)
        /// </summary>
        /// <param name="tableName">Name of the table. Oracle names can be case sensitive.</param>
        public override DataTable ForeignKeys(string tableName)
        {
            if (!IsMySql && !IsOracle && !IsSqlServer && !IsSqlServerCe4)
                return base.ForeignKeys(tableName);
            return FindKeys(tableName, GetForeignKeyType());
        }

        public override DataTable ForeignKeyColumns(string tableName)
        {
            //doesn't exist in SqlServer- but we've overridden anyway
            if (IsSqlServer || IsOracle || IsMySql || IsSqlServerCe4) return new DataTable("ForeignKeyColumns");
            return base.ForeignKeyColumns(tableName);
        }

        /// <summary>
        /// The Unique Key columns for a specific table  (if tableName is null or empty, all constraints are returned). This is Oracle only and returns an empty datatable for SqlServer.
        /// </summary>
        public DataTable UniqueKeys(string tableName)
        {
            return FindKeys(tableName, GetUniqueKeyType());
        }

        /// <summary>
        /// The check constraints for a specific table (if tableName is null or empty, all check constraints are returned)
        /// </summary>
        public DataTable CheckConstraints(string tableName)
        {
            return FindChecks(tableName);
        }

        #region Constraint private methods
        /// <summary>
        /// Finds the primary/foreign/unique keys constraints
        /// </summary>
        /// <param name="tableName">Name of the table. Oracle names can be case sensitive.</param>
        /// <param name="constraintType">Type of the constraint.</param>
        private DataTable FindKeys(string tableName, string constraintType)
        {

            if (!IsMySql && !IsOracle && !IsSqlServer && !IsSqlServerCe4) return new DataTable(constraintType);

            //open a connection
            using (DbConnection conn = Factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                return FindKeys(tableName, constraintType, conn);
            }
        }

        private DataTable FindKeys(string tableName, string constraintType, DbConnection conn)
        {
            DataTable dt = new DataTable(constraintType);

            if (constraintType == "U" && !IsOracle)
                return dt; //only Oracle has this concept

            string sqlCommand = GetKeySql();
            if (String.IsNullOrEmpty(sqlCommand)) return dt;

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


        private string GetPrimaryKeyType()
        {
            return IsOracle ? "P" : "PRIMARY KEY";
        }

        private string GetForeignKeyType()
        {
            return IsOracle ? "R" : "FOREIGN KEY";
        }
        private string GetUniqueKeyType()
        {
            return IsSqlServerCe4 ? "UNIQUE" : "U";
        }

        /// <summary>
        /// Gets the key SQL. GetSchema doesn't work :(
        /// </summary>
        private string GetKeySql()
        {
            string sqlCommand;
            if (IsOracle)//Oracle doesn't have INFORMATION_SCHEMA
            {
                sqlCommand = @"SELECT cols.constraint_name, 
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
            }
            else if (IsSqlServerCe4)
            {
                sqlCommand = @"SELECT
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
            }
            else if (IsMySql) //in MySQL, different constraints for different tables can have the same name (eg Primary)
            {
                sqlCommand = @"SELECT DISTINCT
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
            }
            else //if (IsSqlServer) //use SQL92 INFORMATION_SCHEMA
            {
                sqlCommand = @"SELECT DISTINCT
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
	(keycolumns.table_name = @tableName OR @tableName IS NULL) AND 
	(cons.constraint_schema = @schemaOwner OR @schemaOwner IS NULL) AND 
	cons.constraint_type = @constraint_type";

            }
            return sqlCommand;
        }
        private string GetCheckSql()
        {
            string sqlCommand = null;
            if (IsOracle)//Oracle doesn't have INFORMATION_SCHEMA
            {
                //all_constraints includes NULL constraints. They have generated names- so we exclude them.
                sqlCommand = @"SELECT 
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
            }
            else if (IsSqlServer) //use SQL92 INFORMATION_SCHEMA
            {
                //information_schema.check_constraints doesn't have table, so we join to table constraints
                sqlCommand = @"SELECT 
cons.constraint_name, 
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
            }
            return sqlCommand;
        }

        private DataTable FindChecks(string tableName)
        {
            //open a connection
            using (DbConnection conn = Factory.CreateConnection())
            {
                conn.ConnectionString = ConnectionString;
                return FindChecks(tableName, conn);
            }
        }
        private DataTable FindChecks(string tableName, DbConnection conn)
        {
            DataTable dt = new DataTable("CHECKS");
            string sqlCommand = GetCheckSql();
            if (String.IsNullOrEmpty(sqlCommand)) return dt;

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
        #endregion

        private void AddTableNameSchemaParameters(DbCommand cmd, string tableName)
        {
            var parameter = AddDbParameter("tableName", tableName);
            //sqlserver ce is picky about null parameter types
            parameter.DbType = DbType.String;
            cmd.Parameters.Add(parameter);

            var schemaParameter = AddDbParameter("schemaOwner", Owner);
            schemaParameter.DbType = DbType.String;
            cmd.Parameters.Add(schemaParameter);
        }
        #endregion
    }
}

