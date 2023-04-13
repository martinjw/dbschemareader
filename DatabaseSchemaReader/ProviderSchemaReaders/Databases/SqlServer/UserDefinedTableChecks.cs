using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.SqlServer
{
    internal class UserDefinedTableChecks : SqlExecuter<DatabaseConstraint>
    {
        public UserDefinedTableChecks(int? commandTimeout, string owner) : base(commandTimeout, owner)
        {
            Sql = @"SELECT
SCHEMA_NAME(tt.schema_id) AS Schema_name,
tt.name as table_name,
cc.name AS constraint_name,
cc.definition AS Expression
FROM sys.check_constraints AS cc
INNER JOIN sys.table_types AS tt 
ON cc.parent_object_id =  tt.type_table_object_id
WHERE
    (SCHEMA_NAME(tt.schema_id) = @schema OR @schema IS NULL)
ORDER BY tt.name, cc.name
";
        }

        /// <summary>
        /// Use this for schema level (i.e. all tables)
        /// </summary>
        public IList<DatabaseConstraint> Execute(IConnectionAdapter connectionAdapter)
        {
            ExecuteDbReader(connectionAdapter);
            return Result;
        }

        /// <summary>
        /// Add parameter(s).
        /// </summary>
        /// <param name="command">The command.</param>
        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "schema", Owner);
        }

        /// <summary>
        /// Map the result ADO record to the result.
        /// </summary>
        /// <param name="record">The record.</param>
        protected override void Mapper(IDataRecord record)
        {
            var schema = record.GetString("Schema_name");
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
    }
}