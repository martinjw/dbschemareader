using System.Data.Common;
#if NETSTANDARD1_5
using System.Reflection;
#endif
using System.Text.RegularExpressions;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Oracle
{
    abstract class OracleSqlExecuter<T> : SqlExecuter<T> where T : new()
    {
        /// <summary>
        /// The database version.
        /// </summary>
        private int? _version;

        /// <summary>
        /// Parse out the server version (9, 10, 11 or 12, hopefully)
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns></returns>
        protected int? Version(DbConnection connection)
        {
            if (!_version.HasValue)
            {
                var version = connection.ServerVersion;
                var match = Regex.Match(version, @"\b(\d+)(?=\D)");
                _version = int.Parse(match.Value);
            }
            return _version;
        }


        /// <summary>
        /// When overriding in Oracle, use base.AddParameters(command) or EnsureOracleBindByName(command)
        /// </summary>
        /// <param name="command">The command.</param>
        protected override void AddParameters(DbCommand command)
        {
            EnsureOracleBindByName(command);
        }

        protected void EnsureOracleBindByName(DbCommand command)
        {
            //Oracle.DataAccess.Client only binds first parameter match unless BindByName=true
            //so we violate LiskovSP (in reflection to avoid dependency on ODP)
#if NETSTANDARD1_5
            var bindByName = command.GetType().GetTypeInfo().GetDeclaredProperty("BindByName");
#else
            //netstandard 2.0 and traditional .net
            var bindByName = command.GetType().GetProperty("BindByName");
#endif           
            if (bindByName != null)
            {
                bindByName.SetValue(command, true, null);
            }
        }
    }
}
