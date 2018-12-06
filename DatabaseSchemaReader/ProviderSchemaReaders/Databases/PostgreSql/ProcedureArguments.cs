using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.PostgreSql
{
    internal class ProcedureArguments : SqlExecuter<DatabaseArgument>
    {
        private readonly string _name;
        private readonly Dictionary<long, List<DatabaseArgument>> _requiredDataTypes;

        public ProcedureArguments(int? commandTimeout, string owner, string name) : base(commandTimeout, owner)
        {
            _name = name;
            Owner = owner;
            //pg_proc.proargtypes: An array with the data types of the function arguments. This includes only input arguments (including INOUT arguments), and thus represents the call signature of the function
            //pg_proc.proallargtypes: An array with the data types of the function arguments. This includes all arguments (including OUT and INOUT arguments); however, if all the arguments are IN arguments, this field will be null. Subscripting is 1-based, whereas for historical reasons proargtypes is subscripted from 0
            //pg_proc.proargmodes: An array with the modes of the function arguments, encoded as i for IN arguments, o for OUT arguments, b for INOUT arguments. If all the arguments are IN arguments, this field will be null. Subscripts correspond to positions of proallargtypes not proargtypes
            //pg_proc.proargnames	An array with the names of the function arguments. Arguments without a name are set to empty strings in the array. If none of the arguments have a name, this field will be null. Subscripts correspond to positions of proallargtypes not proargtypes

            Sql = @"SELECT 
ns.nspname AS SCHEMA, 
pr.proname AS NAME, 
tp.typname AS RETURNTYPE, 
pr.proargtypes as INARGS,
pr.proallargtypes as ALLARGS,
pr.proargmodes as ARGMODES,
pr.proargnames as ARGNAMES
  FROM pg_proc pr
LEFT OUTER JOIN pg_type tp ON tp.oid = pr.prorettype
INNER JOIN pg_namespace ns ON pr.pronamespace = ns.oid
  WHERE --proisagg = FALSE AND 
   tp.typname <> 'trigger'
   AND ns.nspname NOT LIKE 'pg_%'
   AND ns.nspname != 'information_schema'
   AND (ns.nspname = :schemaOwner OR :schemaOwner IS NULL)
   AND (pr.proname = :sprocName OR :sprocName IS NULL) 
  ORDER BY pr.proname";

            _requiredDataTypes = new Dictionary<long, List<DatabaseArgument>>();

        }

        public IList<DatabaseArgument> Execute(IConnectionAdapter connectionAdapter)
        {
            _requiredDataTypes.Clear();
            ExecuteDbReader(connectionAdapter);

            //now lookup datatypes
            foreach (var key in _requiredDataTypes.Keys)
            {
                var typeName = LookUpTypes(connectionAdapter, key);
                foreach (var databaseArgument in _requiredDataTypes[key])
                {
                    databaseArgument.DatabaseDataType = typeName;
                }
            }
            _requiredDataTypes.Clear();
            return Result;
        }

        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "schemaOwner", Owner);
            AddDbParameter(command, "sprocName", _name);
        }

        protected override void Mapper(IDataRecord record)
        {
            var allArgs = ReadLongArray(record["ALLARGS"]);
            if (allArgs.Length == 0)
            {
                //there may be just inputparameters
                var inargs = record["INARGS"];
                // it might be an array of uints
                if (inargs is uint[])
                {
                    allArgs = ((uint[])inargs).Select(i => (long)i).ToArray();
                }
                else
                {
                    var s = ReadString(inargs);
                    //inargs is space delimited.
                    if (!string.IsNullOrEmpty(s))
                        s = s.Replace(' ', ',');
                    allArgs = StringToLongArray(s);
                }
            }
            //there are no arguments for this procedure
            if (allArgs.Length == 0) return;

            var schema = ReadString(record["SCHEMA"]);
            var procedure = ReadString(record["NAME"]);
            var modes = ReadArray(record["ARGMODES"]);
            var argNames = ReadArray(record["ARGNAMES"]);

            //create a list of all the argument types we need to look up
            for (int index = 0; index < allArgs.Length; index++)
            {
                //get the name (they may not be named)
                string argName = string.Empty;
                if (index < argNames.Length) argName = argNames[index];
                //Postgresql allows parameters to have no names ($1, $2...).
                //That may be difficult for clients so we'll invent a name
                if (string.IsNullOrEmpty(argName)) argName = "arg" + index;

                string direction;
                if (index >= modes.Length) direction = "IN";
                else direction = ParseDirection(modes[index]);

                var arg = new DatabaseArgument
                {
                    SchemaOwner = schema,
                    ProcedureName = procedure,
                    Name = argName,
                    Ordinal = index,
                };
                if (direction.Contains("IN")) arg.In = true;
                //can be INOUT
                if (direction.Contains("OUT")) arg.Out = true;
                var typeOid = allArgs[index];
                if (!_requiredDataTypes.ContainsKey(typeOid))
                {
                    _requiredDataTypes.Add(typeOid, new List<DatabaseArgument>());
                }
                _requiredDataTypes[typeOid].Add(arg);

                Result.Add(arg);
            }
        }

        private static string ParseDirection(string s)
        {
            switch (s)
            {
                case "b":
                    return "INOUT";
                case "o":
                    return "OUT";
                default:
                    return "IN";
            }
        }

        private string LookUpTypes(IConnectionAdapter connectionAdapter, long id)
        {
            const string sqlCommand =
                @"SELECT pg_catalog.format_type(:oid, NULL)";
            using (var cmd = BuildCommand(connectionAdapter))
            {
                cmd.CommandText = sqlCommand;
                AddDbParameter(cmd, "oid", id);
                return (string)cmd.ExecuteScalar();
            }
        }

        private static long[] ReadLongArray(object o)
        {
            var ar = o as long[];
            if (ar != null) return ar;
            if (o is long) return new[] { (long)o };
            return new long[] { };
        }

        private static long[] StringToLongArray(string s)
        {
            if (string.IsNullOrEmpty(s)) return new long[] { };
            var sa = s.Split(',');
            var result = new long[sa.Length];
            for (var index = 0; index < sa.Length; index++)
            {
                var value = sa[index];
                if (!string.IsNullOrEmpty(value))
                {
                    long l;
                    if (long.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out l))
                    {
                        result[index] = l;
                    }
                }
            }
            return result;
        }

        private static string[] ReadArray(object o)
        {
            var ar = o as string[];
            if (ar != null) return ar;
            var s = ReadString(o);
            if (s == null) return new string[] { };
            s = s.Trim(new[] { '{', '}' });
            return s.Split(',');
        }

        private static string ReadString(object o)
        {
            return o == DBNull.Value ? null : o.ToString();
        }
    }
}