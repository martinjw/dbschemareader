using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.SqlServer
{
    internal class UserDataTypes : SqlExecuter<UserDataType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserDataTypes"/> class.
        /// </summary>
        public UserDataTypes(int? commandTimeout, string owner) : base(commandTimeout, owner)
        {
            Sql = @"SELECT
SCHEMA_NAME(st.schema_id) AS schema_name,
st.name AS TYPE_NAME,
st.max_length,
st.precision,
st.scale,
st.is_nullable,
st2.name AS BaseName
FROM sys.types st
JOIN sys.types st2 ON st.system_type_id = st2.system_type_id AND st2.is_user_defined = 0
WHERE
    st2.name <> 'sysname'
AND st.is_user_defined = 1
AND st.is_table_type = 0
AND (SCHEMA_NAME(st.schema_id) = @schema OR @schema IS NULL)
ORDER BY st.name
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
            var schema = record["schema_name"].ToString();
            var typeName = record["TYPE_NAME"].ToString();
            var tt = new UserDataType
            {
                SchemaOwner = schema,
                Name = typeName,
                MaxLength = record.GetNullableInt("max_length"),
                Precision = record.GetNullableInt("precision"),
                Scale = record.GetNullableInt("scale"),
                Nullable = record.GetBoolean("is_nullable"),
                DbTypeName = record.GetString("BaseName"),
            };
            Result.Add(tt);
        }
    }
}