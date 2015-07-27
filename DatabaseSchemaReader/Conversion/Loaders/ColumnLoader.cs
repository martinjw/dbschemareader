using System;
using System.Collections.Generic;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Conversion.Loaders
{
    /// <summary>
    /// Loads and converts the dataTable (wrapping the Converter). Hides all/byTable logic.
    /// </summary>
    class ColumnLoader
    {
        private readonly SchemaExtendedReader _sr;
        private ColumnConverter _columnConverter;
        /// <summary>
        /// <c>True</c> if cannot get columns for all tables
        /// </summary>
        private readonly bool _noColumns;

        public ColumnLoader(SchemaExtendedReader schemaReader)
        {
            _sr = schemaReader;
            var cols = _sr.Columns(null);
            _noColumns = (cols.Rows.Count == 0);
            _columnConverter = new ColumnConverter(cols);
        }

        public IEnumerable<DatabaseColumn> Load(string tableName, string schemaName)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException("tableName", "must have tableName");
            //schemaName may be null for databases that don't have schemas

            if (_noColumns)
            {
                //have to get columns for specific table
                var cols = _sr.Columns(tableName);
                _columnConverter = new ColumnConverter(cols);
            }

            return _columnConverter.Columns(tableName, schemaName);
        }

    }
}
