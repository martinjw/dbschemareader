using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Firebird
{
    class CheckConstraints : SqlExecuter<DatabaseConstraint>
    {
        private readonly string _tableName;

        public CheckConstraints(string owner, string tableName)
        {
            _tableName = tableName;
            Owner = owner;
            Sql = @"SELECT 
chk.rdb$constraint_name AS constraint_name,  
trig.rdb$relation_name AS table_name,
rel.rdb$owner_name AS owner_name,
trig.rdb$trigger_source AS expression
FROM rdb$check_constraints chk
INNER JOIN rdb$triggers trig 
    ON chk.rdb$trigger_name = trig.rdb$trigger_name
INNER JOIN rdb$relations rel 
    ON trig.rdb$relation_name = rel.rdb$relation_name
WHERE rel.rdb$system_flag = 0 AND
  (@owner is null or @owner = rel.rdb$owner_name) AND
  (@table_name is null or @table_name = rel.rdb$relation_name)
ORDER BY rel.rdb$relation_name, chk.rdb$constraint_name
";

        }

        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "owner", Owner);
            AddDbParameter(command, "table_name", _tableName);
        }

        protected override void Mapper(IDataRecord record)
        {
            var schema = record.GetString("owner_name").TrimEnd();
            var tableName = record.GetString("table_name").TrimEnd();
            var name = record.GetString("constraint_name").TrimEnd();
            var expression = record.GetString("expression");
            var constraint = new DatabaseConstraint
            {
                ConstraintType = ConstraintType.Check,
                Expression = expression,
                SchemaOwner = schema,
                TableName = tableName,
                Name = name,
            };
            if (!Result.Exists(x => x.Name == name))
            {
                Result.Add(constraint);
            }
        }

        public IList<DatabaseConstraint> Execute(DbConnection dbConnection)
        {
            ExecuteDbReader(dbConnection);
            return Result;
        }
    }
}
