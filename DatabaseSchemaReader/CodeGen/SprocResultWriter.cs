using System;
using System.Collections.Generic;
using System.Linq;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.CodeGen
{
    class SprocResultWriter
    {
        private readonly DatabaseStoredProcedure _storedProcedure;
        private readonly string _namespace;
        private readonly ClassBuilder _cb;

        public SprocResultWriter(DatabaseStoredProcedure storedProcedure, string ns)
        {
            _namespace = ns;
            _storedProcedure = storedProcedure;
            _cb = new ClassBuilder();
        }


        public string Write()
        {
            var className = _storedProcedure.NetName ?? (_storedProcedure.NetName = NameFixer.ToPascalCase(_storedProcedure.Name));
            var fullName = _storedProcedure.SchemaOwner + "." + _storedProcedure.Name;

            WriteNamespaces();

            if (!string.IsNullOrEmpty(_namespace))
            {
                _cb.BeginNest("namespace " + _namespace);
            }

            var resultClassName = className + "Result";
            using (_cb.BeginNest("public class " + resultClassName, "Class representing result of " + fullName + " stored procedure"))
            {
                if (_storedProcedure.ResultSets.Count == 1)
                {
                    var result = _storedProcedure.ResultSets[0];
                    WriteProperties(result);
                    AddToString(result.Columns);
                }
                else
                {
                    WriteMultiResultSet(resultClassName);
                }
            }

            if (_storedProcedure.ResultSets.Count > 1)
            {
                WriteMultiResultSetClasses(resultClassName);
            }

            if (!string.IsNullOrEmpty(_namespace))
            {
                _cb.EndNest();
            }

            return _cb.ToString();
        }

        private void WriteMultiResultSetClasses(string resultClassName)
        {
            for (int i = 0; i < _storedProcedure.ResultSets.Count; i++)
            {
                var result = _storedProcedure.ResultSets[i];
                using (_cb.BeginNest("public class " + resultClassName + i, "Result set " + i + " for " + _storedProcedure.Name))
                {
                    WriteProperties(result);
                    AddToString(result.Columns);
                }
            }
        }


        private void WriteMultiResultSet(string resultClassName)
        {
            using (_cb.BeginNest("public " + resultClassName + "()"))
            {
                for (int i = 0; i < _storedProcedure.ResultSets.Count; i++)
                {
                    var name = resultClassName + i;
                    var dataType = "List<" + name + ">();";
                    _cb.AppendLine(name + " = new " + dataType);
                }
            }

            for (int i = 0; i < _storedProcedure.ResultSets.Count; i++)
            {
                var dataType = "IList<" + (resultClassName + i) + ">";
                _cb.AppendAutomaticCollectionProperty(dataType, resultClassName + i);
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
                var dataType = column.DbDataType;
                if (!string.Equals(dataType, "String", StringComparison.OrdinalIgnoreCase) &&
                    !dataType.EndsWith("[]", StringComparison.OrdinalIgnoreCase))
                {
                    dataType += "?"; //nullable
                }
                _cb.AppendAutomaticProperty(dataType, column.NetName);
            }
        }

        private void AddToString(IList<DatabaseColumn> columns)
        {
            if (columns.Count() == 0) return;
            var column = columns[0];
            using (_cb.BeginNest("public override string ToString()"))
            {
                var line = "return \"[" + column.NetName + "] = \" + " + column.NetName + ";";
                _cb.AppendLine(line);
            }
        }

        private void WriteNamespaces()
        {
            _cb.AppendLine("using System;");
            if (_storedProcedure.ResultSets.Count > 1)
            {
                _cb.AppendLine("using System.Collections.Generic;");
            }
        }
    }
}
