using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;

namespace DatabaseSchemaReader.ProviderSchemaReaders
{
    /// <summary>
    /// Reads the arguments for all functions
    /// </summary>
    class PostgreSqlArgumentReader
    {
        private readonly DbProviderFactory _factory;
        private readonly string _owner;

        public PostgreSqlArgumentReader(DbProviderFactory factory, string owner)
        {
            _owner = owner;
            _factory = factory;
        }

        public DataTable StoredProcedureArguments(string storedProcedureName, DbConnection connection)
        {
            //pg_proc.proargtypes: An array with the data types of the function arguments. This includes only input arguments (including INOUT arguments), and thus represents the call signature of the function
            //pg_proc.proallargtypes: An array with the data types of the function arguments. This includes all arguments (including OUT and INOUT arguments); however, if all the arguments are IN arguments, this field will be null. Subscripting is 1-based, whereas for historical reasons proargtypes is subscripted from 0
            //pg_proc.proargmodes: An array with the modes of the function arguments, encoded as i for IN arguments, o for OUT arguments, b for INOUT arguments. If all the arguments are IN arguments, this field will be null. Subscripts correspond to positions of proallargtypes not proargtypes
            //pg_proc.proargnames	An array with the names of the function arguments. Arguments without a name are set to empty strings in the array. If none of the arguments have a name, this field will be null. Subscripts correspond to positions of proallargtypes not proargtypes
            const string sqlCommand =
                @"SELECT 
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
  WHERE proisagg = FALSE 
   AND tp.typname <> 'trigger'
   AND ns.nspname NOT LIKE 'pg_%'
   AND ns.nspname != 'information_schema'
   AND (ns.nspname = :schemaOwner OR :schemaOwner IS NULL)
   AND (pr.proname = :sprocName OR :sprocName IS NULL) 
  ORDER BY pr.proname";
            using (var dt = CreateDataTable("Data"))
            {
                using (var da = _factory.CreateDataAdapter())
                {
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = sqlCommand;

                        var schemaParameter = AddDbParameter(cmd, "schemaOwner", _owner);
                        schemaParameter.DbType = DbType.String;
                        cmd.Parameters.Add(schemaParameter);

                        var nameParameter = AddDbParameter(cmd, "sprocName", storedProcedureName);
                        nameParameter.DbType = DbType.String;
                        cmd.Parameters.Add(nameParameter);

                        da.SelectCommand = cmd;
                        da.Fill(dt);
                    }
                }
                //turn this into a result
                DataTable result = CreateArgumentsTable();
                var requiredDataTypes = new Dictionary<long, string>();
                foreach (DataRow row in dt.Rows)
                {
                    ParseProcedureRows(row, connection, requiredDataTypes, result);
                }
                return result;
            }
        }

        private static void ParseProcedureRows(DataRow row, DbConnection connection, IDictionary<long, string> requiredDataTypes, DataTable result)
        {
            var allArgs = ReadLongArray(row["ALLARGS"]);
            if (allArgs.Length == 0)
            {
                //there may be just inputparameters
                var s = ReadString(row["INARGS"]);
                //inargs is space delimited.
                if (!string.IsNullOrEmpty(s)) s = s.Replace(' ', ',');
                allArgs = StringToLongArray(s);
            }
            //there are no arguments for this procedure
            if (allArgs.Length == 0) return;

            var schema = ReadString(row["SCHEMA"]);
            var procedure = ReadString(row["NAME"]);
            var modes = ReadArray(row["ARGMODES"]);
            var argNames = ReadArray(row["ARGNAMES"]);

            //create a list of all the argument types we need to look up
            for (int index = 0; index < allArgs.Length; index++)
            {
                var typeName = ParseTypeName(allArgs[index], requiredDataTypes, connection);

                //get the name (they may not be named)
                string argName = string.Empty;
                if (index < argNames.Length) argName = argNames[index];
                //Postgresql allows parameters to have no names ($1, $2...).
                //That may be difficult for clients so we'll invent a name
                if (string.IsNullOrEmpty(argName)) argName = "arg" + index;

                string direction;
                if (index >= modes.Length) direction = "IN";
                else direction = ParseDirection(modes[index]);

                var resultRow = result.NewRow();
                resultRow["SCHEMA"] = schema;
                resultRow["PROCEDURE"] = procedure;
                resultRow["PARAMETER_NAME"] = argName;
                resultRow["DATA_TYPE"] = typeName;
                resultRow["ORDINAL_POSITION"] = index;
                resultRow["IN_OUT"] = direction;
                result.Rows.Add(resultRow);
            }
        }


        private static string ParseTypeName(long oid, IDictionary<long, string> requiredDataTypes, DbConnection connection)
        {
            string typeName;
            if (!requiredDataTypes.ContainsKey(oid))
            {
                typeName = LookUpTypes(connection, oid);
                requiredDataTypes.Add(oid, typeName);
            }
            else
            {
                typeName = requiredDataTypes[oid];
            }
            return typeName;
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

        private static DataTable CreateArgumentsTable()
        {
            var result = CreateDataTable("Arguments");
            result.Columns.Add("SCHEMA", typeof(string));
            result.Columns.Add("PROCEDURE", typeof(string));
            result.Columns.Add("PARAMETER_NAME", typeof(string));
            result.Columns.Add("DATA_TYPE", typeof(string));
            result.Columns.Add("ORDINAL_POSITION", typeof(int));
            result.Columns.Add("IN_OUT", typeof(string));
            return result;
        }

        private static string LookUpTypes(DbConnection connection, long id)
        {
            const string sqlCommand =
                @"SELECT pg_catalog.format_type(:oid, NULL)";
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = sqlCommand;
                var parameter = AddDbParameter(cmd, "oid", id);
                cmd.Parameters.Add(parameter);
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
                    result[index] = Convert.ToInt64(value, CultureInfo.InvariantCulture);
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
        private static DbParameter AddDbParameter(DbCommand command, string parameterName, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.Value = value ?? DBNull.Value;
            return parameter;
        }
        private static DataTable CreateDataTable(string tableName)
        {
            var dt = new DataTable(tableName);
            dt.Locale = CultureInfo.InvariantCulture;
            return dt;
        }
    }
}
