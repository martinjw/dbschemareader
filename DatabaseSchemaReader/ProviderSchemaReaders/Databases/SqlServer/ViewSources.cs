using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;
using DatabaseSchemaReader.ProviderSchemaReaders.ResultModels;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.SqlServer
{
    internal class ViewSources : SqlExecuter<ProcedureSource>
    {
        private readonly string _name;

        public ViewSources(int? commandTimeout, string owner, string name) : base(commandTimeout, owner)
        {
            _name = name;
            Owner = owner;
            Sql = @"SELECT
    OBJECT_SCHEMA_NAME(o.object_id) AS ""OWNER"",
    OBJECT_NAME(sm.object_id) AS ""NAME"",
    o.type AS ""TYPE"",
    sm.definition As ""TEXT""
FROM sys.sql_modules AS sm
    JOIN sys.objects AS o
        ON sm.object_id = o.object_id
WHERE (o.type='V')
    AND (OBJECT_SCHEMA_NAME(o.object_id) = @schemaOwner OR @schemaOwner IS NULL)
    AND (OBJECT_NAME(sm.object_id) = @name OR @name IS NULL)
ORDER BY o.type;";
        }

        public IList<ProcedureSource> Execute(IConnectionAdapter connectionAdapter)
        {
            try
            {
                ExecuteDbReader(connectionAdapter);
            }
            catch (DbException exception)
            {
                //1. Security does not allow access
                //2. OBJECT_SCHEMA_NAME is only available from SQLServer 2005 SP2
                Trace.TraceError("Handled: " + exception);
                //continue without the source
            }
            return Result;
        }

        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "schemaOwner", Owner);
            AddDbParameter(command, "name", _name);
        }

        protected override void Mapper(IDataRecord record)
        {
            var source = new ProcedureSource
                        {
                            Name = record.GetString("Name"),
                            SchemaOwner = record.GetString("Owner")
                        };
            var type = record.GetString("Type").Trim();
            switch (type)
            {
                case "P": //sql server procedure
                    source.SourceType = SourceType.StoredProcedure;
                    break;

                case "TF": //sql server table-valued function
                case "FN": //sql server scalar function
                    source.SourceType = SourceType.Function;
                    break;

                case "V": //sql server view
                    source.SourceType = SourceType.View;
                    break;
            }
            source.Text = record.GetString("Text");
            Result.Add(source);
        }
    }
}