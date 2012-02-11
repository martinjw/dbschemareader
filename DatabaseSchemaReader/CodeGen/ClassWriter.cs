using System.Diagnostics;
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
        private readonly DataTypeWriter _dataTypeWriter = new DataTypeWriter();
        private DataAnnotationWriter _dataAnnotationWriter;

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
        /// Gets or sets the code target.
        /// </summary>
        /// <value>
        /// The code target.
        /// </value>
        public CodeTarget CodeTarget { get; set; }

        /// <summary>
        /// Gets or sets the collection namer.
        /// </summary>
        /// <value>
        /// The collection namer.
        /// </value>
        public ICollectionNamer CollectionNamer { get; set; }

        private string NameCollection(string name)
        {
            if (CollectionNamer == null) return name + "Collection";
            return CollectionNamer.NameCollection(name);
        }
        /// <summary>
        /// Writes the C# code of the table
        /// </summary>
        /// <returns></returns>
        public string Write()
        {
            _dataAnnotationWriter = new DataAnnotationWriter(CodeTarget == CodeTarget.PocoEntityCodeFirst);
            var className = _table.NetName;
            if (string.IsNullOrEmpty(className) && _table.DatabaseSchema != null)
            {
                PrepareSchemaNames.Prepare(_table.DatabaseSchema);
                className = _table.NetName;
            }
            _dataTypeWriter.CodeTarget = CodeTarget;

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
                    if (CodeTarget != CodeTarget.PocoEntityCodeFirst)
                    {
                        _cb.AppendAutomaticProperty(className + "Key", "Key");
                    }
                    else
                    {
                        //code first composite key
                        foreach (var column in _table.Columns.Where(c => c.IsPrimaryKey))
                        {
                            WriteColumn(column);
                        }
                    }
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
            var listType = "IList<";
            if (CodeTarget == CodeTarget.PocoEntityCodeFirst) listType = "ICollection<";
            foreach (var foreignKey in _table.ForeignKeyChildren)
            {
                if (foreignKey.IsManyToManyTable() && CodeTarget == CodeTarget.PocoEntityCodeFirst)
                {
                    WriteManyToManyCollection(foreignKey);
                    continue;
                }

                var propertyName = NameCollection(foreignKey.NetName);
                var dataType = listType + foreignKey.NetName + ">";
                _cb.AppendAutomaticCollectionProperty(dataType, propertyName);
            }
        }

        private void WriteManyToManyCollection(DatabaseTable foreignKey)
        {
            //look over the junction table to find the other many-to-many end
            var target = foreignKey.ManyToManyTraversal(_table);
            if (target == null)
            {
                Debug.WriteLine("Can't navigate the many to many relationship for " + _table.Name + " to " + foreignKey.Name);
                return;
            }
            var propertyName = NameCollection(target.NetName);
            var dataType = "ICollection<" + target.NetName + ">";
            _cb.AppendAutomaticCollectionProperty(dataType, propertyName);

        }

        private void WriteManyToManyInitialize(DatabaseTable foreignKey)
        {
            //look over the junction table to find the other many-to-many end
            var target = foreignKey.ManyToManyTraversal(_table);
            if (target == null)
            {
                return;
            }
            var propertyName = NameCollection(target.NetName);
            var dataType = "List<" + target.NetName + ">";
            _cb.AppendLine(propertyName + " = new " + dataType + "();");
        }

        private void InitializeCollectionsInConstructor(string className)
        {
            if (!_table.ForeignKeyChildren.Any()) return;
            using (_cb.BeginNest("public " + className + "()"))
            {
                foreach (var foreignKey in _table.ForeignKeyChildren)
                {
                    if (foreignKey.IsManyToManyTable() && CodeTarget == CodeTarget.PocoEntityCodeFirst)
                    {
                        WriteManyToManyInitialize(foreignKey);
                        continue;
                    }
                    var propertyName = NameCollection(foreignKey.NetName);
                    var dataType = "List<" + foreignKey.NetName + ">";
                    _cb.AppendLine(propertyName + " = new " + dataType + "();");
                }
            }
            _cb.AppendLine("");
        }


        private void WriteColumn(DatabaseColumn column)
        {
            var propertyName = column.NetName;
            var dataType = _dataTypeWriter.Write(column);
            var isFk = column.IsForeignKey && column.ForeignKeyTable != null;
            if (isFk)
            {
                if (CodeTarget == CodeTarget.PocoEntityCodeFirst && column.IsPrimaryKey)
                {
                    //if it's a primary key AND foreign key, CF requires a scalar property
                    _cb.AppendAutomaticProperty(dataType, propertyName + "Id", true);
                }
                dataType = column.ForeignKeyTable.NetName;
            }

            _dataAnnotationWriter.Write(_cb, column);
            //for code first, ordinary properties are non-virtual. 
            var useVirtual = (CodeTarget != CodeTarget.PocoEntityCodeFirst || isFk);
            _cb.AppendAutomaticProperty(dataType, propertyName, useVirtual);
        }



        private void WriteCompositeKeyClass(string className)
        {
            //CodeFirst can cope with multi-keys
            if (CodeTarget == CodeTarget.PocoEntityCodeFirst) return;

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
