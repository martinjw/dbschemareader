using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Oracle
{
    class CheckConstraints : OracleSqlExecuter<DatabaseConstraint>
    {
        private readonly string _tableName;

        public CheckConstraints(int? commandTimeout, string owner, string tableName) : base(commandTimeout, owner)
        {
            _tableName = tableName;
            Owner = owner;
            //all_constraints includes NULL constraints. They have generated names- so we exclude them.
            //Hmm, search_condition is an Oracle LONG which we can't read.
            //TO_LOB can only be used on create table as select, xml fails on < in there... 
            Sql = @"SELECT 
cons.constraint_name, 
cons.owner AS constraint_schema,
cons.table_name,
cons.search_condition AS Expression
FROM all_constraints cons
 WHERE 
    (cons.table_name = :tableName OR :tableName IS NULL) AND 
    (cons.owner = :schemaOwner OR :schemaOwner IS NULL) AND 
     cons.constraint_type = 'C' AND 
     cons.generated <> 'GENERATED NAME'
ORDER BY cons.table_name, cons.constraint_name";

        }

        protected override void AddParameters(DbCommand command)
        {
            EnsureOracleBindByName(command);
            AddDbParameter(command, "schemaOwner", Owner);
            AddDbParameter(command, "tableName", _tableName);
        }

        protected override void Mapper(IDataRecord record)
        {
            var schema = record.GetString("constraint_schema");
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

        public IList<DatabaseConstraint> Execute(IConnectionAdapter connectionAdapter)
        {
            ExecuteDbReader(connectionAdapter);
            return Result;
        }
    }
}
