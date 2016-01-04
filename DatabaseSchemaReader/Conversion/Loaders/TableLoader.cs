using System;
using System.Data;
using System.Linq;
using System.Threading;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Conversion.Loaders
{
    internal class TableLoader
    {
        private readonly SchemaExtendedReader _schemaReader;
        private readonly DatabaseSchema _databaseSchema;

        public event EventHandler<ReaderEventArgs> ReaderProgress;

        public TableLoader(SchemaExtendedReader schemaReader, DatabaseSchema databaseSchema)
        {
            _schemaReader = schemaReader;
            _databaseSchema = databaseSchema;
        }

        public DatabaseTable Load(string tableName, CancellationToken ct)
        {
            if (ct.IsCancellationRequested) return new DatabaseTable();
            ReaderEventArgs.RaiseEvent(ReaderProgress, this, ProgressType.ReadingSchema, SchemaObjectType.Tables, tableName, null, null);

            var schemaOwner = _schemaReader.Owner;
            DatabaseTable table;
            using (var ds = _schemaReader.Table(tableName))
            {
                if (ds == null) return null;
                if (ds.Tables.Count == 0) return null;
                ReaderEventArgs.RaiseEvent(ReaderProgress, this, ProgressType.Processing, SchemaObjectType.Tables, tableName, null, null);

                table = _databaseSchema.FindTableByName(tableName, schemaOwner);
                if (table == null)
                {
                    table = new DatabaseTable();
                    _databaseSchema.Tables.Add(table);
                }
                table.Name = tableName;
                table.SchemaOwner = schemaOwner;
                //columns must be done first as it is updated by the others
                schemaOwner = AddColumns(schemaOwner, tableName, table, ds);
                AddConstraints(ds, table);
                AddOthers(schemaOwner, tableName, table, ds);

                _schemaReader.PostProcessing(table);
            }
            return table;
        }

        private string AddColumns(string schemaOwner, string tableName, DatabaseTable table, DataSet ds)
        {
            table.Columns.Clear();
            var columnConverter = new ColumnConverter(ds.Tables[_schemaReader.ColumnsCollectionName]);
            var databaseColumns = columnConverter.Columns(tableName, schemaOwner).ToList();
            if (!databaseColumns.Any())
            {
                //need to define the schema
                databaseColumns = columnConverter.Columns().ToList();
                var first = databaseColumns.FirstOrDefault();
                if (first != null)
                {
                    //take the schema of the first we find
                    table.SchemaOwner = schemaOwner = first.SchemaOwner;
                }
                databaseColumns = columnConverter.Columns(tableName, schemaOwner).ToList();
            }
            table.Columns.AddRange(databaseColumns);
            return schemaOwner;
        }

        private void AddConstraints(DataSet ds, DatabaseTable table)
        {
            if (ds.Tables.Contains(_schemaReader.PrimaryKeysCollectionName))
            {
                var converter = new SchemaConstraintConverter(ds.Tables[_schemaReader.PrimaryKeysCollectionName],
                    ConstraintType.PrimaryKey);
                var pkConstraints = converter.Constraints();
                PrimaryKeyLogic.AddPrimaryKey(table, pkConstraints);
            }
            if (ds.Tables.Contains(_schemaReader.ForeignKeysCollectionName))
            {
                var converter = new SchemaConstraintConverter(ds.Tables[_schemaReader.ForeignKeysCollectionName],
                    ConstraintType.ForeignKey);
                table.AddConstraints(converter.Constraints());
            }
            if (ds.Tables.Contains(_schemaReader.ForeignKeyColumnsCollectionName))
            {
                var fkConverter = new ForeignKeyColumnConverter(ds.Tables[_schemaReader.ForeignKeyColumnsCollectionName]);
                fkConverter.AddForeignKeyColumns(table.ForeignKeys);
            }

            if (ds.Tables.Contains(_schemaReader.UniqueKeysCollectionName))
            {
                var converter = new SchemaConstraintConverter(ds.Tables[_schemaReader.UniqueKeysCollectionName],
                    ConstraintType.UniqueKey);
                table.AddConstraints(converter.Constraints());
            }
        }

        private void AddOthers(string schemaOwner, string tableName, DatabaseTable table, DataSet ds)
        {
            if (ds.Tables.Contains(_schemaReader.ComputedColumnsCollectionName))
            {
                SchemaConstraintConverter.AddComputed(ds.Tables[_schemaReader.ComputedColumnsCollectionName], table);
            }

            var indexConverter = new IndexConverter(ds.Tables[_schemaReader.IndexColumnsCollectionName], null);
            table.Indexes.AddRange(indexConverter.Indexes(tableName, schemaOwner));

            if (ds.Tables.Contains(_schemaReader.IdentityColumnsCollectionName))
                SchemaConstraintConverter.AddIdentity(ds.Tables[_schemaReader.IdentityColumnsCollectionName], table);
        }
    }
}