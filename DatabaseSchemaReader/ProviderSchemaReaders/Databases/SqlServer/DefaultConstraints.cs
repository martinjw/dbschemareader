using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.SqlServer
{
    class DefaultConstraints : SqlExecuter<DatabaseConstraint>
    {
        private readonly string _tableName;

        public DefaultConstraints(int? commandTimeout, string owner, string tableName) : base(commandTimeout, owner)
        {
            _tableName = tableName;
            Owner = owner;
            Sql = @"SELECT 
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

        }

        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "schemaOwner", Owner);
            AddDbParameter(command, "tableName", _tableName);
        }

        private DatabaseConstraint FindConstraint(string name, string constraintTableName, string schemaName)
        {
            return Result.Find(f => f.Name == name && f.TableName == constraintTableName && f.SchemaOwner == schemaName);
        }

        protected override void Mapper(IDataRecord record)
        {
            var schema = record.GetString("SCHEMA_NAME");
            var tableName = record.GetString("TABLE_NAME");
            var name = record.GetString("CONSTRAINT_NAME");

            var constraint = FindConstraint(name, tableName, schema);
            if (constraint == null)
            {
                constraint = new DatabaseConstraint
                {
                    ConstraintType = ConstraintType.Default,
                    SchemaOwner = schema,
                    TableName = tableName,
                    Name = name,
                    Expression = record.GetString("EXPRESSION"),
                };
            Result.Add(constraint);
            }
            var columnName = record.GetString("COLUMN_NAME");
            constraint.Columns.Add(columnName);
        }

        public IList<DatabaseConstraint> Execute(IConnectionAdapter connectionAdapter)
        {
            ExecuteDbReader(connectionAdapter);
            return Result;
        }
    }
}
