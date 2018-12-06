using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.MySql
{
    internal class Views : SqlExecuter<DatabaseView>
    {
        private readonly string _viewName;

        public Views(int? commandTimeout, string owner, string viewName)
            : base(commandTimeout, owner)
        {
            _viewName = viewName;
            Sql = @"select TABLE_SCHEMA, TABLE_NAME, VIEW_DEFINITION 
from INFORMATION_SCHEMA.VIEWS 
where 
    (TABLE_SCHEMA = @Owner or (@Owner is null)) and 
    (TABLE_NAME = @TABLE_NAME or (@TABLE_NAME is null))
 order by 
    TABLE_SCHEMA, TABLE_NAME";
        }

        public IList<DatabaseView> Execute(IConnectionAdapter connectionAdapter)
        {
            ExecuteDbReader(connectionAdapter);
            return Result;
        }

        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "@Owner", Owner);
            AddDbParameter(command, "@TABLE_NAME", _viewName);
        }

        protected override void Mapper(IDataRecord record)
        {
            var schema = record["TABLE_SCHEMA"].ToString();
            var name = record["TABLE_NAME"].ToString();
            var table = new DatabaseView
                        {
                            Name = name,
                            SchemaOwner = schema,
							Sql = record.GetString("VIEW_DEFINITION"),
                        };

            Result.Add(table);
        }
    }
}
