using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.CodeGen.Procedures
{
    class SprocResultWriter
    {
        private readonly DatabaseStoredProcedure _storedProcedure;
        private readonly string _namespace;
        private readonly ClassBuilder _cb;
        private readonly SprocLogic _logic;
        private readonly string _resultClassName;

        public SprocResultWriter(DatabaseStoredProcedure storedProcedure, string ns)
        {
            _namespace = ns;
            _storedProcedure = storedProcedure;
            _logic = new SprocLogic(_storedProcedure);
            _resultClassName = _logic.ResultClassName;
            _cb = new ClassBuilder();
        }
        internal SprocResultWriter(DatabaseStoredProcedure storedProcedure, string ns, ClassBuilder classBuilder)
        {
            _namespace = ns;
            _storedProcedure = storedProcedure;
            _logic = new SprocLogic(_storedProcedure);
            _resultClassName = _logic.ResultClassName;
            _cb = classBuilder;
        }
        public string ClassName { get { return _resultClassName; } }

        public string Write()
        {
            WriteNamespaces();

            if (!string.IsNullOrEmpty(_namespace))
            {
                _cb.BeginNest("namespace " + _namespace);
            }

            WriteClasses();

            if (!string.IsNullOrEmpty(_namespace))
            {
                _cb.EndNest();
            }

            return _cb.ToString();
        }

        /// <summary>
        /// Writes the classes. This is exposed to SprocWriter so it doesn't include usings/namespaces
        /// </summary>
        internal void WriteClasses()
        {
            using (_cb.BeginNest("public class " + _resultClassName, "Class representing result of " + _storedProcedure.FullName + " stored procedure"))
            {
                if (_logic.ResultType == SprocResultType.Enumerable)
                {
                    var result = _storedProcedure.ResultSets[0];
                    WriteProperties(result);
                    AddToString(result.Columns);
                }
                else
                {
                    WriteMultiResultSet();
                }
            }

            if (_logic.ResultType == SprocResultType.ResultClass)
            {
                WriteMultiResultSetClasses();
            }
        }

        private void WriteMultiResultSetClasses()
        {
            for (int i = 0; i < _storedProcedure.ResultSets.Count; i++)
            {
                var result = _storedProcedure.ResultSets[i];
                var name = result.NetName ?? _resultClassName + i;
                using (_cb.BeginNest("public class " + name, "Result set " + i + " for " + _logic.ClassName))
                {
                    WriteProperties(result);
                    AddToString(result.Columns);
                }
            }
        }


        private void WriteMultiResultSet()
        {
            //constructor
            if (_storedProcedure.ResultSets.Count > 0)
            {
                using (_cb.BeginNest("public " + _resultClassName + "()"))
                {
                    for (int i = 0; i < _storedProcedure.ResultSets.Count; i++)
                    {
                        var rs = _storedProcedure.ResultSets[i];
                        var name = rs.NetName ?? _resultClassName + i;
                        var dataType = "List<" + name + ">();";
                        _cb.AppendLine(name + " = new " + dataType);
                    }
                }
            }

            //properties
            for (int i = 0; i < _storedProcedure.ResultSets.Count; i++)
            {
                var rs = _storedProcedure.ResultSets[i];
                var name = rs.NetName ?? _resultClassName + i;
                var dataType = "IList<" + name + ">";
                _cb.AppendAutomaticCollectionProperty(dataType, name);
            }

            //output parameters
            foreach (var argument in _storedProcedure.Arguments)
            {
                if (!argument.Out) continue;
                //gets rid of REF CURSORS
                if (argument.DataType == null) continue;
                var dataType = argument.DataType.NetDataTypeCSharpName;
                if (!argument.DataType.IsString) dataType += "?";
                _cb.AppendAutomaticProperty(dataType, argument.NetName);
           }
        }

        private void WriteProperties(DatabaseResultSet result)
        {
            foreach (var column in result.Columns)
            {
                if (string.IsNullOrEmpty(column.NetName))
                {
                    column.NetName = NameFixer.ToPascalCase(column.Name);
                }
                var dataType = TranslateDataTypeToCSharp(column.DbDataType);

                if (!string.Equals(dataType, "String", StringComparison.OrdinalIgnoreCase) &&
                    !dataType.EndsWith("[]", StringComparison.OrdinalIgnoreCase))
                {
                    dataType += "?"; //nullable
                }
                _cb.AppendAutomaticProperty(dataType, column.NetName);
            }
        }

        private static string TranslateDataTypeToCSharp(string dataType)
        {
            //these are generally of the form "System.String"
            switch (dataType.ToUpperInvariant())
            {
                case "STRING":
                    return "string";
                case "INT32":
                    return "int";
                case "INT64":
                    return "long";
                case "INT16":
                    return "short";
                case "DECIMAL":
                    return "decimal";
                case "BYTE":
                    return "byte";
                case "BYTE[]":
                    return "byte[]";
                case "CHAR":
                    return "char";
                case "BOOLEAN":
                    return "bool";
                case "SINGLE":
                    return "float";
                default:
                    return dataType;
            }
        }

        private void AddToString(IList<DatabaseColumn> columns)
        {
            if (columns.Count() == 0) return;
            var column = columns[0];
            using (_cb.BeginNest("public override string ToString()"))
            {
                var line = string.Format(
                    CultureInfo.InvariantCulture, 
                    "return \"[{0}] = \" + {0};", 
                    column.NetName);
                _cb.AppendLine(line);
            }
        }

        private void WriteNamespaces()
        {
            _cb.AppendLine("using System;");
            if (_logic.ResultType == SprocResultType.ResultClass)
            {
                _cb.AppendLine("using System.Collections.Generic;");
            }
        }
    }
}
