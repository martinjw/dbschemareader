using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Firebird
{
    class Triggers : SqlExecuter<DatabaseTrigger>
    {
        private readonly string _tableName;
        public Triggers(string owner, string tableName)
        {
            _tableName = tableName;
            Owner = owner;

            Sql = @"SELECT
    r.rdb$owner_name AS OWNER_NAME,
     t.rdb$relation_name AS TABLE_NAME,
     t.rdb$trigger_name AS TRIGGER_NAME,
     t.rdb$trigger_type AS TRIGGER_TYPE,
     t.rdb$trigger_source AS SOURCE
FROM rdb$triggers t
    LEFT JOIN rdb$relations r ON t.rdb$relation_name = r.rdb$relation_name
WHERE
    r.rdb$system_flag = 0 AND
  (@Owner IS NULL OR @Owner = r.rdb$owner_name) AND
  (@TABLE_NAME IS NULL OR @TABLE_NAME = r.rdb$relation_name)
ORDER BY t.rdb$relation_name, t.rdb$trigger_name
";

        }

        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "@Owner", Owner);
            AddDbParameter(command, "@TABLE_NAME", _tableName);
        }

        protected override void Mapper(IDataRecord record)
        {
            var type = record.GetNullableInt("TRIGGER_TYPE");
            var triggerType = FindTriggerType(type);
            var trigger = new DatabaseTrigger
            {
                Name = record.GetString("TRIGGER_NAME").TrimEnd(),
                SchemaOwner = record.GetString("OWNER_NAME").TrimEnd(),
                TableName = record.GetString("TABLE_NAME").TrimEnd(),
                TriggerBody = record.GetString("SOURCE"),
                TriggerType = triggerType,
            };

            Result.Add(trigger);
        }

        private static string FindTriggerType(int? type)
        {
            //the actual calculation is more complex as it's a bitmap
            string triggerType = null;
            switch (type)
            {
                case 1:
                    triggerType = "before insert";
                    break;
                case 2:
                    triggerType = "after insert";
                    break;
                case 3:
                    triggerType = "before update";
                    break;
                case 4:
                    triggerType = "after update";
                    break;
                case 5:
                    triggerType = "before delete";
                    break;
                case 6:
                    triggerType = "after delete";
                    break;
                case 17:
                    triggerType = "before insert or update";
                    break;
                case 18:
                    triggerType = "after insert or update";
                    break;
                case 25:
                    triggerType = "before insert or delete";
                    break;
                case 26:
                    triggerType = "after insert or delete";
                    break;
                case 27:
                    triggerType = "before update or delete";
                    break;
                case 28:
                    triggerType = "after update or delete";
                    break;
                case 113:
                    triggerType = "before insert or update or delete";
                    break;
                case 114:
                    triggerType = "after insert or update or delete";
                    break;
            }
            return triggerType;
        }

        public IList<DatabaseTrigger> Execute(DbConnection dbConnection)
        {
            ExecuteDbReader(dbConnection);
            return Result;
        }
    }
}
