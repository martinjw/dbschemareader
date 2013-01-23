using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using DatabaseSchemaReader.Conversion.KeyMaps;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.Filters;

namespace DatabaseSchemaReader.Conversion
{
    class SchemaProcedureConverter
    {

        public static List<DatabaseSequence> Sequences(DataTable dt)
        {
            List<DatabaseSequence> list = new List<DatabaseSequence>();

            var sequenceKeyMap = new SequenceKeyMap(dt);

            foreach (DataRow row in dt.Rows)
            {
                DatabaseSequence seq = new DatabaseSequence();
                seq.Name = row[sequenceKeyMap.Key].ToString();
                seq.SchemaOwner = row[sequenceKeyMap.OwnerKey].ToString();
                if (!string.IsNullOrEmpty(sequenceKeyMap.MinValueKey))
                    seq.MinimumValue = GetNullableDecimal(row[sequenceKeyMap.MinValueKey]);
                if (!string.IsNullOrEmpty(sequenceKeyMap.MaxValueKey))
                    seq.MaximumValue = GetNullableDecimal(row[sequenceKeyMap.MaxValueKey]);
                if (!string.IsNullOrEmpty(sequenceKeyMap.IncrementKey))
                    seq.IncrementBy = GetNullableInt(row[sequenceKeyMap.IncrementKey]) ?? 1;
                list.Add(seq);
            }
            return list;
        }

        public static List<DatabaseFunction> Functions(DataTable dt)
        {
            List<DatabaseFunction> list = new List<DatabaseFunction>();


            var functionKeyMap = new FunctionKeyMap(dt);
            foreach (DataRow row in dt.Rows)
            {
                DatabaseFunction fun = new DatabaseFunction();
                fun.Name = row[functionKeyMap.Key].ToString();
                if (!string.IsNullOrEmpty(functionKeyMap.OwnerKey))
                    fun.SchemaOwner = row[functionKeyMap.OwnerKey].ToString();
                if (functionKeyMap.SqlKey != null) fun.Sql = row[functionKeyMap.SqlKey].ToString();
                if (functionKeyMap.LangKey != null) fun.Language = row[functionKeyMap.LangKey].ToString();
                if (functionKeyMap.ReturnKey != null) fun.ReturnType = row[functionKeyMap.ReturnKey].ToString();
                list.Add(fun);
            }
            return list;
        }

        public static void StoredProcedures(DatabaseSchema schema, DataTable dt)
        {
            var storedProcedureKeyMap = new StoredProcedureKeyMap(dt);

            foreach (DataRow row in dt.Rows)
            {
                string name = row[storedProcedureKeyMap.Key].ToString();
                string schemaOwner = row[storedProcedureKeyMap.OwnerKey].ToString();
                if (storedProcedureKeyMap.IsDb2)
                {
                    //ignore db2 system sprocs
                    if (IsDb2SystemSchema(schemaOwner)) continue;
                }
                bool isFunction = IsFunction(storedProcedureKeyMap.RoutineTypeKey, row);
                string package = null;
                if (storedProcedureKeyMap.PackageKey != null)
                {
                    package = row[storedProcedureKeyMap.PackageKey].ToString();
                    if (string.IsNullOrEmpty(package)) package = null; //so we can match easily
                }

                //check if already loaded (so can call this function multiple times)
                DatabaseStoredProcedure sproc = FindStoredProcedureOrFunction(schema, name, schemaOwner, package);
                if (sproc == null)
                {
                    sproc = CreateProcedureOrFunction(schema, isFunction);
                    sproc.Name = name;
                    sproc.SchemaOwner = schemaOwner;
                    sproc.Package = package;
                }
                if (storedProcedureKeyMap.Sql != null) sproc.Sql = row[storedProcedureKeyMap.Sql].ToString();
            }
        }

