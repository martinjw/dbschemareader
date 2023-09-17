using DatabaseSchemaReader.CodeGen;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen
{
    /// <summary>
    /// Extensions to quickly turn schema into sql
    /// </summary>
    public static class SchemaExtensions
    {

        /// <summary>
        /// Generate CREATE TABLE for each table.
        /// </summary>
        /// <param name="schema">The database schema</param>
        /// <param name="sqlGenerationParameters">Escape the table and column names</param>
        public static string ToSqlCreateTables(this DatabaseSchema schema, SqlGenerationParameters sqlGenerationParameters = null)
        {
            if (schema == null) return null;

            var sqlType = ProviderToSqlType.Convert(schema.Provider) ?? SqlType.SqlServer;
            var ddlGeneratorFactory = new DdlGeneratorFactory(sqlType);
            if (sqlGenerationParameters != null && sqlGenerationParameters.UseGranularBatching)
            {
                ddlGeneratorFactory.UseGranularBatching = true;
            }

            var tg = ddlGeneratorFactory.AllTablesGenerator(schema);
            if (sqlGenerationParameters != null)
            {
                tg.IncludeSchema = sqlGenerationParameters.IncludeSchema;
                tg.EscapeNames = sqlGenerationParameters.EscapeNames;
            }

            return tg.Write();

        }
        /// <summary>
        /// Generate a CREATE TABLE sql string. For foreign keys, use <see cref="ToSqlCreateForeignKeys"/>
        /// </summary>
        /// <param name="table">The table (should be attached to a databaseSchema)</param>
        /// <param name="sqlGenerationParameters">Escape the table and column names</param>
        public static string ToSqlCreateTable(this DatabaseTable table, SqlGenerationParameters sqlGenerationParameters = null)
        {
            if (table == null) return null;

            var sqlType = ProviderToSqlType.Convert(table.DatabaseSchema?.Provider) ?? SqlType.SqlServer;
            var ddlGeneratorFactory = new DdlGeneratorFactory(sqlType);
            if (sqlGenerationParameters != null && sqlGenerationParameters.UseGranularBatching)
            {
                ddlGeneratorFactory.UseGranularBatching = true;
            }

            var tg = ddlGeneratorFactory.TableGenerator(table);
            if (sqlGenerationParameters != null)
            {
                tg.IncludeSchema = sqlGenerationParameters.IncludeSchema;
                tg.EscapeNames = sqlGenerationParameters.EscapeNames;
            }

            return tg.Write();
        }

        /// <summary>
        /// Generate ALTER TABLE x ADD CONSTRAINT y for all foreign keys
        /// </summary>
        /// <param name="table">The table (should be attached to a databaseSchema)</param>
        /// <param name="sqlGenerationParameters">Escape the table and column names</param>
        public static string ToSqlCreateForeignKeys(this DatabaseTable table, SqlGenerationParameters sqlGenerationParameters = null)
        {
            if (table == null) return null;

            var sqlType = ProviderToSqlType.Convert(table.DatabaseSchema?.Provider) ?? SqlType.SqlServer;
            var ddlGeneratorFactory = new DdlGeneratorFactory(sqlType);
            if (sqlGenerationParameters != null && sqlGenerationParameters.UseGranularBatching)
            {
                ddlGeneratorFactory.UseGranularBatching = true;
            }

            var cw = ddlGeneratorFactory.ConstraintWriter(table);
            if (sqlGenerationParameters != null)
            {
                cw.EscapeNames = sqlGenerationParameters.EscapeNames;
                cw.IncludeSchema = sqlGenerationParameters.IncludeSchema;
            }

            return cw.WriteForeignKeys();
        }

        /// <summary>
        /// Generate SELECT statement by the primary key
        /// </summary>
        /// <param name="table">The table (should be attached to a databaseSchema)</param>
        /// <param name="escapeNames">Table and column names should be escaped</param>
        public static string ToSqlSelectById(this DatabaseTable table, bool escapeNames= true)
        {
            if (table == null) return null;

            var sqlType = ProviderToSqlType.Convert(table.DatabaseSchema?.Provider) ?? SqlType.SqlServer;
            var sqlWriter = new SqlWriter(table, sqlType)
            {
                EscapeNames = escapeNames
            };

            return sqlWriter.SelectByIdSql();
        }

        /// <summary>
        /// Generate SELECT statement with paging
        /// </summary>
        /// <param name="table">The table (should be attached to a databaseSchema)</param>
        /// <param name="escapeNames">Table and column names should be escaped</param>
        public static string ToSqlSelectPaged(this DatabaseTable table, bool escapeNames = true)
        {
            if (table == null) return null;

            var sqlType = ProviderToSqlType.Convert(table.DatabaseSchema?.Provider) ?? SqlType.SqlServer;
            var sqlWriter = new SqlWriter(table, sqlType)
            {
                EscapeNames = escapeNames
            };

            return sqlWriter.SelectPageSql();
        }

        /// <summary>
        /// Generate INSERT statement
        /// </summary>
        /// <param name="table">The table (should be attached to a databaseSchema)</param>
        /// <param name="escapeNames">Table and column names should be escaped</param>
        public static string ToSqlSelectInsert(this DatabaseTable table, bool escapeNames = true)
        {
            if (table == null) return null;

            var sqlType = ProviderToSqlType.Convert(table.DatabaseSchema?.Provider) ?? SqlType.SqlServer;
            var sqlWriter = new SqlWriter(table, sqlType)
            {
                EscapeNames = escapeNames
            };

            return sqlWriter.InsertSql();
        }

        /// <summary>
        /// Generate Update statement
        /// </summary>
        /// <param name="table">The table (should be attached to a databaseSchema)</param>
        /// <param name="escapeNames">Table and column names should be escaped</param>
        public static string ToSqlSelectUpdate(this DatabaseTable table, bool escapeNames = true)
        {
            if (table == null) return null;

            var sqlType = ProviderToSqlType.Convert(table.DatabaseSchema?.Provider) ?? SqlType.SqlServer;
            var sqlWriter = new SqlWriter(table, sqlType)
            {
                EscapeNames = escapeNames
            };

            return sqlWriter.UpdateSql();
        }

        /// <summary>
        /// Generate a C# class
        /// </summary>
        /// <param name="table">The table (should be attached to a databaseSchema)</param>
        public static string ToClass(this DatabaseTable table)
        {
            if (table == null) return null;

            var cw = new ClassWriter(table, new CodeWriterSettings { CodeTarget = CodeTarget.PocoEfCore});
            return cw.Write();
        }

        /// <summary>
        /// Generate ALTER TABLE ADD COLUMN sql
        /// </summary>
        /// <param name="column">The column (attached to a table)</param>
        /// <param name="sqlGenerationParameters">Escape the table and column names</param>
        public static string ToSqlAddColumn(this DatabaseColumn column, SqlGenerationParameters sqlGenerationParameters = null)
        {
            if (column == null) return null;
            if(column.Table == null) return null;

            var sqlType = ProviderToSqlType.Convert(column.Table.DatabaseSchema?.Provider) ?? SqlType.SqlServer;
            var ddlGeneratorFactory = new DdlGeneratorFactory(sqlType);
            if (sqlGenerationParameters != null && sqlGenerationParameters.UseGranularBatching)
            {
                ddlGeneratorFactory.UseGranularBatching = true;
            }

            var migrationGenerator = ddlGeneratorFactory.MigrationGenerator();
            if (sqlGenerationParameters != null)
            {
                migrationGenerator.EscapeNames = sqlGenerationParameters.EscapeNames;
                migrationGenerator.IncludeSchema = sqlGenerationParameters.IncludeSchema;
            }

            return migrationGenerator.AddColumn(column.Table, column);
        }

        /// <summary>
        /// Generate ALTER TABLE DROP COLUMN sql
        /// </summary>
        /// <param name="column">The column (attached to a table)</param>
        /// <param name="sqlGenerationParameters">Escape the table and column names</param>
        public static string ToSqlDropColumn(this DatabaseColumn column, SqlGenerationParameters sqlGenerationParameters = null)
        {
            if (column == null) return null;
            if (column.Table == null) return null;

            var sqlType = ProviderToSqlType.Convert(column.Table.DatabaseSchema?.Provider) ?? SqlType.SqlServer;
            var ddlGeneratorFactory = new DdlGeneratorFactory(sqlType);
            if (sqlGenerationParameters != null && sqlGenerationParameters.UseGranularBatching)
            {
                ddlGeneratorFactory.UseGranularBatching = true;
            }

            var migrationGenerator = ddlGeneratorFactory.MigrationGenerator();
            if (sqlGenerationParameters != null)
            {
                migrationGenerator.EscapeNames = sqlGenerationParameters.EscapeNames;
                migrationGenerator.IncludeSchema = sqlGenerationParameters.IncludeSchema;
            }

            return migrationGenerator.DropColumn(column.Table, column);
        }


        /// <summary>
        /// Generate ALTER TABLE ADD CONSTRAINT sql
        /// </summary>
        /// <param name="constraint">The constraint</param>
        /// <param name="table">The table which the constraint is attached to</param>
        /// <param name="sqlGenerationParameters">Escape the table and column names</param>
        public static string ToSqlAddConstraint(this DatabaseConstraint constraint, DatabaseTable table, SqlGenerationParameters sqlGenerationParameters = null)
        {
            if (constraint == null) return null;
            if (table == null) return null;

            var sqlType = ProviderToSqlType.Convert(table.DatabaseSchema?.Provider) ?? SqlType.SqlServer;
            var ddlGeneratorFactory = new DdlGeneratorFactory(sqlType);
            if (sqlGenerationParameters != null && sqlGenerationParameters.UseGranularBatching)
            {
                ddlGeneratorFactory.UseGranularBatching = true;
            }

            var migrationGenerator = ddlGeneratorFactory.MigrationGenerator();
            if (sqlGenerationParameters != null)
            {
                migrationGenerator.EscapeNames = sqlGenerationParameters.EscapeNames;
                migrationGenerator.IncludeSchema = sqlGenerationParameters.IncludeSchema;
            }

            return migrationGenerator.AddConstraint(table, constraint);
        }

        /// <summary>
        /// Generate CREATE TYPE
        /// </summary>
        /// <param name="udt">The user defined type</param>
        /// <param name="schema">The schema</param>
        /// <param name="sqlGenerationParameters">Escape the table and column names</param>
        public static string ToSqlAddUserDefinedType(this UserDataType udt, DatabaseSchema schema, SqlGenerationParameters sqlGenerationParameters = null)
        {
            if(udt == null) return null;
            if(schema == null) return null;
            var sqlType = ProviderToSqlType.Convert(schema.Provider) ?? SqlType.SqlServer;
            var ddlGeneratorFactory = new DdlGeneratorFactory(sqlType);
            if (sqlGenerationParameters != null && sqlGenerationParameters.UseGranularBatching)
            {
                ddlGeneratorFactory.UseGranularBatching = true;
            }
            var migrationGenerator = ddlGeneratorFactory.MigrationGenerator();
            if (sqlGenerationParameters != null)
            {
                migrationGenerator.EscapeNames = sqlGenerationParameters.EscapeNames;
                migrationGenerator.IncludeSchema = sqlGenerationParameters.IncludeSchema;
            }

            return migrationGenerator.AddUserDataType(udt);
        }

        /// <summary>
        /// Generate CREATE TYPE
        /// </summary>
        /// <param name="udt">The user defined table</param>
        /// <param name="schema">The schema</param>
        /// <param name="sqlGenerationParameters">Escape the table and column names</param>
        public static string ToSqlAddUserDefinedTable(this UserDefinedTable udt, DatabaseSchema schema, SqlGenerationParameters sqlGenerationParameters = null)
        {
            if (udt == null) return null;
            if (schema == null) return null;
            var sqlType = ProviderToSqlType.Convert(schema.Provider) ?? SqlType.SqlServer;
            var ddlGeneratorFactory = new DdlGeneratorFactory(sqlType);
            if (sqlGenerationParameters != null && sqlGenerationParameters.UseGranularBatching)
            {
                ddlGeneratorFactory.UseGranularBatching = true;
            }
            var migrationGenerator = ddlGeneratorFactory.MigrationGenerator();
            if (sqlGenerationParameters != null)
            {
                migrationGenerator.EscapeNames = sqlGenerationParameters.EscapeNames;
                migrationGenerator.IncludeSchema = sqlGenerationParameters.IncludeSchema;
            }

            return migrationGenerator.AddUserDefinedTableType(udt);
        }

    }
}