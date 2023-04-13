using System;
using System.Collections.Generic;
using System.Linq;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Compare
{
    class CompareUserDefinedTypes
    {
        private readonly IList<CompareResult> _results;
        private readonly ComparisonWriter _writer;

        public CompareUserDefinedTypes(IList<CompareResult> results, ComparisonWriter writer)
        {
            _results = results;
            _writer = writer;
        }

        public void Execute(ICollection<UserDataType> baseTypes, ICollection<UserDataType> compareTypes)
        {
            bool first = false;

            //find new functions (in compare, but not in base)
            foreach (var dataType in compareTypes)
            {
                var name = dataType.Name;
                var schema = dataType.SchemaOwner;
                var match = baseTypes.FirstOrDefault(t => t.Name == name && t.SchemaOwner == schema);
                if (match != null) continue;
                var script = string.Empty;
                if (!first)
                {
                    first = true;
                    //CREATE FUNCTION cannot be combined with other statements in a batch, 
                    //so be preceeded by and terminate with a "GO" (sqlServer) or "/" (Oracle)
                    if (_results.Count > 0) script+=_writer.RunStatements() + Environment.NewLine;
                }
                script+="-- NEW USER DEFINED DATA TYPE " + dataType.Name + Environment.NewLine +
                    _writer.AddDataType(dataType);
                CreateResult(ResultType.Add, dataType, script);
            }

            //find dropped and existing functions
            foreach (var dataType in baseTypes)
            {
                var name = dataType.Name;
                var schema = dataType.SchemaOwner;
                var match = compareTypes.FirstOrDefault(t => t.Name == name && t.SchemaOwner == schema);
                if (match == null)
                {
                    CreateResult(ResultType.Delete, dataType, "-- DROP USER DEFINED DATA TYPE " + dataType.Name + Environment.NewLine +
                        _writer.DropUserDataType(dataType));
                    continue;
                }
                //check properties for change

                if (dataType.DbTypeName == match.DbTypeName &&
                    dataType.DefaultValue == match.DefaultValue &&
                    dataType.MaxLength == match.MaxLength &&
                    dataType.Nullable == match.Nullable &&
                    dataType.Precision == match.Precision &&
                    dataType.Scale == match.Scale) continue; //the same

                //in Oracle could be a CREATE OR REPLACE
                CreateResult(ResultType.Change, dataType, "-- ALTER USER DEFINED DATA TYPE " + dataType.Name + Environment.NewLine +
                    _writer.DropUserDataType(dataType) + Environment.NewLine +
                    _writer.AddDataType(match));
            }
        }


        private void CreateResult(ResultType resultType, UserDataType dataType, string script)
        {
            var result = new CompareResult
                {
                    SchemaObjectType = SchemaObjectType.UserDataType,
                    ResultType = resultType,
                    Name = dataType.Name,
                    SchemaOwner = dataType.SchemaOwner,
                    Script = script
                };
            _results.Add(result);
        }
    }
}
