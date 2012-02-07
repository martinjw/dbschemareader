using System;
using System.Globalization;
using System.Linq;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.CodeGen.Procedures
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
        private readonly SprocLogic _logic;
        private readonly SprocResultType _sprocResultType;

        public SprocWriter(DatabaseStoredProcedure storedProcedure, string ns)
        {
            _namespace = ns;
            _storedProcedure = storedProcedure;
            _logic = new SprocLogic(_storedProcedure);
            _sprocResultType = _logic.ResultType;
            _cb = new ClassBuilder();
        }

        public bool HasResultClass
        {
            get { return _sprocResultType != SprocResultType.Void; }
        }

        public bool RequiresOracleReference
        {
            get { return _logic.HasRefCursors; }
        }

        public string Write()
        {
            WriteTop();

            WriteClass();

            WriteEnd();

            return _cb.ToString();
        }

        internal string WriteWithResultClass()
        {
            WriteTop();

            WriteClass();

            if (HasResultClass)
            {
                var rs = new SprocResultWriter(_storedProcedure, _namespace, _cb);
                rs.WriteClasses();
            }

            WriteEnd();

            return _cb.ToString();
        }

        private void WriteTop()
        {
            WriteNamespaces();

            if (!string.IsNullOrEmpty(_namespace))
            {
                _cb.BeginNest("namespace " + _namespace);
            }
        }

        private void WriteEnd()
        {
            if (!string.IsNullOrEmpty(_namespace))
            {
                _cb.EndNest();
            }
        }

        private void WriteClass()
        {
            var className = _logic.ClassName;

            using (_cb.BeginNest("public class " + className, "Class representing " + _storedProcedure.FullName + " stored procedure"))
            {
                WriteConstructor(className);
                WriteCreateCommand();
                WriteAddWithValue();

                WriteExecute();
            }
        }

        private void WriteNamespaces()
        {
            _cb.AppendLine("using System;");
            if (_storedProcedure.ResultSets.Count > 0)
            {
                _cb.AppendLine("using System.Collections.Generic;");
            }
            _cb.AppendLine("using System.Data;");
            _cb.AppendLine("using System.Data.Common;");
            //if it has Oracle refcursors
            if (_logic.HasRefCursors)
            {
                //could also be ODP, Devart etc.
                _cb.AppendLine("using System.Data.OracleClient; //contains a Ref Cursor");
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
            string argList = _logic.CreateArgumentList();

            using (_cb.BeginNest("public virtual DbCommand CreateCommand(" + argList + ")", "Creates the command and all parameters."))
            {
                _cb.AppendLine("var cmd = _connection.CreateCommand();");
                var fullName = _storedProcedure.FullName;
                _cb.AppendLine("cmd.CommandText = @\"" + fullName + "\";");
                _cb.AppendLine("cmd.CommandType = CommandType.StoredProcedure;");

                foreach (var argument in _storedProcedure.Arguments)
                {
                    WriteArgument(argument);
                }

                _cb.AppendLine("return cmd;");
            }
            _cb.AppendLine("//var cmd = sproc.CreateCommand(" + _logic.CreateDummyCall() + ");");
        }

        private void WriteExecute()
        {
            var resultClassName = _logic.ResultClassName;
            var returnType = _logic.ReturnType;

            string argList = _logic.CreateArgumentList();
            var call = _logic.CreateArgumentCall();

            using (_cb.BeginNest("public virtual " + returnType + " Execute(" + argList + ")",
                              "Executes the stored procedure"))
            {
                CreateResultClass(resultClassName);
                using (_cb.BeginNest("using (var cmd = CreateCommand(" + call + "))"))
                {
                    WriteExecuteBody();
                }
                if (_sprocResultType != SprocResultType.Void)
                {
                    _cb.AppendLine("return result;");
                }
            }
            if (_sprocResultType == SprocResultType.Enumerable)
            {
                WriteSingleReadData(resultClassName, _storedProcedure.ResultSets[0]);
                WriteFixNull();
            }
            else if (_sprocResultType == SprocResultType.ResultClass)
            {
                WriteReadData(resultClassName);
                WriteFixNull();
            }

            if (_logic.HasOutputParameters)
            {
                WriteOutputParameters(resultClassName);
            }
        }

        private void CreateResultClass(string resultClassName)
        {
            if (_sprocResultType == SprocResultType.Void)
            {
                //no result
            }
            else if (_sprocResultType == SprocResultType.Enumerable)
            {
                //return a simple enumerable of results
                //We cannot tell is this is a scalar (singular) result
                _cb.AppendLine("var result = new List<" + resultClassName + ">();");
            }
            else //if (sprocResultType == SprocResultType.ResultClass)
            {
                //return a result class containing the multiple result lists
                _cb.AppendLine("var result = new " + resultClassName + "();");
            }
        }

        private void WriteExecuteBody()
        {
            var numberResults = _storedProcedure.ResultSets.Count;

            _cb.AppendLine("var isClosed = (_connection.State == ConnectionState.Closed);");
            _cb.AppendLine("if (isClosed) _connection.Open();");
            using (_cb.BeginNest("try"))
            {
                if (numberResults == 0)
                {
                    _cb.AppendLine("cmd.ExecuteNonQuery();");
                }
                else //if (numberResults > 0)
                {
                    using (_cb.BeginNest("using (var rdr = cmd.ExecuteReader())"))
                    {
                        _cb.AppendLine("ReadData(rdr, result);");
                    }
                }
                //if has output parameters (after reader is closed)
                if (_logic.HasOutputParameters)
                {
                    _cb.AppendLine("UpdateOutputParameters(cmd, result);");
                }
            }
            using (_cb.BeginNest("finally"))
            {
                _cb.AppendLine("if (isClosed) _connection.Close();");
            }
        }

        private void WriteOutputParameters(string resultClassName)
        {
            using (_cb.BeginNest("private static void UpdateOutputParameters(IDbCommand cmd, " + resultClassName + " result)"))
            {
                foreach (var argument in _storedProcedure.Arguments)
                {
                    if (!argument.Out) continue;
                    //gets rid of REF CURSORS
                    if (argument.DataType == null) continue;
                    var dataType = argument.DataType.NetDataTypeCSharpName;
                    if (!argument.DataType.IsString) dataType += "?";
                    _cb.AppendLine("result." + argument.NetName + " = (" + dataType + ")((DbParameter)cmd.Parameters[\"" + argument.Name + "\"]).Value;");
                }
            }
        }

        private void WriteSingleReadData(string resultClassName, DatabaseResultSet result)
        {
            //single result set
            using (_cb.BeginNest("private static void ReadData(IDataReader rdr, ICollection<" + resultClassName + "> result)"))
            {
                using (_cb.BeginNest("while (rdr.Read())"))
                {
                    _cb.AppendLine("var record = new " + resultClassName + "();");
                    for (int index = 0; index < result.Columns.Count; index++)
                    {
                        var column = result.Columns[index];
                        var name = column.NetName ?? NameFixer.ToPascalCase(column.Name);
                        var dataType = column.DbDataType;
                        if (!string.Equals(dataType, "String", StringComparison.OrdinalIgnoreCase) && !dataType.EndsWith("[]", StringComparison.OrdinalIgnoreCase))
                        {
                            dataType += "?"; //nullable
                        }
                        //manage DbNull
                        _cb.AppendLine("record." + name + " = (" + dataType + ")FixNull(rdr[" + index + "]);");
                    }
                    _cb.AppendLine("result.Add(record);");
                }
            }
        }


        private void WriteReadData(string resultClassName)
        {
            if (_storedProcedure.ResultSets.Count == 0) return;

            using (_cb.BeginNest("private static void ReadData(IDataReader rdr, " + resultClassName + " result)"))
            {
                for (int index = 0; index < _storedProcedure.ResultSets.Count; index++)
                {
                    var name = resultClassName + index;
                    _cb.AppendLine("ReadData(rdr, result." + name + ");");
                    _cb.AppendLine("rdr.NextResult();");
                }
            }
            for (int index = 0; index < _storedProcedure.ResultSets.Count; index++)
            {
                var resultSet = _storedProcedure.ResultSets[index];
                var name = resultClassName + index;
                WriteSingleReadData(name, resultSet);
            }
        }

        private void WriteArgument(DatabaseArgument argument)
        {
            _cb.AppendLine("");
            _cb.AppendLine("//" + argument.Name + " " + argument.DatabaseDataType);
            string s = string.Format(CultureInfo.InvariantCulture,
                                     "AddWithValue(cmd, \"{0}\", {1});",
                                     argument.Name, SprocLogic.ArgumentCamelCaseName(argument));
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

        private void WriteFixNull()
        {
            if (_storedProcedure.ResultSets.Count == 0) return;

            using (_cb.BeginNest("private static object FixNull(object value)", "Change DBNull values to null"))
            {
                _cb.AppendLine("return (value == DBNull.Value) ? null : value;");
            }
        }
        private void WriteAddWithValue()
        {
            if (_storedProcedure.Arguments.Count == 0) return;
            //this only applies to input parameters
            if (!_storedProcedure.Arguments.Any(x => x.In)) return;

            //if you have a lot of sprocs, this belongs in a base class or extension class
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
