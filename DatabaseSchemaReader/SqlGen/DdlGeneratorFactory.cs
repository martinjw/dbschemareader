using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen
{
    /// <summary>
    /// Generate Ddl
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Ddl")]
    public class DdlGeneratorFactory
    {
        private readonly SqlType _sqlType;

        /// <summary>
        /// Initializes a new instance of the <see cref="DdlGeneratorFactory"/> class.
        /// </summary>
        /// <param name="sqlType">Type of the SQL.</param>
        public DdlGeneratorFactory(SqlType sqlType)
        {
            _sqlType = sqlType;
        }

        /// <summary>
        /// Creates a table DDL generator.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <returns></returns>
        public ITableGenerator TableGenerator(DatabaseTable table)
        {
            switch (_sqlType)
            {
                case SqlType.SqlServer:
                    return new SqlServer.TableGenerator(table);
                case SqlType.Oracle:
                    return new Oracle.TableGenerator(table);
                case SqlType.MySql:
                    return new MySql.TableGenerator(table);
                case SqlType.SQLite:
                    return new SqLite.TableGenerator(table);
                case SqlType.SqlServerCe:
                    return new SqlServerCe.TableGenerator(table);
                case SqlType.PostgreSql:
                    return new PostgreSql.TableGenerator(table);
                case SqlType.Db2:
                    return new Db2.TableGenerator(table);
            }
            return null;
        }

        /// <summary>
        /// Creates a DDL generator for all tables.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <returns></returns>
        public ITablesGenerator AllTablesGenerator(DatabaseSchema schema)
        {
            switch (_sqlType)
            {
                case SqlType.SqlServer:
                    return new SqlServer.TablesGenerator(schema);
                case SqlType.Oracle:
                    return new Oracle.TablesGenerator(schema);
                case SqlType.MySql:
                    return new MySql.TablesGenerator(schema);
                case SqlType.SQLite:
                    return new SqLite.TablesGenerator(schema);
                case SqlType.SqlServerCe:
                    return new SqlServerCe.TablesGenerator(schema);
                case SqlType.PostgreSql:
                    return new PostgreSql.TablesGenerator(schema);
                case SqlType.Db2:
                    return new Db2.TablesGenerator(schema);
            }
            return null;
        }

        /// <summary>
        /// Creates a stored procedure generator for a table.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <returns></returns>
        public IProcedureGenerator ProcedureGenerator(DatabaseTable table)
        {
            switch (_sqlType)
            {
                case SqlType.SqlServer:
                    return new SqlServer.ProcedureGenerator(table);
                case SqlType.Oracle:
                    return new Oracle.ProcedureGenerator(table);
                case SqlType.MySql:
                    return new MySql.ProcedureGenerator(table);
                case SqlType.SQLite:
                    return null; //no stored procedures in SqlLite
                case SqlType.SqlServerCe:
                    return null; //no stored procedures in SqlServerCE
                case SqlType.PostgreSql:
                    return null; //for now
                case SqlType.Db2:
                    return new Db2.ProcedureGenerator(table);
            }
            return null;
        }

        /// <summary>
        /// Internal method to find constraint writer
        /// </summary>
        /// <param name="databaseTable">The database table.</param>
        /// <returns></returns>
        internal ConstraintWriterBase ConstraintWriter(DatabaseTable databaseTable)
        {
            switch (_sqlType)
            {
                case SqlType.SqlServer:
                    return new SqlServer.ConstraintWriter(databaseTable);
                case SqlType.Oracle:
                    return new Oracle.ConstraintWriter(databaseTable);
                case SqlType.MySql:
                    return new MySql.ConstraintWriter(databaseTable);
                case SqlType.SQLite:
                    return null; //can't alter constraints after creating table
                case SqlType.SqlServerCe:
                    return new SqlServer.ConstraintWriter(databaseTable);
                case SqlType.PostgreSql:
                    return new PostgreSql.ConstraintWriter(databaseTable);
                case SqlType.Db2:
                    return new Db2.ConstraintWriter(databaseTable);
            }
            return null;
        }

        /// <summary>
        /// Creates a migration generator (Create Tables, add/alter/drop columns)
        /// </summary>
        /// <returns></returns>
        public IMigrationGenerator MigrationGenerator()
        {
            switch (_sqlType)
            {
                case SqlType.SqlServer:
                    return new SqlServer.SqlServerMigrationGenerator();
                case SqlType.Oracle:
                    return new Oracle.OracleMigrationGenerator();
                case SqlType.MySql:
                    return new MySql.MySqlMigrationGenerator();
                case SqlType.SQLite:
                    return new SqLite.SqLiteMigrationGenerator();
                case SqlType.SqlServerCe:
                    return new SqlServerCe.SqlServerCeMigrationGenerator();
                case SqlType.PostgreSql:
                    return new PostgreSql.PostgreSqlMigrationGenerator();
                case SqlType.Db2:
                    return new Db2.Db2MigrationGenerator();
            }
            return null;
        }
    }
}
