using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.CodeGen
{
    class SprocWriter
    {
        /*
         * This creates a simple class which sets all the parameters on a DbCommand.
         * You must write the code to create the connection
         * AND to execute the DbCommand (we can't tell if it's ExecuteReader, ExecuteNonQuery etc)
         * 
         * All parameters are implicitly nullable.
         * 
         * This supports simple SQLServer and Oracle parameters.
         * It also supports Oracle REF CURSOR.
         * It does NOT support LOBs and other special database types.
         */
        private readonly DatabaseStoredProcedure _storedProcedure;
        private readonly string _namespace;
        private readonly ClassBuilder _cb;

        public SprocWriter(DatabaseStoredProcedure storedProcedure, string ns)
        {
            _namespace = ns;
            _storedProcedure = storedProcedure;
            _cb = new ClassBuilder();
        }

        public string Write()
        {
            var className = _storedProcedure.NetName ?? (_storedProcedure.NetName = NameFixer.ToPascalCase(_storedProcedure.Name));

            WriteNamespaces();

            if (!string.IsNullOrEmpty(_namespace))
            {
                _cb.BeginNest("namespace " + _namespace);
            }

            var fullName = _storedProcedure.SchemaOwner + "." + _storedProcedure.Name;
            using (_cb.BeginNest("public class " + className, "Class representing " + fullName + " stored procedure"))
            {
                WriteConstructor(className);
                WriteCreateCommand();
                WriteAddWithValue();
            }

            if (!string.IsNullOrEmpty(_namespace))
            {
                _cb.EndNest();
            }

            return _cb.ToString();
        }

        private void WriteNamespaces()
        {
            _cb.AppendLine("using System;");
            _cb.AppendLine("using System.Data;");
            _cb.AppendLine("using System.Data.Common;");
            //if it has Oracle refcursors
            if (_storedProcedure.Arguments.Any(argument => string.Equals("REF CURSOR", argument.DatabaseDataType, StringComparison.OrdinalIgnoreCase)))
            {
                //could also be ODP, Devart etc.
                _cb.AppendLine("using System.Data.OracleClient;");
            }
            _cb.AppendLine("using System.Diagnostics;");
        }

        private void WriteConstructor(string className)
        {
            _cb.AppendLine("[DebuggerBrowsable(DebuggerBrowsableState.Never)]");
            _cb.AppendLine("private readonly DbConnection _connection;");
            _cb.AppendLine("");

            using (_cb.BeginNest("public " + className + "(DbConnection connection)", "Initializes a new instance of the <see cref=\"" + className + "\"/> class."))
            {
                _cb.AppendLine("_connection = connection;");
            }
            _cb.AppendLine("");
        }


        private void WriteCreateCommand()
        {
            string argList = CreateArgumentList();

            using (_cb.BeginNest("public DbCommand CreateCommand(" + argList + ")", "Creates the command and all parameters."))
            {
                _cb.AppendLine("var cmd = _connection.CreateCommand();");
                var fullName = _storedProcedure.Name;
                if (!string.IsNullOrEmpty(_storedProcedure.Package))
                {
                    //prefix with package name
                    fullName = _storedProcedure.Package + "." + fullName;
                }
                if (!_storedProcedure.SchemaOwner.Equals("dbo"))
                {
                    //prefix with schema name
                    fullName = _storedProcedure.SchemaOwner + "." + fullName;
                }
                _cb.AppendLine("cmd.CommandText = @\"" + fullName + "\";");
                _cb.AppendLine("cmd.CommandType = CommandType.StoredProcedure;");

                foreach (var argument in _storedProcedure.Arguments)
                {
                    WriteArgument(argument);
                }

                _cb.AppendLine("return cmd;");
            }
            _cb.AppendLine("//var cmd = sproc.CreateCommand(" + CreateDummyCall() + ");");
        }

        private void WriteArgument(DatabaseArgument argument)
        {
            _cb.AppendLine("");
            _cb.AppendLine("//" + argument.Name + " " + argument.DatabaseDataType);
            string s = string.Format(CultureInfo.InvariantCulture,
                                     "AddWithValue(cmd, \"{0}\", {1});",
                                     argument.Name, argument.NetName);
            if (!argument.Out)
            {
                //normal in parameters
                _cb.AppendLine(s);
                return;
            }

            // output and input-output parameters.
            var pName = "p" + argument.NetName;
            var isRefCursor = string.Equals("REF CURSOR", argument.DatabaseDataType, StringComparison.OrdinalIgnoreCase);
            if (argument.In)
            {
                //input output
                _cb.AppendLine("var " + pName + " = " + s);
            }
            else
            {
                //just output
                if (isRefCursor)
                {
                    _cb.AppendLine("var " + pName + " = (OracleParameter)cmd.CreateParameter();");
                }
                else
                {
                    _cb.AppendLine("var " + pName + " = cmd.CreateParameter();");
                }
                _cb.AppendLine(pName + ".ParameterName = \"" + argument.Name + "\";");
                _cb.AppendLine("cmd.Parameters.Add(" + pName + ");");
            }
            _cb.AppendLine(pName + ".Direction = ParameterDirection." + (argument.In ? "InputOutput" : "Output") + ";");

            // you may need DbType on output parameters
            if (isRefCursor)
            {
                _cb.AppendLine(pName + ".OracleType = OracleType.Cursor;");
                return;
            }
            var dt = argument.DataType;
            if (dt == null)
            {
                _cb.AppendLine(pName + ".DbType = DbType.Object;");
                return;
            }
            var t = dt.GetNetType();
            _cb.AppendLine(pName + ".DbType = DbType." + Type.GetTypeCode(t) + ";");

        }

        private string CreateArgumentList()
        {
            var args = new List<string>();
            foreach (var argument in _storedProcedure.Arguments)
            {
                if (!argument.In) continue;
                var name = argument.NetName;
                if (string.IsNullOrEmpty(name))
                {
                    name = NameFixer.ToPascalCase(argument.Name);
                    argument.NetName = name;
                }
                var netType = "object";
                var dt = argument.DataType;
                if (dt != null)
                {
                    netType = dt.NetCodeName(argument);
                    if (dt.IsNumeric)
                        netType += "?"; //nullable
                    else if (dt.GetNetType() == typeof(DateTime))
                        netType += "?"; //nullable
                }
                args.Add(netType + " " + name);
            }
            return string.Join(", ", args.ToArray());
        }

        private string CreateDummyCall()
        {
            var args = new List<string>();
            foreach (var argument in _storedProcedure.Arguments)
            {
                if (!argument.In) continue;
                var dt = argument.DataType;
                if (dt == null)
                {
                    args.Add("null");
                    continue;
                }
                if (dt.IsNumeric)
                    args.Add("0");
                else if (dt.IsString)
                    args.Add("\"a\"");
                else if (dt.GetNetType() == typeof(DateTime))
                    args.Add("DateTime.Now");
                else
                    args.Add("null");
            }
            return string.Join(", ", args.ToArray());
        }

        private void WriteAddWithValue()
        {
            if (_storedProcedure.Arguments.Count == 0) return;

            using (_cb.BeginNest("private static DbParameter AddWithValue(DbCommand command, string parameterName, object value)"))
            {
                _cb.AppendLine("var p = command.CreateParameter();");
                _cb.AppendLine("p.ParameterName = parameterName;");
                _cb.AppendLine("p.Value = value ?? DBNull.Value;");
                _cb.AppendLine("command.Parameters.Add(p);");
                _cb.AppendLine("return p;");
            }
            _cb.AppendLine("");

        }
    }
}
