using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.SqlServer
{
    internal class UserDefinedTableConstraints : SqlExecuter<DatabaseConstraint>
    {
        public UserDefinedTableConstraints(int? commandTimeout, string owner) : base(commandTimeout, owner)
        {
            Sql = @"SELECT 
SCHEMA_NAME(tt.schema_id) AS Schema_name, 
tt.name AS table_name,  
kc.name AS constraint_name,
kc.type, --PK or UQ
c.name AS column_name,
c.column_id AS ordinal
FROM    sys.table_types AS tt
        INNER JOIN sys.key_constraints AS kc
            ON kc.parent_object_id = tt.type_table_object_id
        INNER JOIN sys.columns AS c
            ON c.object_id = tt.type_table_object_id
WHERE
    (SCHEMA_NAME(tt.schema_id) = @schema OR @schema IS NULL)
ORDER BY tt.name, kc.name, c.column_id
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

        private DatabaseConstraint FindConstraint(string name, string constraintTableName, string schemaName)
        {
            return Result.Find(f => f.Name == name && f.TableName == constraintTableName && f.SchemaOwner == schemaName);
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
            var type = record.GetString("type").ToUpperInvariant();
            var constraint = FindConstraint(name, tableName, schema);
            if (constraint == null)
            {
                constraint = new DatabaseConstraint
                {
                    ConstraintType = type=="PK"? ConstraintType.PrimaryKey: ConstraintType.UniqueKey,
                    SchemaOwner = schema,
                    TableName = tableName,
                    Name = name,
                };
                Result.Add(constraint);
            }
            var columnName = record.GetString("column_name");
            if (columnName != null)
            {
                constraint.Columns.Add(columnName);
            }
        }
    }
}