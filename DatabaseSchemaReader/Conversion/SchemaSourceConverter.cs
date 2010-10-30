using System;
using System.Data;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Conversion
{
    class SchemaSourceConverter
    {
        public static void AddSources(DatabaseSchema schema, DataTable dt)
        {
            //bool hasLines = (dt.Columns.Contains("LINE"));

            DatabaseStoredProcedure matchProcedure;
            DatabaseFunction matchFunction;
            DatabasePackage matchPackage;

            //oracle sources come in lines; assume in order, so we can just concatenate
            foreach (DataRow row in dt.Rows)
            {
                string owner = row["OWNER"].ToString();
                string name = row["NAME"].ToString();
                string type = row["TYPE"].ToString().Trim();
                string text = row["TEXT"].ToString();
                switch (type)
                {
                    case "PACKAGE": //oracle package
                        matchPackage = schema.Packages.Find(delegate(DatabasePackage x) { return x.Name.Equals(name, StringComparison.OrdinalIgnoreCase); });
                        if (matchPackage == null)
                        {
                            matchPackage = AddPackage(name, owner);
                            schema.Packages.Add(matchPackage);
                        }
                        //text will have a newline but not cReturn
                        matchPackage.Definition += text;
                        break;

                    case "PACKAGE BODY": //oracle package body
                        matchPackage = schema.Packages.Find(delegate(DatabasePackage x) { return x.Name.Equals(name, StringComparison.OrdinalIgnoreCase); });
                        if (matchPackage == null)
                        {
                            matchPackage = AddPackage(name, owner);
                            schema.Packages.Add(matchPackage);
                        }
                        //text will have a newline but not cReturn
                        matchPackage.Body += text;
                        break;

                    case "PROCEDURE": //oracle procedure
                        matchProcedure = schema.StoredProcedures.Find(delegate(DatabaseStoredProcedure x) { return x.Name.Equals(name, StringComparison.OrdinalIgnoreCase); });
                        if (matchProcedure == null) continue;
                        //text will have a newline but not cReturn
                        matchProcedure.Sql += text;
                        break;

                    case "FUNCTION": //oracle function
                        matchFunction = schema.Functions.Find(delegate(DatabaseFunction x) { return x.Name.Equals(name, StringComparison.OrdinalIgnoreCase); });
                        if (matchFunction == null) continue;
                        matchFunction.Sql += text;
                        break;

                    case "P": //sql server procedure
                        matchProcedure = schema.StoredProcedures.Find(delegate(DatabaseStoredProcedure x) { return x.Name.Equals(name, StringComparison.OrdinalIgnoreCase); });
                        if (matchProcedure == null) continue;
                        matchProcedure.Sql = text;
                        break;

                    case "FN": //sql server function
                        matchFunction = schema.Functions.Find(delegate(DatabaseFunction x) { return x.Name.Equals(name, StringComparison.OrdinalIgnoreCase); });
                        if (matchFunction == null) continue;
                        matchFunction.Sql = text;
                        break;
                }
            }
        }

        private static DatabasePackage AddPackage(string name, string owner)
        {
            DatabasePackage pack = new DatabasePackage();
            pack.Name = name;
            pack.SchemaOwner = owner;
            return pack;
        }
    }
}
