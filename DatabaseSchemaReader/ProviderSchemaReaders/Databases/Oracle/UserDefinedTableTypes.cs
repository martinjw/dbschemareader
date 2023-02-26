using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Oracle
{
    internal class UserDefinedTableTypes : SqlExecuter<UserDefinedTable>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServer.UserDefinedTableTypes"/> class.
        /// </summary>
        public UserDefinedTableTypes(int? commandTimeout, string owner) : base(commandTimeout, owner)
        {
            Sql = @"SELECT
    at.OWNER,
    at.TYPE_NAME,
    at.TYPECODE,
    ata.ATTR_NAME,
    ata.ATTR_TYPE_NAME,
    ata.LENGTH,
    ata.PRECISION,
    ata.SCALE,
    ata.ATTR_NO,
    atc.ELEM_TYPE_NAME
FROM
    ALL_TYPES      at
    LEFT OUTER JOIN ALL_TYPE_ATTRS ata
        ON at.OWNER = ata.OWNER AND at.TYPE_NAME = ata.TYPE_NAME
    LEFT OUTER JOIN ALL_COLL_TYPES atc
        ON at.OWNER = atc.OWNER AND at.TYPE_NAME = atc.TYPE_NAME
WHERE
(at.OWNER = :schemaOwner OR :schemaOwner IS NULL)
ORDER BY at.OWNER,at.TYPE_NAME,ata.ATTR_NO";
            //consider CHAR_USED (C or B)
            //
        }

        /// <summary>
        /// Use this for schema level (i.e. all tables)
        /// </summary>
        public IList<UserDefinedTable> Execute(IConnectionAdapter connectionAdapter)
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
            AddDbParameter(command, "schemaOwner", Owner);
        }

        /// <summary>
        /// Map the result ADO record to the result.
        /// </summary>
        /// <param name="record">The record.</param>
        protected override void Mapper(IDataRecord record)
        {
            var schema = record["OWNER"].ToString();
            var typeName = record["TYPE_NAME"].ToString();
            var tt =
                Result.FirstOrDefault(
                t => string.Equals(t.Name, typeName, StringComparison.OrdinalIgnoreCase) &&
                  string.Equals(t.SchemaOwner, schema, StringComparison.OrdinalIgnoreCase));
            if (tt == null)
            {
                var typeCode = record["TYPECODE"].ToString();
                var elementName = record["ELEM_TYPE_NAME"].ToString();
                tt = new UserDefinedTable
                {
                    SchemaOwner = schema,
                    Name = typeName,
                    IsCollectionType = string.Equals("COLLECTION", typeCode, StringComparison.OrdinalIgnoreCase),
                    CollectionTypeName = elementName,
                };
                Result.Add(tt);
            }

            var colName = record.GetString("ATTR_NAME");
            if (colName != null)
            {
                var col = new DatabaseColumn
                {
                    Name = colName,
                    SchemaOwner = schema,
                    TableName = typeName,
                    DbDataType = record.GetString("ATTR_TYPE_NAME"),
                    Length = record.GetNullableInt("LENGTH"),
                    Precision = record.GetNullableInt("PRECISION"),
                    Scale = record.GetNullableInt("SCALE"),
                    Ordinal = record.GetInt("ATTR_NO"),
                };
                tt.Columns.Add(col);
            }
        }
    }
}