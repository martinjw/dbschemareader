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
cons.constraint_name, 
cons.constraint_catalog AS constraint_schema,
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
