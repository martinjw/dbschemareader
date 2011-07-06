using System.Collections.Generic;
using System.Linq;
using System.Text;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Compare
{
    class CompareProcedures
    {
        private readonly StringBuilder _sb;
        private readonly ComparisonWriter _writer;

        public CompareProcedures(StringBuilder sb, ComparisonWriter writer)
        {
            _sb = sb;
            _writer = writer;
        }

        public void Execute(IEnumerable<DatabaseStoredProcedure> baseProcedures, IEnumerable<DatabaseStoredProcedure> compareProcedures)
        {
            bool first = false;

            //find new sprocs (in compare, but not in base)
            foreach (var procedure in compareProcedures)
            {
                var name = procedure.Name;
                var schema = procedure.SchemaOwner;
                var match = baseProcedures.FirstOrDefault(t => t.Name == name && t.SchemaOwner == schema);
                if (match != null) continue;
                if (!first)
                {
                    first = true;
                    //CREATE PROCEDURE cannot be combined with other statements in a batch, so be preceeded by and terminate with a "GO" (sqlServer) or "/" (Oracle)
                    if (_sb.Length > 0) _sb.AppendLine(_writer.RunStatements());
                }
                _sb.AppendLine("-- NEW STORED PROCEDURE " + procedure.Name);
                _sb.AppendLine(_writer.AddProcedure(procedure));
            }

            //find dropped and existing sprocs
            foreach (var procedure in baseProcedures)
            {
                var name = procedure.Name;
                var schema = procedure.SchemaOwner;
                var match = compareProcedures.FirstOrDefault(t => t.Name == name && t.SchemaOwner == schema);
                if (match == null)
                {
                    _sb.AppendLine("-- DROP STORED PROCEDURE " + procedure.Name);
                    _sb.AppendLine(_writer.DropProcedure(procedure));
                    continue;
                }
                //sproc may or may not have been changed

                if (procedure.Sql == match.Sql) continue; //the same
                //a sanitized comparison
                if (_writer.CompareProcedure(procedure.Sql, match.Sql)) continue;

                //in Oracle could be a CREATE OR REPLACE
                _sb.AppendLine("-- ALTER STORED PROCEDURE " + procedure.Name);
                _sb.AppendLine(_writer.DropProcedure(procedure));
                _sb.AppendLine(_writer.AddProcedure(match));
            }
        }
    }
}
