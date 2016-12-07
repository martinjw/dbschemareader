using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Firebird
{
    class Constraints : SqlExecuter<DatabaseConstraint>
    {
        private readonly string _tableName;
        private readonly ConstraintType _constraintType;

        public Constraints(string owner, string tableName, ConstraintType constraintType)
        {
            _tableName = tableName;
            _constraintType = constraintType;
            Owner = owner;
            Sql = @"SELECT 
rel.rdb$owner_name as owner,
rc.rdb$relation_name as table_name,
rc.rdb$constraint_name as constraint_name,
s.rdb$field_name as column_name,
rc.rdb$constraint_type as constraint_type,
i2.rdb$relation_name as fk_table,
s2.rdb$field_name as references_column,
refc.rdb$update_rule as update_rule,
refc.rdb$delete_rule as delete_rule,
(s.rdb$field_position + 1) as field_position
FROM rdb$index_segments s
LEFT JOIN rdb$indices i on i.rdb$index_name = s.rdb$index_name
LEFT JOIN rdb$relation_constraints rc on rc.rdb$index_name = s.rdb$index_name
LEFT JOIN rdb$ref_constraints refc on rc.rdb$constraint_name = refc.rdb$constraint_name
LEFT JOIN rdb$relation_constraints rc2 on rc2.rdb$constraint_name = refc.rdb$const_name_uq
LEFT JOIN rdb$indices i2 on i2.rdb$index_name = rc2.rdb$index_name
LEFT JOIN rdb$index_segments s2 on i2.rdb$index_name = s2.rdb$index_name
LEFT JOIN rdb$relations rel on rel.rdb$relation_name = rc.rdb$relation_name
WHERE
  rc.rdb$constraint_type IS NOT NULL AND
  rel.rdb$system_flag = 0 AND
  (@owner is null or @owner = rel.rdb$owner_name) AND
  (@table_name is null or @table_name = rc.rdb$relation_name) AND
  rc.rdb$constraint_type = @constraint_type
ORDER BY 
    rc.rdb$relation_name, rc.rdb$constraint_name, s.rdb$field_position
";

        }

        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "@owner", Owner);
            AddDbParameter(command, "@table_name", _tableName);

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
            var schema = record.GetString("owner").TrimEnd();
            var tableName = record.GetString("table_name").TrimEnd();
            var name = record.GetString("constraint_name").TrimEnd();

            var constraint = FindConstraint(name, tableName, schema);
            if (constraint == null)
            {
                var refersToTable = record.GetString("fk_table");
                if (!string.IsNullOrEmpty(refersToTable)) refersToTable = refersToTable.TrimEnd();
                constraint = new DatabaseConstraint
                {
                    ConstraintType = _constraintType,
                    SchemaOwner = schema,
                    TableName = tableName,
                    Name = name,
                    RefersToTable = refersToTable,
                    DeleteRule = record.GetString("delete_rule"),
                    UpdateRule = record.GetString("update_rule"),
                };
                Result.Add(constraint);
            }
            var columnName = record.GetString("column_name").TrimEnd();
            if (!constraint.Columns.Contains(columnName))
            {
                constraint.Columns.Add(columnName);
            }
        }

        public IList<DatabaseConstraint> Execute(DbConnection dbConnection)
        {
            ExecuteDbReader(dbConnection);
            return Result;
        }
    }
}
