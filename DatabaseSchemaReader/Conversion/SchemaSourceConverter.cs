using System;
using System.Collections.Generic;
using System.Data;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ResultModels;

namespace DatabaseSchemaReader.Conversion
{
    class SchemaSourceConverter
    {
        public static void AddSources(DatabaseSchema schema, DataTable dt)
        {
            var sources = AddSources(dt);
            foreach (var source in sources)
            {
                var name = source.Name;
                var owner = source.SchemaOwner;
                var text = source.Text;
                switch (source.SourceType)
                {
                    case SourceType.StoredProcedure:
                        var matchProcedure = FindStoredProcedure(schema, owner, name);
                        if (matchProcedure == null) continue;
                        matchProcedure.Sql = text;
                        break;
                    case SourceType.Function:
                        var function = FindFunction(schema, owner, name);
                        if (function == null) continue;
                        function.Sql = text;
                        break;
                    case SourceType.View:
                        var matchView = FindView(schema, owner, name);
                        if (matchView == null) continue;
                        matchView.Sql = text;
                        break;
                    case SourceType.Package:
                        var package = FindPackage(schema, name, owner);
                        package.Definition = text;
                        break;
                    case SourceType.PackageBody:
                        var package2 = FindPackage(schema, name, owner);
                        package2.Definition = text;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public static IList<ProcedureSource> AddSources(DataTable dt)
        {
            //oracle sources come in lines; assume in order, so we can just concatenate
            //if they already have source, we don't want to overwrite, so we use a cache
            //var functionCache = new Dictionary<string, DatabaseFunction>();
            var result = new List<ProcedureSource>();

            foreach (DataRow row in dt.Rows)
            {
                string owner = row["OWNER"].ToString();
                string name = row["NAME"].ToString();
                string type = row["TYPE"].ToString().Trim();
                string text = row["TEXT"].ToString();
                switch (type)
                {
                    case "PACKAGE": //oracle package

                        var package = FindSource(result, owner, name, SourceType.Package);
                        //text will have a newline but not cReturn
                        package.Text += text;
                        break;

                    case "PACKAGE BODY": //oracle package body
                        var package2 = FindSource(result, owner, name, SourceType.PackageBody);
                        //text will have a newline but not cReturn
                        package2.Text += text;
                        break;

                    case "PROCEDURE": //oracle procedure
                        var matchProcedure = FindSource(result, owner, name, SourceType.StoredProcedure);
                        //text will have a newline but not cReturn
                        matchProcedure.Text += text;
                        break;

                    case "FUNCTION": //oracle function
                        var matchFunc = FindSource(result, owner, name, SourceType.Function);
                        matchFunc.Text += text;
                        break;

                    case "P": //sql server procedure
                        var matchSproc = FindSource(result, owner, name, SourceType.StoredProcedure);
                        matchSproc.Text = text;
                        break;

                    case "TF": //sql server table-valued function
                    case "FN": //sql server scalar function
                        var matchFunction = FindSource(result, owner, name, SourceType.Function);
                        matchFunction.Text = text;
                        break;

                    case "V": //sql server view
                        var matchView = FindSource(result, owner, name, SourceType.View);
                        matchView.Text = text;
                        break;
                }
            }
            return result;
        }

        private static ProcedureSource FindSource(List<ProcedureSource> sources, string owner, string name, SourceType sourceType)
        {
            var source = sources.Find(x => x.Name == name && x.SchemaOwner == owner && x.SourceType == sourceType);
            if (source == null)
            {
                source = new ProcedureSource
                         {
                             Name = name,
                             SchemaOwner = owner,
                             SourceType = sourceType,
                         };
                sources.Add(source);
            }
            return source;
        }

        //private static DatabaseFunction FindFunction(string name, DatabaseSchema schema, Dictionary<string, DatabaseFunction> functionCache)
        //{
        //    DatabaseFunction function;
        //    if (functionCache.ContainsKey(name))
        //    {
        //        function = functionCache[name];
        //    }
        //    else
        //    {
        //        function = FindFunction(schema, owner, name);
        //        if (function == null) return null;
        //        //we already have sql from the functions collection. Don't add to it.
        //        if (!string.IsNullOrEmpty(function.Sql)) return null;
        //        functionCache.Add(name, function);
        //    }
        //    return function;
        //}


        private static DatabaseView FindView(DatabaseSchema schema, string owner, string name)
        {
            return schema.Views.Find(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.SchemaOwner, owner, StringComparison.OrdinalIgnoreCase));
        }

        private static DatabaseFunction FindFunction(DatabaseSchema schema, string owner, string name)
        {
            return schema.Functions.Find(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.SchemaOwner, owner, StringComparison.OrdinalIgnoreCase));
        }

        private static DatabaseStoredProcedure FindStoredProcedure(DatabaseSchema schema, string owner, string name)
        {
            return schema.StoredProcedures.Find(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.SchemaOwner, owner, StringComparison.OrdinalIgnoreCase));
        }

        private static DatabasePackage FindPackage(DatabaseSchema schema, string name, string owner)
        {
            var matchPackage = schema.Packages.Find(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (matchPackage == null)
            {
                matchPackage = AddPackage(name, owner);
                schema.Packages.Add(matchPackage);
            }
            return matchPackage;
        }

        private static DatabasePackage AddPackage(string name, string owner)
        {
            var pack = new DatabasePackage();
            pack.Name = name;
            pack.SchemaOwner = owner;
            return pack;
        }
    }
}
