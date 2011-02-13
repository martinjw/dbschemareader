using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen
{
    /// <summary>
    /// Generate Ddl
    /// </summary>
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
            }
            return null;
        }
    }
}
