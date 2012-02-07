using System;
using System.Collections.Generic;
using System.Linq;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.CodeGen.Procedures
{
    class SprocLogic
    {
        private readonly DatabaseStoredProcedure _storedProcedure;
        private readonly string _className;
        private bool? _hasOutputParameters;

        public SprocLogic(DatabaseStoredProcedure storedProcedure)
        {
            _storedProcedure = storedProcedure;
            _className = _storedProcedure.NetName ?? (_storedProcedure.NetName = NameFixer.ToPascalCase(_storedProcedure.Name));
        }

        public string ClassName { get { return _className; } }

        public string ResultClassName
        {
            get { return ClassName + "Result"; }
        }
        public SprocResultType ResultType
        {
            get
            {
                var numberResults = _storedProcedure.ResultSets.Count;
                if (numberResults == 0 && !HasOutputParameters)
                {
                    return SprocResultType.Void;
                }
                if (ReturnEnumerable)
                {
                    return SprocResultType.Enumerable;
                }
                return SprocResultType.ResultClass;
            }
        }

        public string ReturnType
        {
            get
            {
                var returnType = ResultClassName;
                var type = ResultType;
                if (type == SprocResultType.Void)
                {
                    return "void";
                }
                if (type == SprocResultType.Enumerable)
                {
                    return "IEnumerable<" + ResultClassName + ">";
                }
                return returnType;
            }
        }

        public bool ReturnEnumerable
        {
            get { return !HasOutputParameters && _storedProcedure.ResultSets.Count == 1; }
        }

        public bool HasRefCursors
        {
            get { return _storedProcedure.Arguments.Any(argument => string.Equals("REF CURSOR", argument.DatabaseDataType, StringComparison.OrdinalIgnoreCase)); }
        }

        public bool HasOutputParameters
        {
            get
            {
                //are there output parameters (apart from RefCursors)?
                if (!_hasOutputParameters.HasValue)
                {
                    _hasOutputParameters = _storedProcedure.Arguments.Any(arg => arg.Out && arg.DatabaseDataType != "REF CURSOR");
                }
                return _hasOutputParameters.Value;
            }
        }

        public static string ArgumentCamelCaseName(DatabaseArgument argument)
        {
            var name = argument.NetName;
            if (string.IsNullOrEmpty(name))
            {
                name = NameFixer.ToPascalCase(argument.Name);
                argument.NetName = name;
            }
            return Char.ToLowerInvariant(name[0])
                + (name.Length > 1 ? name.Substring(1) : string.Empty);
        }

        public string CreateArgumentList()
        {
            var args = new List<string>();
            foreach (var argument in _storedProcedure.Arguments)
            {
                if (!argument.In) continue;
                var name = ArgumentCamelCaseName(argument);
                var netType = "object";
                var dt = argument.DataType;
                if (dt != null)
                {
                    netType = dt.NetCodeName(argument);
                    if (dt.IsNumeric)
                        netType += "?"; //nullable
                    else if (dt.IsDateTime)
                        netType += "?"; //nullable
                }
                args.Add(netType + " " + name);
            }
            return string.Join(", ", args.ToArray());
        }

        public string CreateArgumentCall()
        {
            var args = new List<string>();
            foreach (var argument in _storedProcedure.Arguments)
            {
                if (!argument.In) continue;
                var name = ArgumentCamelCaseName(argument);
                args.Add(name);
            }
            return string.Join(", ", args.ToArray());
        }

        public string CreateDummyCall()
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
                else if (dt.IsDateTime)
                    args.Add("DateTime.Now");
                else
                    args.Add("null");
            }
            return string.Join(", ", args.ToArray());
        }
    }
}
