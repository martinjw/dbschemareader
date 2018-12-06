using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Oracle
{
    class Constraints : OracleSqlExecuter<DatabaseConstraint>
    {
        private readonly string _tableName;
        private readonly ConstraintType _constraintType;

        public Constraints(int? commandTimeout, string owner, string tableName, ConstraintType constraintType) : base(commandTimeout, owner)
        {
            _tableName = tableName;
            _constraintType = constraintType;
            Owner = owner;
            Sql = @"SELECT cols.constraint_name,
cols.owner AS constraint_schema, 
cols.table_name, 
cols.column_name, 
cols.position AS ordinal_position, 
cons.r_constraint_name AS unique_constraint_name, 
cons2.table_name AS fk_table,
cons2.r_owner as fk_schema,
cons.delete_rule
FROM all_constraints cons
INNER JOIN all_cons_columns cols 
  ON cons.constraint_name = cols.constraint_name
  AND cons.owner = cols.owner
LEFT OUTER JOIN all_constraints cons2 
  ON cons.r_constraint_name = cons2.constraint_name
  AND cons.owner = cons2.owner
WHERE 
    cons.owner NOT IN ('SYS', 'SYSMAN', 'CTXSYS', 'MDSYS', 'OLAPSYS', 'ORDSYS', 'OUTLN', 'WKSYS', 'WMSYS', 'XDB', 'ORDPLUGINS', 'SYSTEM') AND
   (cols.table_name = :tableName OR :tableName IS NULL) AND 
   (cols.owner = :schemaOwner OR :schemaOwner IS NULL) AND 
    cons.constraint_type = :constraint_type
ORDER BY cols.constraint_name,cols.table_name, cols.position";

        }

        protected override void AddParameters(DbCommand command)
        {
            EnsureOracleBindByName(command);
            AddDbParameter(command, "schemaOwner", Owner);
            AddDbParameter(command, "tableName", _tableName);

            string constraintType;
            switch (_constraintType)
            {
                case ConstraintType.ForeignKey:
                    constraintType = "R";
                    break;
                case ConstraintType.UniqueKey:
                    constraintType = "U";
                    break;
                default:
                    constraintType = "P";
                    break;
            }

            AddDbParameter(command, "constraint_type", constraintType);
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
                    //UpdateRule = record.GetString("update_rule"),
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
