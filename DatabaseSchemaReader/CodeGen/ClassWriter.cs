using System.Diagnostics;
using System.Linq;
using DatabaseSchemaReader.CodeGen.CodeFirst;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.CodeGen
{
    /// <summary>
    /// Turns a specified <see cref="DatabaseTable"/> into a C# class
    /// </summary>
    public class ClassWriter
    {
        private readonly DatabaseTable _table;
        private readonly ClassBuilder _cb;
        private readonly DataTypeWriter _dataTypeWriter = new DataTypeWriter();
        private DataAnnotationWriter _dataAnnotationWriter;
        private readonly CodeWriterSettings _codeWriterSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassWriter"/> class.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="codeWriterSettings">The code writer settings.</param>
        public ClassWriter(DatabaseTable table, CodeWriterSettings codeWriterSettings)
        {
            _codeWriterSettings = codeWriterSettings;
            _table = table;
            _cb = new ClassBuilder();
        }

        /// <summary>
        /// Writes the C# code of the table
        /// </summary>
        /// <returns></returns>
        public string Write()
        {
            var codeTarget = _codeWriterSettings.CodeTarget;
            _dataAnnotationWriter = new DataAnnotationWriter(IsEntityFramework(), _codeWriterSettings);
            var className = _table.NetName;
            if (string.IsNullOrEmpty(className) && _table.DatabaseSchema != null)
            {
                PrepareSchemaNames.Prepare(_table.DatabaseSchema);
                className = _table.NetName;
            }
            _dataTypeWriter.CodeTarget = codeTarget;

            WriteNamespaces();

            if (!string.IsNullOrEmpty(_codeWriterSettings.Namespace))
            {
                _cb.BeginNest("namespace " + _codeWriterSettings.Namespace);
            }

            if (codeTarget == CodeTarget.PocoRiaServices)
            {
                WriteRiaClass(className);
            }
            else
            {
                var tableOrView = _table is DatabaseView ? "view" : "table";
                using (_cb.BeginNest("public class " + className, "Class representing " + _table.Name + " " + tableOrView))
                {
                    WriteClassMembers(className);
                }
            }

            if (_table.HasCompositeKey)
            {
                WriteCompositeKeyClass(className);
            }

            if (!string.IsNullOrEmpty(_codeWriterSettings.Namespace))
            {
                _cb.EndNest();
            }

            return _cb.ToString();
        }

        private void WriteRiaClass(string className)
        {
            _cb.AppendLine("[MetadataType(typeof(" + className + "." + className + "Metadata))]");
            using (_cb.BeginBrace("public partial class " + className))
            {
                //write the buddy class
                using (_cb.BeginNest("internal sealed class " + className + "Metadata"))
                {
                    WriteClassMembers(className);
                }
            }
        }

        private void WriteClassMembers(string className)
        {
            if (_codeWriterSettings.CodeTarget == CodeTarget.PocoRiaServices)
            {
                RiaServicesWriter.WritePrivateConstructor(className, _cb);
            }
            else
            {
                InitializeCollectionsInConstructor(className);
            }

            if (_table.HasCompositeKey)
            {
                if (!IsEntityFramework())
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

            if (!_table.HasCompositeKey && _codeWriterSettings.CodeTarget != CodeTarget.PocoRiaServices)
            {
                var overrider = new OverrideWriter(_cb, _table);
                overrider.AddOverrides();
            }
        }

        private bool IsCodeFirst()
        {
            return _codeWriterSettings.CodeTarget == CodeTarget.PocoEntityCodeFirst;
        }

        private bool IsEntityFramework()
        {
            return _codeWriterSettings.CodeTarget == CodeTarget.PocoEntityCodeFirst ||
                _codeWriterSettings.CodeTarget == CodeTarget.PocoRiaServices;
        }

        private bool IsNHibernate()
        {
            return _codeWriterSettings.CodeTarget == CodeTarget.PocoNHibernateFluent ||
                _codeWriterSettings.CodeTarget == CodeTarget.PocoNHibernateHbm;
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
            if (IsEntityFramework()) listType = "ICollection<";
            foreach (var foreignKey in _table.ForeignKeyChildren)
            {
                if (foreignKey.IsManyToManyTable() && IsCodeFirst())
                {
                    WriteManyToManyCollection(foreignKey);
                    continue;
                }

                if(_codeWriterSettings.CodeTarget == CodeTarget.PocoRiaServices)
                    _cb.AppendLine("[Include]");
                var propertyName = _codeWriterSettings.NameCollection(foreignKey.NetName);
                var dataType = listType + foreignKey.NetName + ">";
                _cb.AppendAutomaticCollectionProperty(dataType, propertyName, IsNHibernate());
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
            var propertyName = _codeWriterSettings.NameCollection(target.NetName);
            var dataType = "ICollection<" + target.NetName + ">";
            _cb.AppendAutomaticCollectionProperty(dataType, propertyName, IsNHibernate());

        }

        private void WriteManyToManyInitialize(DatabaseTable foreignKey)
        {
            //look over the junction table to find the other many-to-many end
            var target = foreignKey.ManyToManyTraversal(_table);
            if (target == null)
            {
                return;
            }
            var propertyName = _codeWriterSettings.NameCollection(target.NetName);
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
                    if (foreignKey.IsManyToManyTable() && IsCodeFirst())
                    {
                        WriteManyToManyInitialize(foreignKey);
                        continue;
                    }
                    var propertyName = _codeWriterSettings.NameCollection(foreignKey.NetName);
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
                if (IsEntityFramework() && (column.IsPrimaryKey || _codeWriterSettings.UseForeignKeyIdProperties))
                {
                    //if it's a primary key AND foreign key, CF requires a scalar property
                    //optionally allow a shadow Id property to be created (convenient for CF)
                    _cb.AppendAutomaticProperty(dataType, propertyName + "Id", true);
                }
                dataType = column.ForeignKeyTable.NetName;
            }

            _dataAnnotationWriter.Write(_cb, column);
            //for code first, ordinary properties are non-virtual. 
            var useVirtual = (!IsEntityFramework() || isFk);
            _cb.AppendAutomaticProperty(dataType, propertyName, useVirtual);
        }



        private void WriteCompositeKeyClass(string className)
        {
            //CodeFirst can cope with multi-keys
            if (IsEntityFramework()) return;

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
