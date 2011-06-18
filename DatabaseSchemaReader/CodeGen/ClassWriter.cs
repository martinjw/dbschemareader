using System;
using System.Linq;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.CodeGen
{
    /// <summary>
    /// Turns a specified <see cref="DatabaseTable"/> into a C# class
    /// </summary>
    public class ClassWriter
    {
        private readonly DatabaseTable _table;
        private readonly string _ns;
        private readonly ClassBuilder _cb;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassWriter"/> class.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="ns">The ns.</param>
        public ClassWriter(DatabaseTable table, string ns)
        {
            _ns = ns;
            _table = table;
            _cb = new ClassBuilder();
        }

        /// <summary>
        /// Writes the C# code of the table
        /// </summary>
        /// <returns></returns>
        public string Write()
        {
            var className = _table.NetName;
            if (string.IsNullOrEmpty(className) && _table.DatabaseSchema != null)
            {
                PrepareSchemaNames.Prepare(_table.DatabaseSchema);
                className = _table.NetName;
            }


            WriteNamespaces();

            if (!string.IsNullOrEmpty(_ns))
            {
                _cb.BeginNest("namespace " + _ns);
            }

            using (_cb.BeginNest("public class " + className, "Class representing " + _table.Name + " table"))
            {
                InitializeCollectionsInConstructor(className);

                if (_table.HasCompositeKey)
                {
                    _cb.AppendAutomaticProperty(className + "Key", "Key");
                }

                foreach (var column in _table.Columns)
                {
                    if (_table.HasCompositeKey && column.IsPrimaryKey) continue;
                    WriteColumn(column);
                }

                WriteForeignKeyCollections();

                if (!_table.HasCompositeKey)
                {
                    var overrider = new OverrideWriter(_cb, _table);
                    overrider.AddOverrides();
                }
            }

            if (_table.HasCompositeKey)
            {
                WriteCompositeKeyClass(className);
            }

            if (!string.IsNullOrEmpty(_ns))
            {
                _cb.EndNest();
            }

            return _cb.ToString();
        }

        private void WriteNamespaces()
        {
            _cb.AppendLine("using System;");
            if (_table.ForeignKeyChildren.Any())
            {
                _cb.AppendLine("using System.Collections.Generic;");
            }
            _cb.AppendLine("using System.ComponentModel.DataAnnotations;");
        }

        private void WriteForeignKeyCollections()
        {
            foreach (var foreignKey in _table.ForeignKeyChildren)
            {
                var propertyName = foreignKey.NetName + "Collection";
                var dataType = "IList<" + foreignKey.NetName + ">";
                _cb.AppendAutomaticCollectionProperty(dataType, propertyName);
            }
        }

        private void InitializeCollectionsInConstructor(string className)
        {
            if (!_table.ForeignKeyChildren.Any()) return;
            using (_cb.BeginNest("public " + className + "()"))
            {
                foreach (var foreignKey in _table.ForeignKeyChildren)
                {
                    var propertyName = foreignKey.NetName + "Collection";
                    var dataType = "List<" + foreignKey.NetName + ">";
                    _cb.AppendLine(propertyName + " = new " + dataType + "();");
                }
            }
            _cb.AppendLine("");
        }

        private void WriteColumn(DatabaseColumn column)
        {
            var propertyName = column.NetName;
            var dt = column.DataType;
            var dataType = dt != null ? dt.NetCodeName(column) : "object";
            //if it's nullable (and not string or array)
            if (column.Nullable &&
                dt != null &&
                !dt.IsString &&
                !dataType.EndsWith("[]", StringComparison.OrdinalIgnoreCase))
            {
                dataType += "?"; //nullable
            }
            if (column.IsForeignKey && column.ForeignKeyTable != null)
            {
                dataType = column.ForeignKeyTable.NetName;
            }

            DataAnnotationWriter.Write(_cb, column);
            _cb.AppendAutomaticProperty(dataType, propertyName);
        }


        private void WriteCompositeKeyClass(string className)
        {
            using (_cb.BeginNest("public class " + className + "Key", "Class representing " + _table.Name + " composite key"))
            {
                foreach (var column in _table.Columns)
                {
                    if (column.IsPrimaryKey)
                        WriteColumn(column);
                }

                var overrider = new OverrideWriter(_cb, _table);
                overrider.NetName = className + "Key";
                overrider.AddOverrides();
            }
        }
    }
}
