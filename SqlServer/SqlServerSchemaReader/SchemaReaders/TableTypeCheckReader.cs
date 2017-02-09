using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.Databases;
using SqlServerSchemaReader.Schema;
using System;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace SqlServerSchemaReader.SchemaReaders
{
    /// <summary>
    /// Read check constraints for user defined table types (UDTs)
    /// </summary>
    internal class TableTypeCheckReader : SqlExecuter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TableTypeCheckReader"/> class.
        /// </summary>
        public TableTypeCheckReader()
        {
            Sql = @"SELECT
SCHEMA_NAME(tt.schema_id) AS schema_name,
tt.name AS TYPE_NAME,
chk.name,
chk.definition
FROM sys.table_types tt
INNER JOIN sys.check_constraints chk
   ON chk.parent_object_id = tt.type_table_object_id
WHERE (SCHEMA_NAME(tt.schema_id) = @schema OR @schema IS NULL)";
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
            var typeName = record["TYPE_NAME"].ToString();
            var tt =
                _schema.TableTypes.FirstOrDefault(
                    t => string.Equals(t.Name, typeName, StringComparison.OrdinalIgnoreCase) &&
                         string.Equals(t.SchemaOwner, schema, StringComparison.OrdinalIgnoreCase));
            if (tt == null)
            {
                return;
            }

            var constraint = new DatabaseConstraint
            {
                ConstraintType = ConstraintType.Check,
                SchemaOwner = schema,
                TableName = typeName,
                Name = record.GetString("name"), //name is always system generated
                Expression = record.GetString("definition"),
            };
            tt.CheckConstraints.Add(constraint);
        }
    }
}