        private static bool IsDb2SystemSchema(string schemaOwner)
        {
            if (schemaOwner.Equals("SYSPROC", StringComparison.OrdinalIgnoreCase)) return true;
            if (schemaOwner.Equals("SYSFUN", StringComparison.OrdinalIgnoreCase)) return true;
            if (schemaOwner.Equals("SQLJ", StringComparison.OrdinalIgnoreCase)) return true;
            if (schemaOwner.StartsWith("SYSIBM", StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        private static bool IsFunction(string routineTypeKey, DataRow row)
        {
            bool isFunction = false;
            if (!string.IsNullOrEmpty(routineTypeKey))
            {
                var type = row[routineTypeKey].ToString();
                if (string.Equals(type, "FUNCTION", StringComparison.OrdinalIgnoreCase))
                    isFunction = true;
                else if (string.Equals(type, "SQL_PT_FUNCTION", StringComparison.OrdinalIgnoreCase))
                    isFunction = true;
            }
            return isFunction;
        }

        public IFilter StoredProcedureFilter { get; set; }
        public IFilter PackageFilter { get; set; }

        public void UpdateArguments(DatabaseSchema databaseSchema, DataTable arguments)
        {
            if (arguments.Columns.Count == 0) return; //empty datatable

            var argumentsKeyMap = new ArgumentsKeyMap(arguments);

            bool hasPackage = !string.IsNullOrEmpty(argumentsKeyMap.PackageKey);

            //project the sprocs (which won't have packages) into a distinct view
            DataTable sprocTable;
            using (DataView sprocNames = new DataView(arguments))
            {
                sprocNames.Sort = argumentsKeyMap.SprocName;
                if (!hasPackage)
                    sprocTable = sprocNames.ToTable(true, argumentsKeyMap.SprocName, argumentsKeyMap.OwnerKey); //distinct
                else
                    sprocTable = sprocNames.ToTable(true, argumentsKeyMap.SprocName, argumentsKeyMap.OwnerKey, argumentsKeyMap.PackageKey);
            }

            var sprocFilter = StoredProcedureFilter;
            var packFilter = PackageFilter;
            //go thru all sprocs with arguments- if not in sproc list, add it
            foreach (DataRow row in sprocTable.Rows)
            {
                string name = row[argumentsKeyMap.SprocName].ToString();
                //a procedure without a name?
                if (string.IsNullOrEmpty(name)) continue;
                if (sprocFilter != null && sprocFilter.Exclude(name)) continue;

                string owner = row[argumentsKeyMap.OwnerKey].ToString();
                if (argumentsKeyMap.IsDb2)
                {
                    //ignore db2 system sprocs
                    if (IsDb2SystemSchema(owner)) continue;
                }

                string package = null; //for non-Oracle, package is always null
                if (hasPackage)
                {
                    package = row[argumentsKeyMap.PackageKey].ToString();
                    if (string.IsNullOrEmpty(package)) package = null; //so we can match easily
                    else if (packFilter != null && packFilter.Exclude(package)) continue;
                }

                using (DataView dv = new DataView(arguments))
                {
                    //match sproc name and schema
                    dv.RowFilter = string.Format(CultureInfo.InvariantCulture, "[{0}] = '{1}' AND ISNULL([{2}],'') = '{3}'",
                                                 argumentsKeyMap.SprocName, name, argumentsKeyMap.OwnerKey, owner);
                    if (!string.IsNullOrEmpty(argumentsKeyMap.OrdinalKey))
                        dv.Sort = argumentsKeyMap.OrdinalKey;
                    List<DatabaseArgument> args = StoredProcedureArguments(dv);

                    DatabaseStoredProcedure sproc = FindStoredProcedureOrFunction(databaseSchema, name, owner, package);

                    if (sproc == null) //sproc in a package and not found before?
                    {
                        sproc = CreateProcedureOrFunction(databaseSchema, args);
                        sproc.Name = name;
                        sproc.SchemaOwner = owner;
                        sproc.Package = package;
                    }
                    sproc.Arguments.AddRange(args);
                }
            }
        }

        private static DatabaseStoredProcedure CreateProcedureOrFunction(DatabaseSchema databaseSchema, bool isFunction)
        {
            DatabaseStoredProcedure sproc;
            if (isFunction)
            {
                //functions are just a type of stored procedure
                DatabaseFunction fun = new DatabaseFunction();
                databaseSchema.Functions.Add(fun);
                sproc = fun;
            }
            else
            {
                sproc = new DatabaseStoredProcedure();
                databaseSchema.StoredProcedures.Add(sproc);
            }
            return sproc;
        }

        private static DatabaseStoredProcedure CreateProcedureOrFunction(DatabaseSchema databaseSchema, List<DatabaseArgument> args)
        {
            //if it's ordinal 0 and no name, it's a function not a sproc
            DatabaseStoredProcedure sproc;
            if (args.Find(delegate(DatabaseArgument arg) { return arg.Ordinal == 0 && string.IsNullOrEmpty(arg.Name); }) != null)
            {
                //functions are just a type of stored procedure
                DatabaseFunction fun = new DatabaseFunction();
                databaseSchema.Functions.Add(fun);
                sproc = fun;
            }
            else
            {
                sproc = new DatabaseStoredProcedure();
                databaseSchema.StoredProcedures.Add(sproc);
            }
            return sproc;
        }

        private static DatabaseStoredProcedure FindStoredProcedureOrFunction(DatabaseSchema databaseSchema, string name, string owner, string package)
        {
            var sproc = databaseSchema.StoredProcedures.Find(delegate(DatabaseStoredProcedure x) { return x.Name == name && x.SchemaOwner == owner && x.Package == package; });
            if (sproc == null) //is it actually a function?
            {
                DatabaseFunction fun = databaseSchema.Functions.Find(delegate(DatabaseFunction f) { return f.Name == name && f.SchemaOwner == owner && f.Package == package; });
                if (fun != null)
                {
                    return fun;
                }
            }
            return sproc;
        }

        private static List<DatabaseArgument> StoredProcedureArguments(DataView dataView)
        {
            DataTable arguments = dataView.Table;
            List<DatabaseArgument> list = new List<DatabaseArgument>();


            var argumentsKeyMap = new ArgumentsKeyMap(arguments);

            foreach (DataRowView row in dataView)
            {
                var argName = row[argumentsKeyMap.ParameterName].ToString();
                //check if it's already there
                var argument = AddArgumentToList(list, argName);

                argument.ProcedureName = row[argumentsKeyMap.SprocName].ToString();
                argument.SchemaOwner = row[argumentsKeyMap.OwnerKey].ToString();
                AddPackage(row, argumentsKeyMap.PackageKey, argument);
                if (!string.IsNullOrEmpty(argumentsKeyMap.OrdinalKey))
                    argument.Ordinal = Convert.ToDecimal(row[argumentsKeyMap.OrdinalKey], CultureInfo.CurrentCulture);

                argument.DatabaseDataType = row[argumentsKeyMap.DatatypeKey].ToString();
                AddInOut(row, argumentsKeyMap.InoutKey, argument);
                if (argumentsKeyMap.Db2ColumnTypeKey != null) ApplyColumnType((string)row[argumentsKeyMap.Db2ColumnTypeKey], argument);

                //Oracle: these can be decimals, but we'll assume ints
                if (argumentsKeyMap.LengthKey != null)
                    argument.Length = GetNullableInt(row[argumentsKeyMap.LengthKey]);
                if (argumentsKeyMap.PrecisionKey != null)
                    argument.Precision = GetNullableInt(row[argumentsKeyMap.PrecisionKey]);
                if (argumentsKeyMap.ScaleKey != null)
                    argument.Scale = GetNullableInt(row[argumentsKeyMap.ScaleKey]);
            }
            return list;
        }

        private static void ApplyColumnType(string columnType, DatabaseArgument argument)
        {
            switch (columnType)
            {
                case "SQL_PARAM_INPUT":
                    argument.In = true;
                    return;
                case "SQL_PARAM_INPUT_OUTPUT":
                    argument.In = true;
                    argument.Out = true;
                    return;
                case "SQL_PARAM_OUTPUT":
                    argument.Out = true;
                    return;
                case "SQL_RETURN_VALUE":
                    argument.Ordinal = -1;
                    return;
                case "SQL_RESULT_COL":
                    //db2 returns result columns?
                    break;
            }
        }

        private static void AddPackage(DataRowView row, string packageKey, DatabaseArgument argument)
        {
            if (packageKey != null) argument.PackageName = row[packageKey].ToString();
        }

        private static void AddInOut(DataRowView row, string inoutKey, DatabaseArgument argument)
        {
            if (inoutKey == null) return;
            string inout = row[inoutKey].ToString();
            if (inout.Contains("IN")) argument.In = true;
            else if (inout.Contains("OUT")) argument.Out = true;
        }

        private static DatabaseArgument AddArgumentToList(List<DatabaseArgument> list, string argName)
        {
            var existing = list.Find(delegate(DatabaseArgument arg) { return arg.Name == argName; });
            if (existing == null)
            {
                DatabaseArgument argument = new DatabaseArgument();
                argument.Name = argName;
                list.Add(argument);
                return argument;
            }
            return existing;
        }

        public static List<DatabasePackage> Packages(DataTable dt)
        {
            List<DatabasePackage> list = new List<DatabasePackage>();
            if (dt.Rows.Count == 0) return list;

            //oracle and ODP
            string key = "OBJECT_NAME";
            string ownerKey = "OWNER";
            //Devart.Data.Oracle
            if (!dt.Columns.Contains(key)) key = "NAME";
            if (!dt.Columns.Contains(ownerKey)) ownerKey = "SCHEMA";

            foreach (DataRow row in dt.Rows)
            {
                DatabasePackage package = new DatabasePackage();
                package.Name = row[key].ToString();
                package.SchemaOwner = row[ownerKey].ToString();
                list.Add(package);
            }
            return list;
        }

        private static int? GetNullableInt(object o)
        {
            try
            {
                return (o != DBNull.Value) ? Convert.ToInt32(o, CultureInfo.CurrentCulture) : (int?)null;
            }
            catch (OverflowException)
            {
                //this occurs for blobs and clobs using the OleDb provider
                return -1;
            }
        }

        private static decimal? GetNullableDecimal(object o)
        {
            return (o != DBNull.Value) ? Convert.ToDecimal(o, CultureInfo.CurrentCulture) : (decimal?)null;
        }
    }
}
