using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.Databases;
using SqlServerSchemaReader.Schema;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace SqlServerSchemaReader.SchemaReaders
{
    /// <summary>
    /// Read primary/unique constraints for user defined table types (UDTs)
    /// </summary>
    internal class TableTypeConstraintReader : SqlExecuter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TableTypeConstraintReader"/> class.
        /// </summary>
        public TableTypeConstraintReader()
        {
            Sql = @"SELECT
SCHEMA_NAME(tt.schema_id) AS schema_name,
tt.name AS TYPE_NAME, 
con.name,
con.type,
c.name AS column_name,
ic.key_ordinal 
FROM sys.table_types tt
INNER JOIN sys.key_constraints con
   ON con.parent_object_id = tt.type_table_object_id
INNER JOIN sys.index_columns ic
   ON ic.object_id = tt.type_table_object_id
   AND ic.index_id = con.unique_index_id
INNER JOIN sys.columns c
   ON c.object_id = tt.type_table_object_id 
   AND ic.column_id = c.column_id
WHERE (SCHEMA_NAME(tt.schema_id) = @schema OR @schema IS NULL)
ORDER BY tt.name, con.name, con.type, ic.key_ordinal
";
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

            var conType = record.GetString("type");
            var ct = string.Equals("PK", conType, StringComparison.OrdinalIgnoreCase)
                ? ConstraintType.PrimaryKey
                : ConstraintType.UniqueKey;
            var conName = record.GetString("name"); //name is always system generated
            var constraint = new DatabaseConstraint
            {
                ConstraintType = ct,
                SchemaOwner = schema,
                TableName = typeName,
                Name = conName,
            };
            //find if it's already there
            if (ct == ConstraintType.PrimaryKey)
            {
                var con = tt.PrimaryKey;
                if (con == null) tt.PrimaryKey = constraint;
                else constraint = con;
            }
            if (ct == ConstraintType.UniqueKey)
            {
                var con = tt.UniqueKeys.Find(x => string.Equals(conName, x.Name, StringComparison.OrdinalIgnoreCase));
                if (con == null) tt.UniqueKeys.Add(constraint);
                else constraint = con;
            }
            //we're in key_ordinal order so this is correct
            var colName = record.GetString("column_name");
            constraint.Columns.Add(colName);
            tt.UpdateConstraintColumns();
        }
    }
}