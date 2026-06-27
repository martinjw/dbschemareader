using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.PostgreSql
{
    class CheckConstraints : SqlExecuter<DatabaseConstraint>
    {
        private readonly string _tableName;

        public CheckConstraints(int? commandTimeout, string owner, string tableName) : base(commandTimeout, owner)
        {
            _tableName = tableName;
            Owner = owner;
            Sql = @"SELECT
    con.conname AS constraint_name,
    n.nspname AS constraint_schema,
    c.relname AS table_name,
    cons.check_clause AS Expression
FROM pg_constraint con
         JOIN pg_class c ON con.conrelid = c.oid
         JOIN pg_namespace n ON con.connamespace = n.oid
         JOIN INFORMATION_SCHEMA.CHECK_CONSTRAINTS cons ON con.conname = cons.constraint_name
WHERE
    (c.relname = :tableName OR :tableName IS NULL) AND 
    n.nspname NOT IN ('pg_catalog', 'information_schema') AND 
    con.contype = 'c'
ORDER BY c.relname, con.conname;";

        }

        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "schemaOwner", Owner);
            AddDbParameter(command, "tableName", _tableName);
        }

        protected override void Mapper(IDataRecord record)
        {
            var schema = record.GetString("constraint_schema");
            var tableName = record.GetString("table_name");
            var name = record.GetString("constraint_name");
            var expression = record.GetString("Expression");
            var constraint = new DatabaseConstraint
            {
                ConstraintType = ConstraintType.Check,
                Expression = expression,
                SchemaOwner = schema,
                TableName = tableName,
                Name = name,
            };
            Result.Add(constraint);
        }

        public IList<DatabaseConstraint> Execute(IConnectionAdapter connectionAdapter)
        {
            ExecuteDbReader(connectionAdapter);
            return Result;
        }
    }
}
