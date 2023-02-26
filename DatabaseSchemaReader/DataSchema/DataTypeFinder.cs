using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// Assigns <see cref="DataType"/> based on the db type names
    /// </summary>
    internal class DataTypeFinder
    {
        private readonly IDictionary<string, DataType> _dataTypes;
        private readonly IList<UserDataType> _userDefined;
        private readonly List<UserDefinedTable> _userDefinedTables;

        public DataTypeFinder(DatabaseSchema databaseSchema)
            : this(databaseSchema.DataTypes)
        {
            //check if no datatypes loaded
            _userDefined = databaseSchema.UserDataTypes;
            _userDefinedTables = databaseSchema.UserDefinedTables;
        }

        public DataTypeFinder(IList<DataType> types)
        {
            if (types.Count == 0) return;
            //quickly lookup the datatypes
            var dataTypes = new Dictionary<string, DataType>();
            foreach (DataType type in types)
            {
                //just in case there are duplicate names
                if (!dataTypes.ContainsKey(type.TypeName)) dataTypes.Add(type.TypeName, type);
            }
            _dataTypes = dataTypes;
        }

        public DataType Find(string dbDataType)
        {
            //quick lookup in dictionary, otherwise has to loop thru

            if (string.IsNullOrEmpty(dbDataType)) return null;
            if (_dataTypes == null) return null;

            DataType dt;
            if (_dataTypes.TryGetValue(dbDataType, out dt)) return dt;

            //try without (n)
            var unbraced = Regex.Replace(dbDataType, "\\([^\\)]*\\)", string.Empty);
            if (_dataTypes.TryGetValue(unbraced, out dt)) return dt;

            var brace = dbDataType.IndexOf('(');
            if (brace > 1)
            {
                dbDataType = dbDataType.Substring(0, brace).ToUpperInvariant();
                if (_dataTypes.TryGetValue(dbDataType, out dt)) return dt;
            }

            //TIMESTAMP(9) from Oracle == Timestamp
            dt = _dataTypes.Values.FirstOrDefault(dataType => dbDataType.StartsWith(dataType.TypeName, StringComparison.OrdinalIgnoreCase));

            int i;
            if (dt == null && int.TryParse(dbDataType, NumberStyles.Integer, CultureInfo.InvariantCulture, out i))
                dt = _dataTypes.Values.FirstOrDefault(dataType => i.Equals(dataType.ProviderDbType));

            //look up a udt
            var udt = _userDefined?.FirstOrDefault(x => string.Equals(x.Name, dbDataType, StringComparison.OrdinalIgnoreCase));
            if (udt != null) return udt.DataType;

            return dt;
        }

        public void UpdateArguments(DatabaseStoredProcedure databaseStoredProcedure)
        {
            foreach (DatabaseArgument arg in databaseStoredProcedure.Arguments)
            {
                arg.DataType = Find(arg.DatabaseDataType);
                if (arg.DataType == null && _userDefinedTables != null)
                {
                    arg.UserDefinedTable = _userDefinedTables.FirstOrDefault(x =>
                        string.Equals(x.Name, arg.DatabaseDataType, StringComparison.OrdinalIgnoreCase));
                }
            }
        }
    }
}