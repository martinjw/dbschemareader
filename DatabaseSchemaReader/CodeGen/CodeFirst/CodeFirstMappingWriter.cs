using System;
using System.Globalization;
using System.Linq;
using System.Text;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.CodeGen.CodeFirst
{
    class CodeFirstMappingWriter
    {
        //http://msdn.microsoft.com/en-us/library/hh295844%28v=vs.103%29.aspx

        private readonly DatabaseTable _table;
        private readonly CodeWriterSettings _codeWriterSettings;
        private readonly MappingNamer _mappingNamer;
        private readonly ClassBuilder _cb;
        private DatabaseTable _inheritanceTable;

        public CodeFirstMappingWriter(DatabaseTable table, CodeWriterSettings codeWriterSettings, MappingNamer mappingNamer)
        {
            if (table == null) throw new ArgumentNullException("table");
            if (mappingNamer == null) throw new ArgumentNullException("mappingNamer");

            _codeWriterSettings = codeWriterSettings;
            _mappingNamer = mappingNamer;
            _table = table;
            _cb = new ClassBuilder();
        }

        /// <summary>
        /// Gets the name of the mapping class.
        /// </summary>
        /// <value>
        /// The name of the mapping class.
        /// </value>
        public string MappingClassName { get; private set; }

        public string Write()
        {
            WriteUsings();

            MappingClassName = _mappingNamer.NameMappingClass(_table.NetName);

            _inheritanceTable = _table.FindInheritanceTable();

            using (_cb.BeginNest("namespace " + _codeWriterSettings.Namespace + ".Mapping"))
            {
                var cdef = (_codeWriterSettings.CodeTarget == CodeTarget.PocoEfCore)
                    ? "public static class " + MappingClassName
                    : "public class " + MappingClassName + " : EntityTypeConfiguration<" + _table.NetName + ">";
                using (_cb.BeginNest(cdef, "Class mapping to " + _table.Name + " table"))
                {
                    var mapMethod = _codeWriterSettings.CodeTarget == CodeTarget.PocoEfCore
                        ? "public static void Map(EntityTypeBuilder<" + _table.NetName + "> b)"
                        : "public " + MappingClassName + "()";
                    using (_cb.BeginNest(mapMethod, "Mapping"))
                    {
                        MapTableName();

                        AddPrimaryKey();

                        _cb.AppendLine("// Properties");
                        WriteColumns();

                        WriteForeignKeys();

                        WriteNavigationProperties();
                    }
                }
            }

            return _cb.ToString();
        }

        private string Builder
        {
            get { return _codeWriterSettings.CodeTarget == CodeTarget.PocoEfCore ? "b." : ""; }
        }

        private void WriteForeignKeys()
        {
            foreach (var foreignKey in _table.ForeignKeys)
            {
                //we inherit from it instead (problem with self-joins)
                if (Equals(foreignKey.ReferencedTable(_table.DatabaseSchema), _inheritanceTable))
                    continue;

                WriteForeignKey(foreignKey);
            }
        }

        private void WriteNavigationProperties()
        {
            if (_table.ForeignKeyChildren.Count == 0) return;

            _cb.AppendLine("// Navigation properties");

            var hasTablePerTypeInheritance =
                (_table.ForeignKeyChildren.Count(fk => _table.IsSharedPrimaryKey(fk)) > 1);

            foreach (var foreignKeyChild in _table.ForeignKeyChildren)
            {
                if (hasTablePerTypeInheritance && _table.IsSharedPrimaryKey(foreignKeyChild))
                    continue;

                WriteForeignKeyCollection(foreignKeyChild);
            }
        }

        private void WriteUsings()
        {
            _cb.AppendLine("using System.ComponentModel.DataAnnotations;");

            if (RequiresDataAnnotationsSchema())
            {
                //in EF v5 DatabaseGeneratedOption is in DataAnnotations.Schema
                _cb.AppendLine("using System.ComponentModel.DataAnnotations.Schema;");
            }
            if (_codeWriterSettings.CodeTarget == CodeTarget.PocoEntityCodeFirst)
            {
                _cb.AppendLine("using System.Data.Entity.ModelConfiguration;");
            }
            else
            {
                _cb.AppendLine("using Microsoft.EntityFrameworkCore;");
                _cb.AppendLine("using Microsoft.EntityFrameworkCore.Metadata.Builders;");
            }
        }

        private bool RequiresDataAnnotationsSchema()
        {
            if (_table.PrimaryKeyColumn != null && !_table.PrimaryKeyColumn.IsAutoNumber) return true;
            foreach (var column in _table.Columns)
            {
                if (column.IsComputed) return true;
                if (column.DataType != null &&
                    column.DataType.TypeName.Equals("timestamp", StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        private void MapTableName()
        {
            //NB CodeFirst automatically pluralizes (PluralizingTableNameConvention)
            //If the name is singular in database, it won't work...
            //if (_table.Name == _table.NetName) return;

            //Safer to always specify table name!

            var name = _table.Name;
            _cb.AppendLine("//table");
            if (!string.IsNullOrEmpty(_table.SchemaOwner) && _table.SchemaOwner != "dbo")
            {
                _cb.AppendFormat(Builder + "ToTable(\"{0}\", \"{1}\");", name, _table.SchemaOwner);
            }
            else
            {
                _cb.AppendFormat(Builder + "ToTable(\"{0}\");", name);
            }
        }

        private void AddPrimaryKey()
        {
            if (_table.PrimaryKey == null || _table.PrimaryKey.Columns.Count == 0)
            {
                if (_table is DatabaseView)
                {
                    AddCompositePrimaryKeyForView();
                    return;
                }
                _cb.AppendLine("//TODO- you MUST add a primary key!");
                return;
            }
            if (_table.HasCompositeKey)
            {
                AddCompositePrimaryKey();
                return;
            }

            var idColumn = _table.PrimaryKeyColumn;

            if (_inheritanceTable != null)
            {
                //already mapped by inheritance, but point out that EF TPT can't cope
                if (idColumn.Name != _inheritanceTable.PrimaryKeyColumn.Name)
                {
                    _cb.AppendLine("// In TPT, primary key columns must be named identically!");
                }
                return;
            }

            //in case PrepareSchemaNames.Prepare(schema) not done
            var netName = idColumn.NetName ?? idColumn.Name;

            //IdKeyDiscoveryConvention: "Id" or class"Id" is default
            if (netName.Equals("Id", StringComparison.OrdinalIgnoreCase))
                return;
            if (netName.Equals(_table.NetName + "Id", StringComparison.OrdinalIgnoreCase))
                return;

            _cb.AppendLine("// Primary key");
            _cb.AppendLine(Builder + "HasKey(x => x." + netName + ");");
        }

        private void AddCompositePrimaryKey()
        {
            var keys = string.Join(", ",
                    _table.Columns
                    .Where(x => x.IsPrimaryKey)
                    //primary keys must be scalar so if it's a foreign key use the Id mirror property
                    //.Select(x => "x." + x.NetName + (x.IsForeignKey ? "Id" : string.Empty))
                    .Select(x => "x." + ClassWriter.PropertyName(x))
                    .ToArray());
            _cb.AppendLine("// Primary key (composite)");
            //double braces for a format
            _cb.AppendFormat(Builder + "HasKey(x => new {{ {0} }});", keys);
        }

        private void AddCompositePrimaryKeyForView()
        {
            //we make all the non-nullable columns as keys.
            //Nullable pks make EF die (EntityKey.AddHashValue NullReference)
            var candidatePrimaryKeys = _table.Columns.Where(x => !x.Nullable).ToArray();
            if (!candidatePrimaryKeys.Any())
            {
                candidatePrimaryKeys = _table.Columns.ToArray();
                _cb.AppendLine("// Warning: nullable columns may cause EntityKey errors. Try AsNoTracking()");
            }
            var keys = string.Join(", ",
                    candidatePrimaryKeys
                    //primary keys must be scalar so if it's a foreign key use the Id mirror property
                    .Select(x => "x." + x.NetName + (x.IsForeignKey ? "Id" : string.Empty))
                    .ToArray());
            _cb.AppendLine("// Primary key (composite for view)");
            //double braces for a format
            _cb.AppendFormat(Builder + "HasKey(x => new {{ {0} }});", keys);
        }

        private void WriteColumns()
        {
            //map the columns
            foreach (var column in _table.Columns)
            {
                WriteColumn(column);
            }
        }

        private void WriteColumn(DatabaseColumn column)
        {
            if (column.IsForeignKey)
            {
                //WriteForeignKey(column);
                return;
            }

            var propertyName = column.NetName;
            if (string.IsNullOrEmpty(propertyName)) propertyName = column.Name;
            var sb = new StringBuilder();
            if (column.IsPrimaryKey)
            {
                //let's comment it to make it explicit
                _cb.AppendLine("//  " + propertyName + " is primary key" +
                    ((column.IsAutoNumber) ? " (identity)" : ""));
            }

            sb.AppendFormat(CultureInfo.InvariantCulture, Builder + "Property(x => x.{0})", propertyName);
            if (propertyName != column.Name)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, ".HasColumnName(\"{0}\")", column.Name);
            }
            if (column.IsPrimaryKey && !column.IsAutoNumber)
            {
                if (_codeWriterSettings.CodeTarget == CodeTarget.PocoEntityCodeFirst)
                {
                    //assumed to be identity by default
                    sb.AppendFormat(CultureInfo.InvariantCulture,
                        ".HasDatabaseGeneratedOption(DatabaseGeneratedOption.None)");
                }
            }
            if (column.IsComputed)
            {
                if (_codeWriterSettings.CodeTarget == CodeTarget.PocoEfCore)
                {
                    sb.AppendLine(".HasComputedColumnSql(\"" + column.ComputedDefinition + "\")");
                }
                else
                {
                    sb.AppendLine("//NB cannot specify definition, so DDL in CreateDatabase and migrations will fail");
                    sb.AppendLine(".HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed)");
                }
            }
            else
            {
                WriteColumnType(column, sb);

                if (!column.Nullable)
                {
                    sb.Append(".IsRequired()");
                }
            }

            sb.Append(";");
            _cb.AppendLine(sb.ToString());
        }

        private void WriteColumnType(DatabaseColumn column, StringBuilder sb)
        {
            var dt = column.DataType;
            if (dt == null)
            {
                //we don't know the type, so state it explicitly
                sb.AppendFormat(CultureInfo.InvariantCulture,
                                ".HasColumnType(\"{0}\")",
                                column.DbDataType);
                return;
            }
            //nvarchar(max) may be -1
            if (dt.IsStringClob && _codeWriterSettings.CodeTarget == CodeTarget.PocoEntityCodeFirst)
            {
                sb.Append(".IsMaxLength()");
                return;
            }
            if (dt.IsString)
            {
                if (_codeWriterSettings.CodeTarget == CodeTarget.PocoEntityCodeFirst && (column.Length == -1 || column.Length >= 1073741823))
                {
                    //MaxLength (and Text/Ntext/Clob) should be marked explicitly
                    sb.Append(".IsMaxLength()");
                }
                else if (column.Length > 0 && column.Length < 1073741823)
                {
                    //otherwise specify an explicit max size
                    sb.AppendFormat(CultureInfo.InvariantCulture, ".HasMaxLength({0})",
                                    column.Length.GetValueOrDefault());
                }
                return;
            }
            if (dt.TypeName.Equals("money", StringComparison.OrdinalIgnoreCase))
            {
                sb.Append(".HasColumnType(\"money\")");
                return;
            }
            if (dt.IsNumeric && !dt.IsInt && !dt.IsFloat && column.Precision.HasValue) //decimal
            {
                if (Type.GetType(dt.NetDataType) != typeof(decimal))
                    return; //short and long don't have precision either
                var cst = dt.NetCodeName(column);
                if (cst == "int" || cst == "short" || cst == "long")
                {
                    //we've decided it's an integer type, HasPrecision not valid
                    return;
                }
                if (column.Precision != 18 || column.Scale != 0)
                {
                    sb.AppendFormat(CultureInfo.InvariantCulture, ".HasPrecision({0}, {1})",
                                    column.Precision.GetValueOrDefault(),
                                    column.Scale.GetValueOrDefault());
                    return;
                }
            }
            //special types (SQLServer only for now) that can be explicitly mapped
            if (dt.TypeName.Equals("image", StringComparison.OrdinalIgnoreCase))
            {
                sb.Append(".HasColumnType(\"image\")");
                return;
            }
            if (dt.TypeName.Equals("timestamp", StringComparison.OrdinalIgnoreCase))
            {
                if (_codeWriterSettings.CodeTarget == CodeTarget.PocoEfCore)
                {
                    sb.Append(".HasColumnType(\"timestamp\").ValueGeneratedOnAddOrUpdate().IsConcurrencyToken();");
                }
                else
                {
                    sb.Append(".IsConcurrencyToken().HasColumnType(\"timestamp\").HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed)");
                }
            }
        }

        private void WriteForeignKey(DatabaseConstraint foreignKey)
        {
            var propertyName = _codeWriterSettings.Namer.ForeignKeyName(_table, foreignKey);
            if (string.IsNullOrEmpty(propertyName)) return;

            var columnName = foreignKey.Columns.FirstOrDefault();
            var columns = foreignKey.Columns.Select(colName => _table.FindColumn(colName)).ToList();
            var optional = columns.All(col => col.Nullable);
            //foreign key is a primary key = shared with another table
            var isPrimaryKey = columns.All(col => col.IsPrimaryKey);

            var sb = new StringBuilder();

            if (_codeWriterSettings.CodeTarget == CodeTarget.PocoEntityCodeFirst)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "Has{0}(x => x.{1})",
                    optional ? "Optional" : "Required",
                    propertyName);

                //1:1 shared primary key
                if (isPrimaryKey)
                {
                    sb.Append(";");
                    _cb.AppendLine(sb.ToString());
                    return;
                }
            }
            else
            {
                //EF Core
                sb.AppendFormat(CultureInfo.InvariantCulture, "b.HasOne(x => x.{0})", propertyName);
                //1:1 shared primary key
                if (isPrimaryKey)
                {
                    sb.Append(".WithOne();");
                    _cb.AppendLine(sb.ToString());
                    return;
                }
            }

            //then map the inverse with our foreign key children convention
            var fkPropertyName = _codeWriterSettings.Namer.ForeignKeyCollectionName(foreignKey.RefersToTable, _table, foreignKey);

            sb.AppendFormat(CultureInfo.InvariantCulture, ".WithMany(c => c.{0})", fkPropertyName);

            if (_codeWriterSettings.UseForeignKeyIdProperties)
            {
                //for pk/fk we have a mirror property
                //TODO: don't use Id here
                var fkIdName = propertyName + "Id";
                _cb.AppendFormat(Builder + "Property(x => x.{0}).HasColumnName(\"{1}\");", fkIdName, columnName);
                sb.AppendFormat(CultureInfo.InvariantCulture, ".HasForeignKey(c => c.{0})", fkIdName);
            }
            else if (_codeWriterSettings.CodeTarget == CodeTarget.PocoEntityCodeFirst)
            {
                //otherwise specify the underlying column name
                sb.AppendFormat(CultureInfo.InvariantCulture, ".Map(m => m.MapKey(\"{0}\"))", columnName);
            }
            //could look up cascade rule here
            sb.Append(";");
            _cb.AppendLine(sb.ToString());
        }

        private void WriteForeignKeyCollection(DatabaseTable foreignKeyChild)
        {
            if (foreignKeyChild.IsManyToManyTable() && _codeWriterSettings.CodeTarget == CodeTarget.PocoEntityCodeFirst)
            {
                WriteManyToManyForeignKeyCollection(foreignKeyChild);
                return;
            }

            var foreignKeyTable = foreignKeyChild.Name;
            var childClass = foreignKeyChild.NetName;
            var foreignKey = foreignKeyChild.ForeignKeys.FirstOrDefault(fk => fk.RefersToTable == _table.Name);
            if (foreignKey == null) return; //corruption in our database
            //we won't deal with composite keys
            if (_table.IsSharedPrimaryKey(foreignKeyChild))
            {
                _cb.AppendFormat("//shared primary key to {0} ({1})", foreignKeyTable, childClass);
                if (_codeWriterSettings.CodeTarget == CodeTarget.PocoEntityCodeFirst)
                {
                    _cb.AppendFormat(Builder + "HasOptional(x => x.{0});", childClass);
                }
                else
                {
                    _cb.AppendFormat(Builder + "HasOne(x => x.{0}).WithOne();", childClass);
                }
                return;
            }

            //the foreign keys that point at this table from the other table
            var fks = _table.InverseForeignKeys(foreignKeyChild);
            foreach (var fk in fks)
            {
                _cb.AppendFormat("//Foreign key to {0} ({1})", foreignKeyTable, childClass);
                var propertyName = _codeWriterSettings.Namer.ForeignKeyCollectionName(_table.Name, foreignKeyChild, fk);
                if (_codeWriterSettings.CodeTarget == CodeTarget.PocoEntityCodeFirst)
                {
                    //specify the opposite direction? Probably not needed
                    _cb.AppendFormat("HasMany(x => x.{0});", propertyName);
                }
                else
                {
                    //EF Core v1 - inverse direction is required in Core
                    var dependentPropertyName = _codeWriterSettings.Namer.ForeignKeyName(foreignKeyChild, fk);

                    _cb.AppendFormat("b.HasMany(x => x.{0}).WithOne(d => d.{1});", propertyName, dependentPropertyName);
                }
            }
        }

        private void WriteManyToManyForeignKeyCollection(DatabaseTable foreignKeyChild)
        {
            var otherEnd = foreignKeyChild.ManyToManyTraversal(_table);
            _cb.AppendLine("// Many to many foreign key to " + otherEnd.Name);

            var childClass = otherEnd.NetName;
            var propertyName = _codeWriterSettings.Namer.NameCollection(childClass);
            var reverseName = _codeWriterSettings.Namer.NameCollection(_table.NetName);

            var sb = new StringBuilder();
            sb.AppendFormat(CultureInfo.InvariantCulture, Builder + "HasMany(x => x.{0})", propertyName);
            sb.AppendFormat(CultureInfo.InvariantCulture, ".WithMany(z => z.{0})", reverseName);
            _cb.AppendLine(sb.ToString());
            using (_cb.BeginBrace(".Map(map => "))
            {
                _cb.AppendLine("map.ToTable(\"" + foreignKeyChild.Name + "\");");
                //left key = HasMany side
                var cols = foreignKeyChild.ForeignKeys
                    .First(x => x.RefersToTable == _table.Name)
                    .Columns.Select(x => '"' + x + '"')
                    .ToArray();
                var leftColumns = string.Join(", ", cols);
                _cb.AppendLine("map.MapLeftKey(" + leftColumns + ");");
                //right key = WithMany side
                cols = foreignKeyChild.ForeignKeys
                    .First(x => x.RefersToTable == otherEnd.Name)
                    .Columns.Select(x => '"' + x + '"')
                    .ToArray();
                var rightColumns = string.Join(", ", cols);
                _cb.AppendLine("map.MapRightKey(" + rightColumns + ");");
            }

            _cb.AppendLine(");");
        }
    }
}