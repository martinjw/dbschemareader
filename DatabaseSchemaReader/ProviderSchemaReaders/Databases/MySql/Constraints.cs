using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.MySql
{
    class Constraints : SqlExecuter<DatabaseConstraint>
    {
        private readonly string _tableName;
        private readonly ConstraintType _constraintType;

        public Constraints(int? commandTimeout, string owner, string tableName, ConstraintType constraintType)
            : base(commandTimeout, owner)
        {
            _tableName = tableName;
            _constraintType = constraintType;
            Sql = @"SELECT DISTINCT
cons.constraint_schema,
cons.constraint_name, 
keycolumns.table_name, 
column_name, 
ordinal_position, 
refs.unique_constraint_name, 
cons2.TABLE_SCHEMA AS fk_schema,
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
        -- MySQL 8.0.12 bug https://bugs.mysql.com/bug.php?id=90690 requires collate clause
        cons2.constraint_name COLLATE utf8_unicode_ci = refs.unique_constraint_name COLLATE utf8_unicode_ci AND
        cons2.table_name = refs.referenced_table_name
WHERE 
    (keycolumns.table_name = @tableName OR @tableName IS NULL) AND 
    (cons.constraint_schema = @schemaOwner OR @schemaOwner IS NULL) AND 
    cons.constraint_type = @constraint_type
ORDER BY
    cons.constraint_schema, keycolumns.table_name, cons.constraint_name, ordinal_position";

        }

        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "@schemaOwner", Owner);
            AddDbParameter(command, "@tableName", _tableName);

            string constraintType;
            switch (_constraintType)
            {
                case ConstraintType.ForeignKey:
                    constraintType = "FOREIGN KEY";
                    break;
                case ConstraintType.UniqueKey:
                    constraintType = "UNIQUE";
                    break;
                default:
                    constraintType = "PRIMARY KEY";
                    break;
            }

            AddDbParameter(command, "@constraint_type", constraintType);
        }

        private DatabaseConstraint FindConstraint(string name, string constraintTableName, string schemaName)
        {
            return Result.Find(f => f.Name == name && f.TableName == constraintTableName && f.SchemaOwner == schemaName);
        }

        protected override void Mapper(IDataRecord record)
        {
            var schema = record.GetString("constraint_schema");
            var tableName = record.GetString("table_name");
            var name = record.GetString("constraint_name");

            var constraint = FindConstraint(name, tableName, schema);
            if (constraint == null)
            {
                constraint = new DatabaseConstraint
                {
                    ConstraintType = _constraintType,
                    SchemaOwner = schema,
                    TableName = tableName,
                    Name = name,
                    RefersToConstraint = record.GetString("unique_constraint_name"),
                    RefersToTable = record.GetString("fk_table"),
                    RefersToSchema = record.GetString("fk_schema"),
                    DeleteRule = record.GetString("delete_rule"),
                    UpdateRule = record.GetString("update_rule"),
                };
                Result.Add(constraint);
            }
            var columnName = record.GetString("column_name");
            constraint.Columns.Add(columnName);
        }

        public IList<DatabaseConstraint> Execute(IConnectionAdapter connectionAdapter)
        {
            ExecuteDbReader(connectionAdapter);
            return Result;
        }
    }
}
