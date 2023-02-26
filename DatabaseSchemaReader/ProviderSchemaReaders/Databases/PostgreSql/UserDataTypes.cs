using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.PostgreSql
{
    internal class UserDataTypes : SqlExecuter<UserDataType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserDataTypes"/> class.
        /// </summary>
        public UserDataTypes(int? commandTimeout, string owner) : base(commandTimeout, owner)
        {
            Sql = @"SELECT t.typname,
ns.nspname AS schemaname,
t.typnotnull,
t.typdefault,
t2.typname AS BaseName
FROM pg_catalog.pg_type t
  JOIN pg_catalog.pg_namespace ns
    ON ns.oid = t.typnamespace
  JOIN pg_catalog.pg_type t2
    ON t2.oid = t.typbasetype
WHERE t.typtype = 'd'
AND ns.nspname NOT LIKE 'pg_%'
AND ns.nspname != 'information_schema'
AND (ns.nspname = :schema OR :schema IS NULL)
ORDER BY t.typname
";
        }

        /// <summary>
        /// Use this for schema level (i.e. all tables)
        /// </summary>
        public IList<UserDataType> Execute(IConnectionAdapter connectionAdapter)
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

        /// <summary>
        /// Map the result ADO record to the result.
        /// </summary>
        /// <param name="record">The record.</param>
        protected override void Mapper(IDataRecord record)
        {
            var schema = record["schemaname"].ToString();
            var typeName = record["typname"].ToString();
            var tt = new UserDataType
            {
                SchemaOwner = schema,
                Name = typeName,
                Nullable = record.GetBoolean("typnotnull"),
                DefaultValue = record.GetString("typdefault"),
                DbTypeName = record.GetString("BaseName"),
            };
            Result.Add(tt);
        }
    }
}