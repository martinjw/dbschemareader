using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.Filters;

namespace DatabaseSchemaReader.Conversion.Loaders
{
    internal class ProcedureLoader
    {
        private readonly SchemaExtendedReader _schemaReader;
        private readonly DatabaseSchema _databaseSchema;
        private readonly Exclusions _exclusions;

        public event EventHandler<ReaderEventArgs> ReaderProgress;

        public ProcedureLoader(SchemaExtendedReader schemaReader, DatabaseSchema databaseSchema, Exclusions exclusions)
        {
            _schemaReader = schemaReader;
            _databaseSchema = databaseSchema;
            _exclusions = exclusions;
        }

        public void Load(CancellationToken ct)
        {
            if (ct.IsCancellationRequested) return;
            try
            {
                ReaderEventArgs.RaiseEvent(ReaderProgress, this, ProgressType.ReadingSchema, SchemaObjectType.Functions);
                var functions = _schemaReader.Functions();
                ReaderEventArgs.RaiseEvent(ReaderProgress, this, ProgressType.Processing, SchemaObjectType.Functions);
                _databaseSchema.Functions.Clear();
                _databaseSchema.Functions.AddRange(SchemaProcedureConverter.Functions(functions));
            }
            catch (DbException ex)
            {
                Debug.WriteLine("Cannot read functions - database security may prevent access to DDL\n" + ex.Message);
                throw; //or suppress if not applicable
            }
            if (ct.IsCancellationRequested) return;

            ReaderEventArgs.RaiseEvent(ReaderProgress, this, ProgressType.ReadingSchema, SchemaObjectType.StoredProcedure);
            var dt = _schemaReader.StoredProcedures();

            if (ct.IsCancellationRequested) return;
            ReaderEventArgs.RaiseEvent(ReaderProgress, this, ProgressType.Processing, SchemaObjectType.StoredProcedure);
            SchemaProcedureConverter.StoredProcedures(_databaseSchema, dt);
            var procFilter = _exclusions.StoredProcedureFilter;
            if (procFilter != null)
            {
                _databaseSchema.StoredProcedures.RemoveAll(p => procFilter.Exclude(p.Name));
            }

            _databaseSchema.Packages.Clear();
            _databaseSchema.Packages.AddRange(SchemaProcedureConverter.Packages(_schemaReader.Packages()));
            var packFilter = _exclusions.PackageFilter;
            if (packFilter != null)
            {
                _databaseSchema.Packages.RemoveAll(p => packFilter.Exclude(p.Name));
            }

            if (ct.IsCancellationRequested) return;

            //do all the arguments as one call and sort them out.
            //NB: This is often slow on Oracle
            ReaderEventArgs.RaiseEvent(ReaderProgress, this, ProgressType.ReadingSchema, SchemaObjectType.ProcedureArguments);
            var args = _schemaReader.StoredProcedureArguments(null);

            ReaderEventArgs.RaiseEvent(ReaderProgress, this, ProgressType.Processing, SchemaObjectType.ProcedureArguments);
            var converter = new SchemaProcedureConverter();
            converter.PackageFilter = _exclusions.PackageFilter;
            converter.StoredProcedureFilter = _exclusions.StoredProcedureFilter;
            if (args.Rows.Count == 0)
            {
                //MySql v6 won't do all stored procedures. So we have to do them individually.
                foreach (var sproc in _databaseSchema.StoredProcedures)
                {
                    if (ct.IsCancellationRequested) return;
                    args = _schemaReader.StoredProcedureArguments(sproc.Name);
                    converter.UpdateArguments(_databaseSchema, args);
                }

                foreach (var function in _databaseSchema.Functions)
                {
                    if (ct.IsCancellationRequested) return;
                    args = _schemaReader.StoredProcedureArguments(function.Name);
                    converter.UpdateArguments(_databaseSchema, args);
                }
            }

            if (ct.IsCancellationRequested) return;
            //arguments could be for functions too
            converter.UpdateArguments(_databaseSchema, args);
            foreach (var function in _databaseSchema.Functions)
            {
                //return types are assigned as arguments (in most platforms). Move them to return type.
                function.CheckArgumentsForReturnType();
            }

            if (ct.IsCancellationRequested) return;
            //procedure, function and view source sql
            ReaderEventArgs.RaiseEvent(ReaderProgress, this, ProgressType.ReadingSchema, SchemaObjectType.ProcedureSource);
            var srcs = _schemaReader.ProcedureSource(null);

            if (ct.IsCancellationRequested) return;
            ReaderEventArgs.RaiseEvent(ReaderProgress, this, ProgressType.Processing, SchemaObjectType.ProcedureSource);
            SchemaSourceConverter.AddSources(_databaseSchema, srcs);
        }
    }
}