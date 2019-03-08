using System;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable once RedundantUsingDirective 
using System.Threading;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.Filters;
using DatabaseSchemaReader.ProviderSchemaReaders.Adapters;
using DatabaseSchemaReader.ProviderSchemaReaders.ResultModels;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Builders
{
    class ProcedureBuilder
    {
        private readonly ReaderAdapter _readerAdapter;
        private readonly DatabaseSchema _databaseSchema;
        private readonly Exclusions _exclusions;

        public event EventHandler<ReaderEventArgs> ReaderProgress;

        public ProcedureBuilder(ReaderAdapter readerAdapter, DatabaseSchema databaseSchema, Exclusions exclusions)
        {
            _readerAdapter = readerAdapter;
            _databaseSchema = databaseSchema;
            _exclusions = exclusions;
        }

        protected void RaiseReadingProgress(SchemaObjectType schemaObjectType)
        {
            ReaderEventArgs.RaiseEvent(ReaderProgress, this, ProgressType.ReadingSchema, schemaObjectType);
        }

        protected void RaiseProgress(ProgressType progressType,
            SchemaObjectType schemaObjectType,
            string name, int? index, int? count)
        {
            ReaderEventArgs.RaiseEvent(ReaderProgress, this, progressType, schemaObjectType,
    name, index, count);
        }

        public void Execute(CancellationToken ct)
        {
            if (ct.IsCancellationRequested) return;

            RaiseReadingProgress(SchemaObjectType.StoredProcedure);
            var sprocs = _readerAdapter.StoredProcedures(null).ToList();

            if (ct.IsCancellationRequested) return;
            RaiseReadingProgress(SchemaObjectType.Functions);
            var functions = _readerAdapter.Functions(null).ToList();

            //before we split sprocs and functions into packages, add arguments and sources
            if (ct.IsCancellationRequested) return;
            RaiseReadingProgress(SchemaObjectType.ProcedureArguments);
            var args = _readerAdapter.ProcedureArguments(null).ToList();
            AddArguments(sprocs, functions, args);

            //exclusions
            var procFilter = _exclusions.StoredProcedureFilter;
            if (procFilter != null)
            {
                sprocs.RemoveAll(p => procFilter.Exclude(p.Name));
                functions.RemoveAll(p => procFilter.Exclude(p.Name));
            }

            //packages
            var packFilter = _exclusions.PackageFilter;
            if (packFilter == null) packFilter = new Filter();
            var packs = _readerAdapter.Packages(null).ToList();
            packs.RemoveAll(p => packFilter.Exclude(p.Name));

            //move sprocs into packages
            MoveStoredProceduresIntoPackages(packs, sprocs, packFilter);
            //move funcs into packages
            MoveFunctionsIntoPackages(packs, functions, packFilter);

            //add to schema
            if (ct.IsCancellationRequested) return;
            _databaseSchema.StoredProcedures.Clear();
            _databaseSchema.StoredProcedures.AddRange(sprocs);
            _databaseSchema.Functions.Clear();
            _databaseSchema.Functions.AddRange(functions);
            _databaseSchema.Packages.Clear();
            _databaseSchema.Packages.AddRange(packs);

            if (ct.IsCancellationRequested) return;
            RaiseReadingProgress(SchemaObjectType.ProcedureSource);
            AddSources();
        }

        private void AddSources()
        {
            var sources = _readerAdapter.ProcedureSources(null).ToList();
            foreach (var source in sources)
            {
                var name = source.Name;
                var owner = source.SchemaOwner;
                switch (source.SourceType)
                {
                    case SourceType.StoredProcedure:
                        var sproc = _databaseSchema.StoredProcedures.Find(x => x.Name == name && x.SchemaOwner == owner);
                        if (sproc != null) sproc.Sql = source.Text;
                        break;

                    case SourceType.Function:
                        var fun = _databaseSchema.Functions.Find(x => x.Name == name && x.SchemaOwner == owner);
                        if (fun != null) fun.Sql = source.Text;
                        break;

                    case SourceType.View:
                        var view = _databaseSchema.Views.Find(x => x.Name == name && x.SchemaOwner == owner);
                        if (view != null) view.Sql = source.Text;
                        break;

                    case SourceType.Package:
                        var pack = _databaseSchema.Packages.Find(x => x.Name == name && x.SchemaOwner == owner);
                        if (pack != null) pack.Definition = source.Text;
                        break;

                    case SourceType.PackageBody:
                        var pack2 = _databaseSchema.Packages.Find(x => x.Name == name && x.SchemaOwner == owner);
                        if (pack2 != null) pack2.Body = source.Text;
                        break;
                }
            }
        }

        private void AddArguments(List<DatabaseStoredProcedure> sprocs, List<DatabaseFunction> functions, List<DatabaseArgument> args)
        {
            FilterOracleOwner(args);
            foreach (var sproc in sprocs)
            {
                var sprocArgs =
                    args.Where(
                        x =>
                            x.SchemaOwner == sproc.SchemaOwner && x.ProcedureName == sproc.Name &&
                            x.PackageName == sproc.Package)
                        .OrderBy(x => x.Ordinal);
                sproc.Arguments.Clear();
                sproc.Arguments.AddRange(sprocArgs);
            }
            foreach (var func in functions)
            {
                var funcArgs =
                    args.Where(
                        x =>
                            x.SchemaOwner == func.SchemaOwner && x.ProcedureName == func.Name &&
                            x.PackageName == func.Package)
                        .OrderBy(x => x.Ordinal);
                func.Arguments.Clear();
                func.Arguments.AddRange(funcArgs);
                func.CheckArgumentsForReturnType();
            }
        }

        private void FilterOracleOwner(List<DatabaseArgument> args)
        {
            var isOracle = (_readerAdapter.Parameters.SqlType == SqlType.Oracle);
            if (isOracle)
            {
                var schemaOwner = _readerAdapter.Parameters.Owner;
                if (!string.IsNullOrEmpty(schemaOwner))
                {
                    args.RemoveAll(x => x.SchemaOwner != schemaOwner);
                    return;
                }
                var systemOwners = new[] { "SYS", "CTXSYS", "MDSYS", "OLAPSYS", "ORDSYS", "OUTLN", "WKSYS", "WMSYS", "XDB", "ORDPLUGINS", "SYSTEM" };
                args.RemoveAll(x => systemOwners.Contains(x.SchemaOwner) || x.SchemaOwner.StartsWith("APEX", StringComparison.Ordinal));
            }
        }

        private void MoveFunctionsIntoPackages(List<DatabasePackage> packs, List<DatabaseFunction> functions, IFilter packFilter)
        {
            var packFuncs = functions.Where(x => !string.IsNullOrEmpty(x.Package)).ToList();
            if (packFuncs.Count == 0) return;
            var packList = packFuncs.Select(x => x.Package).Distinct();
            foreach (var packName in packList)
            {
                if (packFilter.Exclude(packName)) continue;
                var packContents = functions.Where(x => x.Package == packName).ToList();
                var package = packs.FirstOrDefault(x => string.Equals(x.Name, packName));
                if (package == null)
                {
                    package = new DatabasePackage { Name = packName };
                }
                package.Functions.AddRange(packContents);
                _databaseSchema.Packages.Add(package);
            }
            functions.RemoveAll(x => !string.IsNullOrEmpty(x.Package));
        }

        private void MoveStoredProceduresIntoPackages(List<DatabasePackage> packs, List<DatabaseStoredProcedure> sprocs, IFilter packFilter)
        {
            var packSprocs = sprocs.Where(x => !string.IsNullOrEmpty(x.Package)).ToList();
            if (packSprocs.Count == 0) return;
            var packList = packSprocs.Select(x => x.Package).Distinct();
            foreach (var packName in packList)
            {
                if (packFilter.Exclude(packName)) continue;
                var packContents = sprocs.Where(x => x.Package == packName).ToList();
                var package = packs.FirstOrDefault(x => string.Equals(x.Name, packName));
                if (package == null)
                {
                    package = new DatabasePackage { Name = packName };
                }
                package.StoredProcedures.AddRange(packContents);
                _databaseSchema.Packages.Add(package);
            }
            sprocs.RemoveAll(x => !string.IsNullOrEmpty(x.Package));
        }
    }
}