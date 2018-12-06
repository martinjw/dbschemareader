using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.SqlServerCe
{
    class Constraints : SqlExecuter<DatabaseConstraint>
    {
        private readonly string _tableName;
        private readonly ConstraintType _constraintType;

        public Constraints(int? commandTimeout, string owner, string tableName, ConstraintType constraintType) : base(commandTimeout, owner)
        {
            _tableName = tableName;
            _constraintType = constraintType;
            Owner = owner;
            Sql = @"SELECT
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
    CONS.CONSTRAINT_TYPE = @constraint_type
ORDER BY
    KEYCOLUMNS.TABLE_NAME, KEYCOLUMNS.CONSTRAINT_NAME, KEYCOLUMNS.ORDINAL_POSITION";

        }

        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "schemaOwner", Owner, DbType.String);
            AddDbParameter(command, "tableName", _tableName, DbType.String);

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

            AddDbParameter(command, "constraint_type", constraintType, DbType.String);
        }

        private DatabaseConstraint FindConstraint(string name, string constraintTableName)
        {
            return Result.Find(f => f.Name == name && f.TableName == constraintTableName);
        }

        protected override void Mapper(IDataRecord record)
        {
            var tableName = record.GetString("table_name");
            var name = record.GetString("constraint_name");

            var constraint = FindConstraint(name, tableName);
            if (constraint == null)
            {
                constraint = new DatabaseConstraint
                {
                    ConstraintType = _constraintType,
                    TableName = tableName,
                    Name = name,
                    RefersToConstraint = record.GetString("unique_constraint_name"),
                    RefersToTable = record.GetString("fk_table"),
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
