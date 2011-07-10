using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Conversion
{
    static class SchemaProcedureConverter
    {

        public static List<DatabaseSequence> Sequences(DataTable dt)
        {
            List<DatabaseSequence> list = new List<DatabaseSequence>();
            //oracle
            string key = "SEQUENCE_NAME";
            string ownerKey = "SEQUENCE_OWNER";
            string minValueKey = "MIN_VALUE";
            string maxValueKey = "MAX_VALUE";
            string incrementKey = "INCREMENT_BY";
            //DDTek.Oracle is different
            if (!dt.Columns.Contains(ownerKey)) ownerKey = "SEQUENCE_SCHEMA";
            //Devart.Data.Oracle
            if (!dt.Columns.Contains(key)) key = "NAME";
            if (!dt.Columns.Contains(ownerKey)) ownerKey = "SCHEMA";
            if (!dt.Columns.Contains(minValueKey)) minValueKey = "MINVALUE";
            if (!dt.Columns.Contains(maxValueKey)) maxValueKey = "MAXVALUE";
            if (!dt.Columns.Contains(incrementKey)) incrementKey = "INCREMENTBY";
            //Firebird generators
            if (!dt.Columns.Contains(ownerKey)) ownerKey = "GENERATOR_SCHEMA";
            if (!dt.Columns.Contains(key)) key = "GENERATOR_NAME";
            //Devart.Data.PostgreSql
            if (!dt.Columns.Contains(minValueKey)) minValueKey = null;
            if (!dt.Columns.Contains(maxValueKey)) maxValueKey = null;
            if (!dt.Columns.Contains(incrementKey)) incrementKey = null;


            foreach (DataRow row in dt.Rows)
            {
                DatabaseSequence seq = new DatabaseSequence();
                seq.Name = row[key].ToString();
                seq.SchemaOwner = row[ownerKey].ToString();
                if (!string.IsNullOrEmpty(minValueKey))
                    seq.MinimumValue = GetNullableDecimal(row[minValueKey]);
                if (!string.IsNullOrEmpty(maxValueKey))
                    seq.MaximumValue = GetNullableDecimal(row[maxValueKey]);
                if (!string.IsNullOrEmpty(incrementKey))
                    seq.IncrementBy = GetNullableInt(row[incrementKey]) ?? 1;
                list.Add(seq);
            }
            return list;
        }

        public static List<DatabaseFunction> Functions(DataTable dt)
        {
            List<DatabaseFunction> list = new List<DatabaseFunction>();
            //oracle
            string key = "OBJECT_NAME";
            string ownerKey = "OWNER";
            string sqlKey = "SQL";
            string langKey = "LANGUAGE";
            string returnKey = "RETURNTYPE";
            //devart
            if (!dt.Columns.Contains(key)) key = "NAME";
            if (!dt.Columns.Contains(ownerKey)) ownerKey = "SCHEMA";
            if (!dt.Columns.Contains(sqlKey)) sqlKey = "BODY";
            //firebird
            if (!dt.Columns.Contains(key)) key = "FUNCTION_NAME";
            if (!dt.Columns.Contains(ownerKey)) ownerKey = "FUNCTION_SCHEMA";
            if (!dt.Columns.Contains(returnKey)) returnKey = "RETURN_ARGUMENT";
            //other
            if (!dt.Columns.Contains(ownerKey)) ownerKey = null;
            if (!dt.Columns.Contains(sqlKey)) sqlKey = null;
            if (!dt.Columns.Contains(langKey)) langKey = null;
            if (!dt.Columns.Contains(returnKey)) returnKey = null;

            foreach (DataRow row in dt.Rows)
            {
                DatabaseFunction fun = new DatabaseFunction();
                fun.Name = row[key].ToString();
                if (!string.IsNullOrEmpty(ownerKey))
                    fun.SchemaOwner = row[ownerKey].ToString();
                if (sqlKey != null) fun.Sql = row[sqlKey].ToString();
                if (langKey != null) fun.Language = row[langKey].ToString();
                if (returnKey != null) fun.ReturnType = row[returnKey].ToString();
                list.Add(fun);
            }
            return list;
        }

        public static void StoredProcedures(DatabaseSchema schema, DataTable dt)
        {
            //sql server
            string key = "ROUTINE_NAME";
            string ownerKey = "ROUTINE_SCHEMA";
            string routineTypeKey = "ROUTINE_TYPE";
            if (!dt.Columns.Contains(routineTypeKey)) routineTypeKey = "PROCEDURE_TYPE_NAME";
            if (!dt.Columns.Contains(routineTypeKey)) routineTypeKey = null;
            //oracle
            if (!dt.Columns.Contains(key)) key = "OBJECT_NAME";
            if (!dt.Columns.Contains(ownerKey)) ownerKey = "OWNER";
            string packageKey = "PACKAGE_NAME";
            if (!dt.Columns.Contains(packageKey)) packageKey = null; //sql
            //jet (and firebird)
            if (!dt.Columns.Contains(key)) key = "PROCEDURE_NAME";
            if (!dt.Columns.Contains(ownerKey)) ownerKey = "PROCEDURE_SCHEMA";
            string sql = "PROCEDURE_DEFINITION";
            if (!dt.Columns.Contains(sql)) sql = "ROUTINE_DEFINITION"; //MySql
            if (!dt.Columns.Contains(sql)) sql = "SOURCE"; //firebird
            if (!dt.Columns.Contains(sql)) sql = null;
            //Devart.Data.Oracle
            if (!dt.Columns.Contains(key)) key = "NAME";
            if (!dt.Columns.Contains(ownerKey)) ownerKey = "SCHEMA";
            if (packageKey == null && dt.Columns.Contains("PACKAGE")) packageKey = "PACKAGE";
            if (!dt.Columns.Contains(ownerKey)) ownerKey = "DATABASE";
            //Intersystems Cache
            if (!dt.Columns.Contains(ownerKey)) ownerKey = "PROCEDURE_SCHEM";
            var isDb2 = dt.Columns.Contains("PROCEDURE_MODULE");

            foreach (DataRow row in dt.Rows)
            {
                string name = row[key].ToString();
                string schemaOwner = row[ownerKey].ToString();
                if (isDb2)
                {
                    //ignore db2 system sprocs
                    if (IsDb2SystemSchema(schemaOwner)) continue;
                }
                bool isFunction = IsFunction(routineTypeKey, row);
                string package = null;
                if (packageKey != null)
                {
                    package = row[packageKey].ToString();
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
                if (sql != null) sproc.Sql = row[sql].ToString();
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

        public static void UpdateArguments(DatabaseSchema databaseSchema, DataTable arguments)
        {
            if (arguments.Columns.Count == 0) return; //empty datatable

            //sql server
            string sprocKey = "SPECIFIC_NAME";
            string ordinalKey = "ORDINAL_POSITION";
            string ownerKey = "SPECIFIC_SCHEMA";

            //oracle
            if (!arguments.Columns.Contains(sprocKey)) sprocKey = "OBJECT_NAME";
            if (!arguments.Columns.Contains(ordinalKey)) ordinalKey = "POSITION";
            if (!arguments.Columns.Contains(ownerKey)) ownerKey = "OWNER";
            string packageKey = "PACKAGE_NAME";
            if (!arguments.Columns.Contains(packageKey)) packageKey = null; //sql

            //oledb and firebird
            if (!arguments.Columns.Contains(sprocKey)) sprocKey = "PROCEDURE_NAME";
            if (!arguments.Columns.Contains(ownerKey)) ownerKey = "PROCEDURE_SCHEMA";

            //Devart.Data.Oracle
            if (!arguments.Columns.Contains(sprocKey)) sprocKey = "PROCEDURE";
            if (!arguments.Columns.Contains(ownerKey)) ownerKey = "SCHEMA";
            if (packageKey == null && arguments.Columns.Contains("PACKAGE")) packageKey = "PACKAGE";

            //Devart.Data.PostgreSql
            if (!arguments.Columns.Contains(sprocKey)) sprocKey = "ROUTINE";

            //Intersystems.Cache
            if (!arguments.Columns.Contains(ownerKey)) ownerKey = "PROCEDURE_OWNER";
            if (!arguments.Columns.Contains(ordinalKey)) ordinalKey = null;

            var isDb2 = arguments.Columns.Contains("PROCEDURE_MODULE");

            //project the sprocs (which won't have packages) into a distinct view
            DataTable sprocTable;
            using (DataView sprocNames = new DataView(arguments))
            {
                sprocNames.Sort = sprocKey;
                if (packageKey == null)
                    sprocTable = sprocNames.ToTable(true, sprocKey, ownerKey); //distinct
                else
                    sprocTable = sprocNames.ToTable(true, sprocKey, ownerKey, packageKey);
            }

            //go thru all sprocs with arguments- if not in sproc list, add it
            foreach (DataRow row in sprocTable.Rows)
            {
                string name = row[sprocKey].ToString();
                //a procedure without a name?
                if (string.IsNullOrEmpty(name)) continue;
                string owner = row[ownerKey].ToString();
                if (isDb2)
                {
                    //ignore db2 system sprocs
                    if (IsDb2SystemSchema(owner)) continue;
                }

                string package = null; //for non-Oracle, package is always null
                if (packageKey != null)
                {
                    package = row[packageKey].ToString();
                    if (string.IsNullOrEmpty(package)) package = null; //so we can match easily
                }

                using (DataView dv = new DataView(arguments))
                {
                    //match sproc name and schema
                    dv.RowFilter = string.Format(CultureInfo.InvariantCulture, "[{0}] = '{1}' AND [{2}] = '{3}'",
                                                 sprocKey, name, ownerKey, owner);
                    if (!string.IsNullOrEmpty(ordinalKey))
                        dv.Sort = ordinalKey;
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
            //oracle
            string ownerKey = "SPECIFIC_SCHEMA";
            string sprocName = "SPECIFIC_NAME";
            string name = "PARAMETER_NAME";
            string inoutKey = "PARAMETER_MODE";
            string datatypeKey = "DATA_TYPE";
            string packageKey = "PACKAGE_NAME";
            string ordinalKey = "ORDINAL_POSITION";
            string lengthKey = "CHARACTER_MAXIMUM_LENGTH";
            string precisionKey = "NUMERIC_PRECISION";
            string scaleKey = "NUMERIC_SCALE";

            //sql server
            if (!arguments.Columns.Contains(sprocName)) sprocName = "OBJECT_NAME";
            if (!arguments.Columns.Contains(ownerKey)) ownerKey = "OWNER";
            if (!arguments.Columns.Contains(name)) name = "ARGUMENT_NAME";
            if (!arguments.Columns.Contains(inoutKey)) inoutKey = "IN_OUT";
            if (!arguments.Columns.Contains(packageKey)) packageKey = null;
            if (!arguments.Columns.Contains(ordinalKey)) ordinalKey = "POSITION";
            if (!arguments.Columns.Contains(lengthKey)) lengthKey = "DATA_LENGTH";
            if (!arguments.Columns.Contains(precisionKey)) precisionKey = "DATA_PRECISION";
            if (!arguments.Columns.Contains(scaleKey)) scaleKey = "DATA_SCALE";

            //Devart.Data.Oracle
            if (!arguments.Columns.Contains(name)) name = "NAME";
            if (!arguments.Columns.Contains(sprocName)) sprocName = "PROCEDURE";
            if (!arguments.Columns.Contains(ownerKey)) ownerKey = "SCHEMA";
            if (packageKey == null && arguments.Columns.Contains("PACKAGE")) packageKey = "PACKAGE";
            if (!arguments.Columns.Contains(datatypeKey)) datatypeKey = "DATATYPE";
            if (!arguments.Columns.Contains(precisionKey)) precisionKey = "PRECISION";
            if (!arguments.Columns.Contains(lengthKey)) lengthKey = "LENGTH";
            if (!arguments.Columns.Contains(scaleKey)) scaleKey = "SCALE";
            if (!arguments.Columns.Contains(inoutKey)) inoutKey = "DIRECTION";

            //oledb and firebird and db2
            if (!arguments.Columns.Contains(sprocName)) sprocName = "PROCEDURE_NAME";
            if (!arguments.Columns.Contains(ownerKey)) ownerKey = "PROCEDURE_SCHEMA";
            //firebird
            if (!arguments.Columns.Contains(datatypeKey)) datatypeKey = "PARAMETER_DATA_TYPE";
            if (!arguments.Columns.Contains(inoutKey)) inoutKey = "PARAMETER_DIRECTION";
            if (!arguments.Columns.Contains(lengthKey)) lengthKey = "CHARACTER_MAX_LENGTH";
            if (!arguments.Columns.Contains(precisionKey)) precisionKey = "NUMERIC_PRECISION";
            if (!arguments.Columns.Contains(scaleKey)) scaleKey = "NUMERIC_SCALE";
            //db2
            if (!arguments.Columns.Contains(name)) name = "COLUMN_NAME";
            if (arguments.Columns.Contains("PROVIDER_TYPE_NAME")) datatypeKey = "PROVIDER_TYPE_NAME";
            string db2ColumnTypeKey = null;
            if (arguments.Columns.Contains("COLUMN_TYPE_NAME")) db2ColumnTypeKey = "COLUMN_TYPE_NAME";
            if (!arguments.Columns.Contains(lengthKey)) lengthKey = "COLUMN_SIZE";
            if (!arguments.Columns.Contains(precisionKey)) precisionKey = "COLUMN_SIZE";
            if (!arguments.Columns.Contains(scaleKey)) scaleKey = "DECIMAL_DIGITS";

            //Intersystems.Cache
            if (!arguments.Columns.Contains(ownerKey)) ownerKey = "PROCEDURE_OWNER";
            if (!arguments.Columns.Contains(ordinalKey)) ordinalKey = null;
            if (arguments.Columns.Contains("TYPE_NAME")) datatypeKey = "TYPE_NAME";

            //not provided
            if (!arguments.Columns.Contains(precisionKey)) precisionKey = null;
            if (!arguments.Columns.Contains(lengthKey)) lengthKey = null;
            if (!arguments.Columns.Contains(scaleKey)) scaleKey = null;
            if (!arguments.Columns.Contains(inoutKey)) inoutKey = null;

            foreach (DataRowView row in dataView)
            {
                var argName = row[name].ToString();
                //check if it's already there
                var argument = AddArgumentToList(list, argName);

                argument.ProcedureName = row[sprocName].ToString();
                argument.SchemaOwner = row[ownerKey].ToString();
                AddPackage(row, packageKey, argument);
                if (!string.IsNullOrEmpty(ordinalKey))
                    argument.Ordinal = Convert.ToDecimal(row[ordinalKey], CultureInfo.CurrentCulture);

                argument.DatabaseDataType = row[datatypeKey].ToString();
                AddInOut(row, inoutKey, argument);
                if (db2ColumnTypeKey != null) ApplyColumnType((string)row[db2ColumnTypeKey], argument);

                //Oracle: these can be decimals, but we'll assume ints
                if (lengthKey != null)
                    argument.Length = GetNullableInt(row[lengthKey]);
                if (precisionKey != null)
                    argument.Precision = GetNullableInt(row[precisionKey]);
                if (scaleKey != null)
                    argument.Scale = GetNullableInt(row[scaleKey]);
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
