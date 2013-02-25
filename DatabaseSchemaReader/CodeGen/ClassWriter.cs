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
                PrepareSchemaNames.Prepare(_table.DatabaseSchema, _codeWriterSettings.Namer);
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
                        WriteColumn(column, false);
                    }
                }
            }
            else
            {
                //single primary key column
                var column = _table.PrimaryKeyColumn;
                //could be a view or have no primary key
                if (column != null)
                    WriteColumn(column);
            }

            foreach (var column in _table.Columns)
            {
                if (column.IsPrimaryKey) continue;
                if (column.IsForeignKey) continue;
                WriteColumn(column);
            }

            foreach (var foreignKey in _table.ForeignKeys)
            {
                WriteForeignKey(foreignKey);
            }

            WriteForeignKeyCollections();

            if (!_table.HasCompositeKey && _codeWriterSettings.CodeTarget != CodeTarget.PocoRiaServices)
            {
                var overrider = new OverrideWriter(_cb, _table, _codeWriterSettings.Namer);
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
                if (_table.IsSharedPrimaryKey(foreignKey))
                {
                    //type and property name are the same
                    _cb.AppendAutomaticProperty(foreignKey.NetName, foreignKey.NetName, true);
                    continue;
                }

                //the other table may have more than one fk pointing at this table
                var fks = _table.InverseForeignKeys(foreignKey);
                foreach (var fk in fks)
                {
                    var propertyName = _codeWriterSettings.Namer.ForeignKeyCollectionName(_table.Name, foreignKey, fk);
                    var dataType = listType + foreignKey.NetName + ">";
                    WriteForeignKeyChild(propertyName, dataType);
                }
            }
        }



        private void WriteForeignKeyChild(string propertyName, string dataType)
        {
            if (_codeWriterSettings.CodeTarget == CodeTarget.PocoRiaServices)
                _cb.AppendLine("[Include]");
            _cb.AppendAutomaticCollectionProperty(dataType, propertyName, IsNHibernate());
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
            var propertyName = _codeWriterSettings.Namer.NameCollection(target.NetName);
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
            var propertyName = _codeWriterSettings.Namer.NameCollection(target.NetName);
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
                    if (_table.IsSharedPrimaryKey(foreignKey))
                    {
                        continue;
                    }
                    var fks = _table.InverseForeignKeys(foreignKey);
                    foreach (DatabaseConstraint fk in fks)
                    {
                        var propertyName = _codeWriterSettings.Namer.ForeignKeyCollectionName(_table.Name, foreignKey, fk);
                        var dataType = "List<" + foreignKey.NetName + ">";
                        _cb.AppendLine(propertyName + " = new " + dataType + "();");
                    }
                }
            }
            _cb.AppendLine("");
        }

        private void WriteColumn(DatabaseColumn column)
        {
            WriteColumn(column, false);
        }

        private void WriteColumn(DatabaseColumn column, bool notNetName)
        {
            var propertyName = column.NetName;
            // KL: Ensures that property name doesn't match class name
            if (propertyName == column.Table.NetName)
            {
                propertyName = string.Format("{0}Column", propertyName);
            }
            var dataType = _dataTypeWriter.Write(column);

            if (column.IsPrimaryKey && column.IsForeignKey)
            {
                //a foreign key will be written, so we need to avoid a collision
                var refTable = column.ForeignKeyTable;
                var fkDataType = refTable != null ? refTable.NetName : column.ForeignKeyTableName;
                if (fkDataType == propertyName)
                    notNetName = true;
            }

            if (notNetName)
            {
                //in EF, you want a fk Id property
                //must not conflict with entity fk name
                propertyName += "Id";
            }

            _dataAnnotationWriter.Write(_cb, column);
            //for code first, ordinary properties are non-virtual. 
            var useVirtual = !IsEntityFramework();
            _cb.AppendAutomaticProperty(dataType, propertyName, useVirtual);
        }


        /// <summary>
        /// KL:
        /// Similar to WriteColumn. Will send the appropriate dataType and propertyName to
        /// _cb.AppendAutomaticProperty to be written.
        /// 
        /// This method was needed to support composite foreign keys.
        /// </summary>
        /// <param name="foreignKey"></param>
        private void WriteForeignKey(DatabaseConstraint foreignKey)
        {
            // get the reference table
            var refTable = foreignKey.ReferencedTable(_table.DatabaseSchema);

            if (refTable == null)
            {
                //we can't find the foreign key table, so just write the columns
                WriteForeignKeyColumns(foreignKey, "");
                return;
            }

            var propertyName = _codeWriterSettings.Namer.ForeignKeyName(_table, foreignKey);
            var dataType = refTable.NetName;

            _cb.AppendAutomaticProperty(dataType, propertyName);

            if (IsEntityFramework() && _codeWriterSettings.UseForeignKeyIdProperties)
            {
                WriteForeignKeyColumns(foreignKey, propertyName);
            }
        }

        private void WriteForeignKeyColumns(DatabaseConstraint foreignKey, string propertyName)
        {
            //for code first, we may have to write scalar properties
            //1 if the fk is also a pk
            //2 if they selected use Foreign Key Ids
            foreach (var columnName in foreignKey.Columns)
            {
                var column = _table.FindColumn(columnName);
                if (column == null) continue;
                //primary keys are already been written
                if (!column.IsPrimaryKey)
                {
                    WriteColumn(column, propertyName.Equals(column.NetName));
                }
            }
        }

        private void WriteCompositeKeyClass(string className)
        {
            //CodeFirst can cope with multi-keys
            if (IsEntityFramework()) return;

            using (_cb.BeginNest("public class " + className + "Key", "Class representing " + _table.Name + " composite key"))
            {
                foreach (var column in _table.Columns.Where(x => x.IsPrimaryKey))
                {
                    WriteColumn(column);
                }

                var overrider = new OverrideWriter(_cb, _table, _codeWriterSettings.Namer);
                overrider.NetName = className + "Key";
                overrider.AddOverrides();
            }
        }
    }
}
