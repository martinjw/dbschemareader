using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.Extenders;
using SqlServerSchemaReader.Schema;
using SqlServerSchemaReader.SchemaReaders;
using System.Data.Common;
using System.Linq;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace SqlServerSchemaReader.Extenders
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
        public void Execute(DatabaseSchema databaseSchema, IConnectionAdapter connection)
        {
            var schema = databaseSchema as SqlServerSchema;
            if (schema == null) return;

            //look for UDTs
            var tr = new AliasTypeReader();
            tr.Execute(schema, connection);
            var hasUdts = false;
            if (schema.AliasTypes.Any())
            {
                new AliasTypeColumnReader().Execute(schema, connection);
                hasUdts = true;
            }

            var ttr = new TableTypeReader();
            ttr.Execute(schema, connection);
            if (schema.TableTypes.Any())
            {
                //if we have table types, look up constraints for them
                new TableTypeConstraintReader().Execute(schema, connection);
                new TableTypeCheckReader().Execute(schema, connection);
                hasUdts = true;
            }
            if (hasUdts)
            {
                //applies to alias types and table types
                new UdtParameterReader().Execute(schema, connection);
            }

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