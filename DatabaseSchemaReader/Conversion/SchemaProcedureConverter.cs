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

            foreach (DataRow row in dt.Rows)
            {
                DatabaseSequence seq = new DatabaseSequence();
                seq.Name = row[key].ToString();
                seq.SchemaOwner = row[ownerKey].ToString();
                seq.MininumValue = GetNullableDecimal(row[minValueKey]);
                seq.MaximumValue = GetNullableDecimal(row[maxValueKey]);
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
            //devart
            if (!dt.Columns.Contains(key)) key = "NAME";
            if (!dt.Columns.Contains(ownerKey)) ownerKey = "SCHEMA";
            //other
            if (!dt.Columns.Contains(ownerKey)) ownerKey = null;
            if (!dt.Columns.Contains(sqlKey)) sqlKey = null;
            foreach (DataRow row in dt.Rows)
            {
                DatabaseFunction fun = new DatabaseFunction();
                fun.Name = row[key].ToString();
                if (!string.IsNullOrEmpty(ownerKey))
                    fun.SchemaOwner = row[ownerKey].ToString();
                if (sqlKey != null) fun.Sql = row[sqlKey].ToString();
                list.Add(fun);
            }
            return list;
        }

        public static List<DatabaseStoredProcedure> StoredProcedures(DataTable dt)
        {
            List<DatabaseStoredProcedure> list = new List<DatabaseStoredProcedure>();
            //sql server
            string key = "ROUTINE_NAME";
            string ownerKey = "ROUTINE_SCHEMA";
            //oracle
            if (!dt.Columns.Contains(key)) key = "OBJECT_NAME";
            if (!dt.Columns.Contains(ownerKey)) ownerKey = "OWNER";
            //jet
            if (!dt.Columns.Contains(key)) key = "PROCEDURE_NAME";
            if (!dt.Columns.Contains(ownerKey)) ownerKey = "PROCEDURE_SCHEMA";
            string sql = "PROCEDURE_DEFINITION";
            if (!dt.Columns.Contains(sql)) sql = "ROUTINE_DEFINITION"; //MySql
            if (!dt.Columns.Contains(sql)) sql = "SOURCE"; //firebird
            if (!dt.Columns.Contains(sql)) sql = null;
            //Devart.Data.Oracle
            if (!dt.Columns.Contains(key)) key = "NAME";
            if (!dt.Columns.Contains(ownerKey)) ownerKey = "SCHEMA";

            foreach (DataRow row in dt.Rows)
            {
                DatabaseStoredProcedure t = new DatabaseStoredProcedure();
                t.Name = row[key].ToString();
                t.SchemaOwner = row[ownerKey].ToString();
                if (sql != null) t.Sql = row[sql].ToString();
                list.Add(t);
            }
            return list;
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

            //oledb
            if (!arguments.Columns.Contains(sprocKey)) sprocKey = "PROCEDURE_NAME";
            if (!arguments.Columns.Contains(ownerKey)) ownerKey = "PROCEDURE_SCHEMA";

            //Devart.Data.Oracle
            if (!arguments.Columns.Contains(sprocKey)) sprocKey = "PROCEDURE";
            if (!arguments.Columns.Contains(ownerKey)) ownerKey = "SCHEMA";
            if (packageKey == null && arguments.Columns.Contains("PACKAGE")) packageKey = "PACKAGE";


            //project the sprocs (which won't have packages) into a distinct view
            DataView sprocNames = new DataView(arguments);
            sprocNames.Sort = sprocKey;
            DataTable sprocTable;
            if (packageKey == null)
                sprocTable = sprocNames.ToTable(true, sprocKey, ownerKey); //distinct
            else
                sprocTable = sprocNames.ToTable(true, sprocKey, ownerKey, packageKey);

            //go thru all sprocs with arguments- if not in sproc list, add it
            foreach (DataRow row in sprocTable.Rows)
            {
                string name = row[sprocKey].ToString();
                string owner = row[ownerKey].ToString();
                string package = null; //for non-Oracle, package is always null
                if (packageKey != null)
                {
                    package = row[packageKey].ToString();
                    if (package == string.Empty) package = null; //so we can match easily
                }

                DataView dv = new DataView(arguments);
                //match sproc name and schema
                dv.RowFilter = "[" + sprocKey + "] = '" + name + "' AND [" + ownerKey + "] = '" + owner + "'";
                dv.Sort = ordinalKey;
                List<DatabaseArgument> args = StoredProcedureArguments(dv);

                DatabaseStoredProcedure sproc = databaseSchema.StoredProcedures.Find(delegate(DatabaseStoredProcedure x) { return x.Name == name && x.SchemaOwner == owner && x.Package == package; });
                if (sproc == null) //is it actually a function?
                {
                    DatabaseFunction fun = databaseSchema.Functions.Find(delegate(DatabaseFunction f) { return f.Name == name && f.SchemaOwner == owner && f.Package == package; });
                    if (fun != null)
                    {
                        fun.Arguments = args;
                        continue;
                    }
                }
                if (sproc == null) //sproc in a package and not found before?
                {
                    sproc = new DatabaseStoredProcedure();
                    sproc.Name = name;
                    sproc.SchemaOwner = owner;
                    sproc.Package = package;
                    databaseSchema.StoredProcedures.Add(sproc);
                }
                sproc.Arguments = args;
            }
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

            //oledb
            if (!arguments.Columns.Contains(sprocName)) sprocName = "PROCEDURE_NAME";
            if (!arguments.Columns.Contains(ownerKey)) ownerKey = "PROCEDURE_SCHEMA";
            if (!arguments.Columns.Contains(inoutKey)) inoutKey = null;

            foreach (DataRowView row in dataView)
            {
                var t = new DatabaseArgument();
                t.Name = row[name].ToString();
                t.ProcedureName = row[sprocName].ToString();
                t.SchemaOwner = row[ownerKey].ToString();
                if (packageKey != null) t.PackageName = row[packageKey].ToString();
                t.Ordinal = Convert.ToDecimal(row[ordinalKey], CultureInfo.CurrentCulture);

                t.DatabaseDataType = row[datatypeKey].ToString();
                if (inoutKey != null)
                {
                    string inout = row[inoutKey].ToString();
                    if (inout.Contains("IN")) t.In = true;
                    if (inout.Contains("OUT")) t.Out = true;
                }

                //Oracle: these can be decimals, but we'll assume ints
                t.Length = GetNullableInt(row[lengthKey]);
                t.Precision = GetNullableInt(row[precisionKey]);
                t.Scale = GetNullableInt(row[scaleKey]);

                list.Add(t);
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
