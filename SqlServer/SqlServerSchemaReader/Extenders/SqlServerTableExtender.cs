using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.Extenders;
using SqlServerSchemaReader.Schema;

namespace SqlServerSchemaReader.Extenders
{
    /// <summary>
    /// Extender for SqlServer tables
    /// </summary>
    /// <seealso cref="DatabaseSchemaReader.Extenders.IExtendTable" />
    public class SqlServerTableExtender : IExtendTable
    {
        /// <summary>
        /// Add additional information to database table.
        /// </summary>
        /// <param name="databaseTable"></param>
        /// <param name="connection"></param>
        public void Execute(DatabaseTable databaseTable, DbConnection connection)
        {
            var sqlTable = databaseTable as SqlServerTable;
            if (sqlTable == null) return;

            //we actually do this in IExtendSchema, but could be done here (once per table)
            //var sr = new StatisticsReader(databaseTable.SchemaOwner, databaseTable.Name);
            //var stats = sr.Execute(connection);
            //sqlTable.DatabaseStatistics.AddRange(stats);
        }
    }
}