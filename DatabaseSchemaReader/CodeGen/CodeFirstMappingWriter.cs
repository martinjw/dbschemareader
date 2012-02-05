using System;
using System.Globalization;
using System.Linq;
using System.Text;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.CodeGen
{
    class CodeFirstMappingWriter
    {
        //http://msdn.microsoft.com/en-us/library/hh295844%28v=vs.103%29.aspx

        //many to many
        //modelBuilder.Entity<Course>()
        //    .HasMany(t => t.Instructors)
        //    .WithMany(t => t.Courses)
        //    .Map(m =>
        //    {
        //        m.ToTable("CourseInstructor");
        //        m.MapLeftKey("CourseID");
        //        m.MapRightKey("InstructorID");
        //    });


        private readonly DatabaseTable _table;
        private readonly string _ns;
        private readonly ClassBuilder _cb;

        public CodeFirstMappingWriter(DatabaseTable table, string ns)
        {
            _ns = ns;
            _table = table;
            _cb = new ClassBuilder();
        }

        public string Write()
        {
            _cb.AppendLine("using System.ComponentModel.DataAnnotations;");
            _cb.AppendLine("using System.Data.Entity.ModelConfiguration;");

            var className = _table.NetName + "Mapping";

            using (_cb.BeginNest("namespace " + _ns + ".Mapping"))
            {
                using (_cb.BeginNest("public class " + className + " : EntityTypeConfiguration<" + _table.NetName + ">", "Class mapping to " + _table.Name + " table"))
                {
                    using (_cb.BeginNest("public " + className + "()", "Constructor"))
                    {
                        MapTableName();

                        AddPrimaryKey();

                        _cb.AppendLine("// Properties");
                        WriteColumns();

                        _cb.AppendLine("// Navigation properties");
                        foreach (var foreignKeyChild in _table.ForeignKeyChildren)
                        {
                            WriteForeignKeyCollection(foreignKeyChild);
                        }
                    }
                }
            }

            return _cb.ToString();
        }

        private void MapTableName()
        {
            //if it's the same, no need to map
            if (_table.Name == _table.NetName) return;

            var name = _table.Name;
            _cb.AppendLine("//table");
            if (!string.IsNullOrEmpty(_table.SchemaOwner) && _table.SchemaOwner != "dbo")
            {
                _cb.AppendFormat("ToTable(\"{0}\", \"{1}\");", name, _table.SchemaOwner);
            }
            else
            {
                _cb.AppendFormat("ToTable(\"{0}\");", name);
            }
        }

        private void AddPrimaryKey()
        {
            if (_table.PrimaryKey == null || _table.PrimaryKey.Columns.Count == 0)
            {
                _cb.AppendLine("//TODO- you MUST add a primary key!");
                return;
            }
            _cb.AppendLine("// Primary key");
            if (_table.HasCompositeKey)
            {
                AddCompositePrimaryKey();
                return;
            }

            var idColumn = _table.PrimaryKeyColumn;

            //"Id" or class"Id" is default
            if (idColumn.NetName.Equals("Id", StringComparison.OrdinalIgnoreCase))
                return;
            if (idColumn.NetName.Equals(_table.NetName + "Id", StringComparison.OrdinalIgnoreCase))
                return;

            _cb.AppendLine("HasKey(x => x." + idColumn.NetName + ");");
        }

        private void AddCompositePrimaryKey()
        {
            var keys = string.Join(", ",
                    _table.Columns
                    .Where(x => x.IsPrimaryKey)
                //primary keys must be scalar so if it's a foreign key use the Id mirror property
                    .Select(x => "x." + x.NetName + (x.IsForeignKey ? "Id" : string.Empty))
                    .ToArray());
            //double braces for a format
            _cb.AppendFormat("HasKey(x => new {{ {0} }});", keys);
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
                WriteForeignKey(column);
                return;
            }

            var propertyName = column.NetName;
            var sb = new StringBuilder();
            sb.AppendFormat(CultureInfo.InvariantCulture, "Property(x => x.{0})", propertyName);
            if (propertyName != column.Name)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, ".HasColumnName(\"{0}\")", column.Name);
            }
            if (column.IsPrimaryKey && !column.IsIdentity)
            {
                //assumed to be identity by default
                sb.AppendFormat(CultureInfo.InvariantCulture, ".HasDatabaseGeneratedOption(DatabaseGeneratedOption.None)");
            }

            var dt = column.DataType;
            if (dt != null)
            {
                //nvarchar(max) may be -1
                if (dt.IsString && column.Length > 0 && column.Length < 1073741823)
                {
                    sb.AppendFormat(CultureInfo.InvariantCulture, ".HasMaxLength({0})", column.Length.GetValueOrDefault());
                }
                if (dt.TypeName == "TIMESTAMP")
                {
                    sb.Append(".IsConcurrencyToken().HasColumnType(\"timestamp\").HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed)");
                }
            }

            if (!column.Nullable)
            {
                sb.Append(".IsRequired()");
            }

            sb.Append(";");
            _cb.AppendLine(sb.ToString());
        }

        private void WriteForeignKey(DatabaseColumn column)
        {
            var propertyName = column.NetName;
            var sb = new StringBuilder();
            sb.AppendFormat(CultureInfo.InvariantCulture, "Has{0}(x => x.{1})",
                column.Nullable ? "Optional" : "Required",
                propertyName);
            //then map the inverse with our foreign key children convention
            sb.AppendFormat(CultureInfo.InvariantCulture, ".WithMany(c => c.{0}Collection)", column.Table.NetName);
            if (column.IsPrimaryKey)
            {
                //for pk/fk we have a mirror property
                sb.AppendFormat(CultureInfo.InvariantCulture, ".HasForeignKey(c => c.{0}Id)", propertyName);
            }
            else
            {
                //otherwise specify the underlying column name
                sb.AppendFormat(CultureInfo.InvariantCulture, ".Map(m => m.MapKey(\"{0}\"))", column.Name);
            }
            //could look up cascade rule here
            sb.Append(";");
            _cb.AppendLine(sb.ToString());
        }

        private void WriteForeignKeyCollection(DatabaseTable foreignKeyChild)
        {
            if (foreignKeyChild.IsManyToManyTable())
            {
                WriteManyToManyForeignKeyCollection(foreignKeyChild);
                return;
            }

            var foreignKeyTable = foreignKeyChild.Name;
            var childClass = foreignKeyChild.NetName;
            var foreignKey = foreignKeyChild.ForeignKeys.FirstOrDefault(fk => fk.RefersToTable == _table.Name);
            if (foreignKey == null) return; //corruption in our database
            //we won't deal with composite keys
            //var fkColumn = foreignKey.Columns.FirstOrDefault();

            _cb.AppendFormat("//Foreign key to {0} ({1})", foreignKeyTable, childClass);
            var propertyName = childClass + "Collection";

            var sb = new StringBuilder();
            sb.AppendFormat(CultureInfo.InvariantCulture, "HasMany(x => x.{0})", propertyName);
            //specify the opposite direction? Probably not needed

            sb.Append(";");
            _cb.AppendLine(sb.ToString());
        }

        private void WriteManyToManyForeignKeyCollection(DatabaseTable foreignKeyChild)
        {
            var otherEnd = foreignKeyChild.ManyToManyTraversal(_table);
            _cb.AppendLine("// Many to many foreign key to " + otherEnd.Name);
            var childClass = otherEnd.NetName;
            var propertyName = childClass + "Collection";

            var sb = new StringBuilder();
            sb.AppendFormat(CultureInfo.InvariantCulture, "HasMany(x => x.{0})", propertyName);
            sb.AppendFormat(CultureInfo.InvariantCulture, ".WithMany(z => z.{0}Collection)", _table.NetName);
            _cb.AppendLine(sb.ToString());
            using (_cb.BeginNest(".Map(map => "))
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
