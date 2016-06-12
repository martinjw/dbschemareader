using System;
using System.Collections.Generic;
using System.Linq;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Conversion.Loaders
{
    /// <summary>
    /// Loads and converts the dataTable (wrapping the Converter). Hides all/byTable logic.
    /// </summary>
    class ViewColumnLoader
    {
        private readonly SchemaExtendedReader _sr;
        private ViewColumnConverter _columnConverter;
        /// <summary>
        /// <c>True</c> if cannot get columns for all views
        /// </summary>
        private readonly bool _noColumns;

        public ViewColumnLoader(SchemaExtendedReader schemaReader)
        {
            _sr = schemaReader;
            var cols = _sr.ViewColumns(null);
            _noColumns = (cols.Rows.Count == 0);
            _columnConverter = new ViewColumnConverter(cols);
        }

        public IList<DatabaseColumn> Load()
        {
            return _columnConverter.Columns().ToList();
        }

        public IEnumerable<DatabaseColumn> Load(string viewName, string schemaName)
        {
            if (string.IsNullOrEmpty(viewName)) throw new ArgumentNullException("viewName", "must have viewName");
            //schemaName may be null

            if (_noColumns)
            {
                //have to get columns for specific table
                var cols = _sr.Columns(viewName);
                _columnConverter = new ViewColumnConverter(cols);
            }

            return _columnConverter.Columns(viewName, schemaName);
        }
    }
}
