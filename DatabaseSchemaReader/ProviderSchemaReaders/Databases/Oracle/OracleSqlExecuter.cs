using System.Data.Common;
#if NETSTANDARD1_5
using System.Reflection;
#endif
using System.Text.RegularExpressions;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Oracle
{
    abstract class OracleSqlExecuter<T> : SqlExecuter<T> where T : new()
    {
        protected OracleSqlExecuter(int? commandTimeout, string owner) : base(commandTimeout, owner)
        {
        }

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
            var typeInfo = command.GetType().GetTypeInfo();
            var bindByName = typeInfo.GetDeclaredProperty("BindByName");
            var initialLongFetchSize = typeInfo.GetDeclaredProperty("InitialLONGFetchSize");
#else
            //netstandard 2.0 and traditional .net
            var type = command.GetType();
            var bindByName = type.GetProperty("BindByName");
            var initialLongFetchSize = type.GetProperty("InitialLONGFetchSize");
#endif           
            if (bindByName != null)
            {
                bindByName.SetValue(command, true, null);
            }
            if (initialLongFetchSize != null)
            {
                initialLongFetchSize.SetValue(command, -1, null);
            }
        }
    }
}
