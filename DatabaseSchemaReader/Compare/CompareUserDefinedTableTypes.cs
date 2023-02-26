using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Compare
{
    class CompareUserDefinedTableTypes
    {
        private readonly IList<CompareResult> _results;
        private readonly ComparisonWriter _writer;

        public CompareUserDefinedTableTypes(IList<CompareResult> results, ComparisonWriter writer)
        {
            _results = results;
            _writer = writer;
        }

        public void Execute(ICollection<UserDefinedTable> baseTypes, ICollection<UserDefinedTable> compareTypes)
        {
            bool first = false;

            //find new functions (in compare, but not in base)
            foreach (var userDefinedTable in compareTypes)
            {
                var name = userDefinedTable.Name;
                var schema = userDefinedTable.SchemaOwner;
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
                script+= "-- NEW USER DEFINED TABLE TYPE " + userDefinedTable.Name + Environment.NewLine +
                    _writer.AddUserDefinedTableType(userDefinedTable);
                CreateResult(ResultType.Add, userDefinedTable, script);
            }

            //find dropped and existing functions
            foreach (var userDefinedTable in baseTypes)
            {
                var name = userDefinedTable.Name;
                var schema = userDefinedTable.SchemaOwner;
                var match = compareTypes.FirstOrDefault(t => t.Name == name && t.SchemaOwner == schema);
                if (match == null)
                {
                    CreateResult(ResultType.Delete, userDefinedTable, "-- DROP USER DEFINED TABLE TYPE " + userDefinedTable.Name + Environment.NewLine +
                        _writer.DropUserDefinedTableType(userDefinedTable));
                    continue;
                }
                //check properties for change
                if(IsEqual(userDefinedTable, match)) continue;

                //in Oracle could be a CREATE OR REPLACE
                CreateResult(ResultType.Change, userDefinedTable, "-- ALTER USER DEFINED TABLE TYPE " + userDefinedTable.Name + Environment.NewLine +
                    _writer.DropUserDefinedTableType(userDefinedTable) + Environment.NewLine +
                    _writer.AddUserDefinedTableType(match));
            }
        }

        private bool IsEqual(UserDefinedTable userDefinedTable, UserDefinedTable match)
        {
            if (userDefinedTable.Source != match.Source ||
                userDefinedTable.SourceBody != match.SourceBody) return false;
            if(userDefinedTable.CollectionTypeName != match.CollectionTypeName) return false;
            if (userDefinedTable.Columns.Count != match.Columns.Count) return false;
            foreach (var column in userDefinedTable.Columns)
            {
                var name = column.Name;
                var matchColumn = match.Columns.FirstOrDefault(t => t.Name == name);
                if (matchColumn == null) return false;

                if (string.Equals(column.DbDataType, matchColumn.DbDataType, StringComparison.OrdinalIgnoreCase) &&
                    column.Length == matchColumn.Length &&
                    column.Precision == matchColumn.Precision &&
                    column.Scale == matchColumn.Scale &&
                    column.Nullable == matchColumn.Nullable &&
                    column.Ordinal == matchColumn.Ordinal &&
                    column.DefaultValue == matchColumn.DefaultValue)
                {
                    continue;
                }

                return false;
            }
            //check constraints, pk and uks, indexes

            return true;

        }


        private void CreateResult(ResultType resultType, UserDefinedTable dataType, string script)
        {
            var result = new CompareResult
                {
                    SchemaObjectType = SchemaObjectType.UserTableType,
                    ResultType = resultType,
                    Name = dataType.Name,
                    SchemaOwner = dataType.SchemaOwner,
                    Script = script
                };
            _results.Add(result);
        }
    }
}
