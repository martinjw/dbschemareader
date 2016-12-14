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
        private DatabaseTable _inheritanceTable;
        //private CodeInserter _codeInserter;

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
            //_codeInserter = codeWriterSettings.CodeInserter;
            //if (_codeInserter == null) _codeInserter = new CodeInserter();
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

            _inheritanceTable = _table.FindInheritanceTable();

            WriteNamespaces();
            _codeWriterSettings.CodeInserter.WriteNamespaces(_table, _cb);

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
                var comment = "Class representing " + _table.Name + " " + tableOrView;
                var classDefinition = "public class " + className;
                if (_inheritanceTable != null)
                {
                    classDefinition += " : " + _inheritanceTable.NetName;
                }

                _codeWriterSettings.CodeInserter.WriteTableAnnotations(_table, _cb);

                using (_cb.BeginNest(classDefinition, comment))
                {
                    WriteClassMembers(className);
                }
            }

            if (_table.HasCompositeKey && _inheritanceTable == null)
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

            _codeWriterSettings.CodeInserter.WriteClassMembers(_table, _cb);

            if (_inheritanceTable == null)
                WritePrimaryKey(className);

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

            if (!_table.HasCompositeKey &&
                _codeWriterSettings.CodeTarget != CodeTarget.PocoRiaServices &&
                _inheritanceTable == null)
            {
                var overrider = new OverrideWriter(_cb, _table, _codeWriterSettings.Namer);
                overrider.AddOverrides();
            }
        }

        private void WritePrimaryKey(string className)
        {
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
        }


        //private bool IsCodeFirst()
        //{
        //    return _codeWriterSettings.CodeTarget == CodeTarget.PocoEntityCodeFirst ||
        //        _codeWriterSettings.CodeTarget == CodeTarget.PocoEfCore;
        //}

        private bool IsEntityFramework()
        {
            return _codeWriterSettings.CodeTarget == CodeTarget.PocoEntityCodeFirst ||
                _codeWriterSettings.CodeTarget == CodeTarget.PocoRiaServices ||
                _codeWriterSettings.CodeTarget == CodeTarget.PocoEfCore;
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
            if (_codeWriterSettings.CodeTarget == CodeTarget.PocoEntityCodeFirst &&
                _codeWriterSettings.WriteCodeFirstIndexAttribute &&
                _table.Indexes.Count > 0)
            {
                //Index attribute
                _cb.AppendLine("using System.ComponentModel.DataAnnotations.Schema;");
            }
        }

        private void WriteForeignKeyCollections()
        {
            var listType = "IList<";
            if (IsEntityFramework()) listType = "ICollection<";
            var hasTablePerTypeInheritance =
                (_table.ForeignKeyChildren.Count(fk => _table.IsSharedPrimaryKey(fk)) > 1);

            foreach (var foreignKey in _table.ForeignKeyChildren)
            {
                if (foreignKey.IsManyToManyTable() && _codeWriterSettings.CodeTarget == CodeTarget.PocoEntityCodeFirst)
                {
                    WriteManyToManyCollection(foreignKey);
                    continue;
                }
                if (_table.IsSharedPrimaryKey(foreignKey))
                {
                    if (hasTablePerTypeInheritance)
                        continue;
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
                    if (foreignKey.IsManyToManyTable() && _codeWriterSettings.CodeTarget == CodeTarget.PocoEntityCodeFirst)
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
            var propertyName = PropertyName(column);
            var dataType = _dataTypeWriter.Write(column);

            if (notNetName)
            {
                //in EF, you want a fk Id property
                //must not conflict with entity fk name
                propertyName += "Id";
            }

            _codeWriterSettings.CodeInserter.WriteColumnAnnotations(_table, column, _cb);

            var writeAnnotations = true;
            if (column.IsPrimaryKey &&
                _codeWriterSettings.CodeTarget == CodeTarget.PocoEfCore &&
                _table.PrimaryKey.Columns.Count > 1)
            {
                //EF Core doesn't like [Key] annotations on composite keys
                writeAnnotations = false;
            }
            if(writeAnnotations)
            _dataAnnotationWriter.Write(_cb, column);
            //for code first, ordinary properties are non-virtual. 
            var useVirtual = !IsEntityFramework();
            _cb.AppendAutomaticProperty(dataType, propertyName, useVirtual);
        }

        /// <summary>
        /// Logic for propertyName. Shared with mapping.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns></returns>
        internal static string PropertyName(DatabaseColumn column)
        {
            var propertyName = column.NetName;
            //in case the netName hasn't been set
            if (string.IsNullOrEmpty(propertyName)) propertyName = column.Name;
            // KL: Ensures that property name doesn't match class name
            if (propertyName == column.Table.NetName)
            {
                propertyName = string.Format("{0}Column", propertyName);
            }
            if (column.IsPrimaryKey && column.IsForeignKey)
            {
                //if it's a composite key as well, always write an Id version
                var table = column.Table;
                if (table != null && table.HasCompositeKey)
                {
                    return propertyName + "Id";
                }
                //a foreign key will be written, so we need to avoid a collision
                var refTable = FindForeignKeyTable(column);
                var fkDataType = refTable != null ? refTable.NetName : column.ForeignKeyTableName;
                if (fkDataType == propertyName)
                {
                    //in EF, you want a fk Id property
                    //must not conflict with entity fk name
                    propertyName += "Id";
                }
            }
            return propertyName;
        }

        private static DatabaseTable FindForeignKeyTable(DatabaseColumn column)
        {
            var refTable = column.ForeignKeyTable;
            if (refTable != null) return refTable;
            //column may be in multiple fks, and the model may be broken
            var table = column.Table;
            if (table == null) return null;
            //find the first foreign key containing this column
            var fk = table.ForeignKeys.FirstOrDefault(c => c.Columns.Contains(column.Name));
            if (fk != null)
            {
                refTable = fk.ReferencedTable(table.DatabaseSchema);
            }
            return refTable;
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

            //we inherit from it instead (problem with self-joins)
            if (Equals(refTable, _inheritanceTable)) return;

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
