using System.Data.Common;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Extenders.SqlServer
{
    /// <summary>
    /// Extender for the SqlServer schema. 
    /// </summary>
    /// <seealso cref="DatabaseSchemaReader.Extenders.IExtendSchema" />
    public class SqlServerSchemaExtender : IExtendSchema
    {
        /// <summary>
        /// Add additional information to database schema.
        /// </summary>
        /// <param name="databaseSchema"></param>
        /// <param name="connection"></param>
        public void Execute(DatabaseSchema databaseSchema, DbConnection connection)
        {
            var schema = databaseSchema as SqlServerSchema;
            if (schema == null) return;

            //grab memory optimized flag
            var hr = new HekatonReader();
            hr.Execute(schema, connection);

            //grab statistics
            var sr = new StatisticsReader(schema.Owner, null);
            var stats = sr.Execute(connection);
            foreach (var stat in stats)
            {
                var table = schema.FindTableByName(stat.TableName, stat.SchemaOwner) as SqlServerTable;
                table?.DatabaseStatistics.Add(stat);
            }
        }
    }
}