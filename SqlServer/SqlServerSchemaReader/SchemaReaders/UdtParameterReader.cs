using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.Databases;
using SqlServerSchemaReader.Schema;
using System;
using System.Data;
using System.Data.Common;

namespace SqlServerSchemaReader.SchemaReaders
{
    /// <summary>
    /// Read dependent parameters for user defined types
    /// </summary>
    internal class UdtParameterReader : SqlExecuter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UdtParameterReader"/> class.
        /// </summary>
        public UdtParameterReader()
        {
            Sql = @"SELECT
SCHEMA_NAME(t.schema_id) AS schema_name,
OBJECT_NAME(p.object_id) AS proc_name,
p.name AS param_name,
TYPE_NAME(p.user_type_id) AS type_name
FROM sys.parameters p
INNER JOIN sys.types t
 ON t.user_type_id= p.user_type_id
WHERE t.is_user_defined = 1
AND (SCHEMA_NAME(t.schema_id) = @schema OR @schema IS NULL)";
        }

        private SqlServerSchema _schema;

        /// <summary>
        /// Use this for schema level (i.e. all tables)
        /// </summary>
        public void Execute(SqlServerSchema schema, DbConnection connection)
        {
            _schema = schema;
            ExecuteDbReader(connection);
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
            var schema = record["schema_name"].ToString();
            var typeName = record["type_name"].ToString();
            AliasType aliasType = null;
            TableType tableType = null;
            aliasType =
                _schema.AliasTypes.Find(
                    t => string.Equals(t.Name, typeName, StringComparison.OrdinalIgnoreCase) &&
                         string.Equals(t.SchemaOwner, schema, StringComparison.OrdinalIgnoreCase));
            if (aliasType == null)
            {
                tableType = _schema.TableTypes.Find(
                    t => string.Equals(t.Name, typeName, StringComparison.OrdinalIgnoreCase) &&
                         string.Equals(t.SchemaOwner, schema, StringComparison.OrdinalIgnoreCase));
                if (tableType == null)
                {
                    return;
                }
            }

            var arg = new DatabaseArgument
            {
                SchemaOwner = schema,
                ProcedureName = record.GetString("proc_name"),
                Name = record.GetString("param_name"),
                DatabaseDataType = typeName,
            };
            //either alias or tableType exist
            if (aliasType != null)
            {
                aliasType.DependentArguments.Add(arg);
            }
            else
            {
                tableType.DependentArguments.Add(arg);
            }
        }
    }
}