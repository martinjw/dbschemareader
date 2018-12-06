using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;
using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.PostgreSql
{
    internal class ServerVersion : SqlExecuter<int>
    {
        public ServerVersion(int? commandTimeout) : base(commandTimeout, null)
        {
        }

        protected override void AddParameters(DbCommand command)
        {
        }

        protected override void Mapper(IDataRecord record)
        {
        }

        public int Execute(IConnectionAdapter connectionAdapter)
        {
            try
            {
                //server_version_num available from 8.2 +
                var cmd = BuildCommand(connectionAdapter);
                cmd.CommandText = @"SELECT current_setting('server_version_num')"; //or SHOW server_version
                //bizarrely, although this is version in a numeric format, it comes back as a string
                return int.Parse((string)cmd.ExecuteScalar(), NumberStyles.Any);
            }
            catch (Exception exception)
            {
                //possibly older than 8.2
                Trace.TraceError("Error reading postgresql version " + exception);
                return 1;
            }
        }
    }
}