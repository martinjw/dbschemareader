using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.Filters;

namespace DatabaseSchemaReader.Conversion.Loaders
{
    internal class AllTablesLoader
    {
        private readonly SchemaExtendedReader _schemaReader;
        private readonly Exclusions _exclusions;

        public event EventHandler<ReaderEventArgs> ReaderProgress;

        public AllTablesLoader(SchemaExtendedReader schemaReader, Exclusions exclusions)
        {
            _schemaReader = schemaReader;
            _exclusions = exclusions;
        }

        private void RaiseReadingProgress(SchemaObjectType schemaObjectType)
        {
            ReaderEventArgs.RaiseEvent(ReaderProgress, this, ProgressType.ReadingSchema, schemaObjectType);
        }

        private IList<DatabaseTable> EmptyList()
        {
            return new List<DatabaseTable>();
        }

        public IList<DatabaseTable> Load(CancellationToken ct)
        {
            if (ct.IsCancellationRequested) return EmptyList();
            RaiseReadingProgress(SchemaObjectType.Tables);

            //get full datatables for all tables, to minimize database calls
            var tabs = _schemaReader.Tables();

            //we either use the converters directly (DataTable to our db model)
            //or loaders, which wrap the schema loader calls and converters
            //loaders hide the switch between calling for all tables, or a specific table
            if (ct.IsCancellationRequested) return EmptyList();
            RaiseReadingProgress(SchemaObjectType.Columns);
            var columnLoader = new ColumnLoader(_schemaReader);

            if (ct.IsCancellationRequested) return EmptyList();
            RaiseReadingProgress(SchemaObjectType.Constraints);
            var constraintLoader = new SchemaConstraintLoader(_schemaReader);
            var indexLoader = new IndexLoader(_schemaReader);

            var ids = _schemaReader.IdentityColumns(null);
            var computeds = _schemaReader.ComputedColumns(null);

            if (ct.IsCancellationRequested) return EmptyList();
            RaiseReadingProgress(SchemaObjectType.Descriptions);
            var tableDescriptions = new TableDescriptionConverter(_schemaReader.TableDescription(null));
            var columnDescriptions = new ColumnDescriptionConverter(_schemaReader.ColumnDescription(null));

            DataTable triggers = _schemaReader.Triggers(null);
            var triggerConverter = new TriggerConverter(triggers);

            if (ct.IsCancellationRequested) return EmptyList();
            ReaderEventArgs.RaiseEvent(ReaderProgress, this, ProgressType.Processing, SchemaObjectType.Tables);
            var tables = SchemaConverter.Tables(tabs);
            var tableFilter = _exclusions.TableFilter;
            if (tableFilter != null)
            {
                tables.RemoveAll(t => tableFilter.Exclude(t.Name));
            }
            tables.Sort(delegate(DatabaseTable t1, DatabaseTable t2)
            {
                //doesn't account for mixed schemas
                return string.Compare(t1.Name, t2.Name, StringComparison.OrdinalIgnoreCase);
            });

            int tablesCount = tables.Count;
            for (var i = 0; i < tablesCount; i++)
            {
                var table = tables[i];
                var tableName = table.Name;
                var schemaName = table.SchemaOwner;

                if (ct.IsCancellationRequested) return tables;
                ReaderEventArgs.RaiseEvent(ReaderProgress, this, ProgressType.Processing, SchemaObjectType.Tables, tableName, i, tablesCount);
                table.Description = tableDescriptions.FindDescription(table.SchemaOwner, tableName);

                var databaseColumns = columnLoader.Load(tableName, schemaName);
                table.Columns.AddRange(databaseColumns);

                columnDescriptions.AddDescriptions(table);

                var pkConstraints = constraintLoader.Load(tableName, schemaName, ConstraintType.PrimaryKey);
                PrimaryKeyLogic.AddPrimaryKey(table, pkConstraints);

                var fks = constraintLoader.Load(tableName, schemaName, ConstraintType.ForeignKey);
                table.AddConstraints(fks);

                table.AddConstraints(constraintLoader.Load(tableName, schemaName, ConstraintType.UniqueKey));
                table.AddConstraints(constraintLoader.Load(tableName, schemaName, ConstraintType.Check));
                table.AddConstraints(constraintLoader.Load(tableName, schemaName, ConstraintType.Default));

                indexLoader.AddIndexes(table);

                SchemaConstraintConverter.AddIdentity(ids, table);
                SchemaConstraintConverter.AddComputed(computeds, table);

                table.Triggers.Clear();
                table.Triggers.AddRange(triggerConverter.Triggers(tableName));
                _schemaReader.PostProcessing(table);
            }
            return tables;
        }
    }
}