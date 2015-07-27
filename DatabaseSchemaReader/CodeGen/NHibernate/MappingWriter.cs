using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.CodeGen.NHibernate
{
    class MappingWriter
    {
        private readonly XNamespace _xmlns = "urn:nhibernate-mapping-2.2";
        private readonly DatabaseTable _table;
        private readonly XDocument _doc;
        private readonly XContainer _classElement;
        private readonly CodeWriterSettings _codeWriterSettings;

        public MappingWriter(DatabaseTable table, CodeWriterSettings codeWriterSettings)
        {
            _codeWriterSettings = codeWriterSettings;
            var ns = codeWriterSettings.Namespace;
            _table = table;
            _doc = XDocument.Parse(@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<hibernate-mapping xmlns=""urn:nhibernate-mapping-2.2"" namespace=""" + ns + @""" assembly=""" + ns + @""">
</hibernate-mapping>");
            var hibmap = _doc.Descendants(_xmlns + "hibernate-mapping").First();
            //add the class element
            _classElement = new XElement(_xmlns + "class",
                                         new XAttribute("name", _table.NetName),
                                         new XAttribute("table", SqlSafe(_table.Name)),
                                         _table.SchemaOwner != null ? new XAttribute("schema", SqlSafe(_table.SchemaOwner)) : null,
                //consider this
                                         new XAttribute("dynamic-update", "true"),
                                         new XAttribute("optimistic-lock", "dirty"));
            if (_table is DatabaseView)
            {
                _classElement.Add(new XAttribute("mutable", "false"));
            }
            hibmap.Add(_classElement);
        }

        private static string SqlSafe(string s)
        {
            return "`" + s + "`";
        }

        public string Write()
        {
            AddPrimaryKey();
            WriteColumns();

            var hasTablePerTypeInheritance =
                (_table.ForeignKeyChildren.Count(fk => _table.IsSharedPrimaryKey(fk)) > 1);

            foreach (var foreignKeyChild in _table.ForeignKeyChildren)
            {
                if (hasTablePerTypeInheritance && _table.IsSharedPrimaryKey(foreignKeyChild))
                {
                    WriteJoinedSubClass(foreignKeyChild);
                    continue;
                }

                WriteForeignKeyCollection(foreignKeyChild);
            }

            return _doc.ToString();
        }

        private void WriteJoinedSubClass(DatabaseTable foreignKeyChild)
        {
            var joinedSubClass = new XElement(_xmlns + "joined-subclass");
            joinedSubClass.SetAttributeValue("name", foreignKeyChild.NetName);
            joinedSubClass.SetAttributeValue("table", foreignKeyChild.Name);
            var key = new XElement(_xmlns + "key");
            key.SetAttributeValue("column", foreignKeyChild.PrimaryKeyColumn.Name);
            joinedSubClass.Add(key);
            foreach (var column in foreignKeyChild.Columns.Where(x => !x.IsPrimaryKey))
            {
                if (column.IsForeignKey)
                {
                    var many2One = CreateForeignKey(column);
                    joinedSubClass.Add(many2One);
                    continue;
                }
                var property = CreateColumn(column);
                joinedSubClass.Add(property);
            }
            _classElement.Add(joinedSubClass);
        }

        private void WriteForeignKeyCollection(DatabaseTable foreignKeyChild)
        {
            var foreignKeyTable = foreignKeyChild.Name;
            var childClass = foreignKeyChild.NetName;
            var fks = _table.InverseForeignKeys(foreignKeyChild);
            if (!fks.Any()) return; //corruption in our database

            _classElement.Add(
                new XComment(string.Format(CultureInfo.InvariantCulture, "Foreign key to {0} ({1})", foreignKeyTable, childClass)));

            if (_table.IsSharedPrimaryKey(foreignKeyChild))
            {
                var fk = fks.First();
                if (fk == null) return;
                if (fk.Columns.Count == 1)
                {
                    var one2One = new XElement(_xmlns + "one-to-one");
                    one2One.SetAttributeValue("name", childClass);
                    one2One.SetAttributeValue("constrained", "true");
                    one2One.SetAttributeValue("foreign-key", "none");
                    one2One.SetAttributeValue("class", childClass);
                    _classElement.Add(one2One);
                    return;
                }
            }

            foreach (var fk in fks)
            {
                var propertyName = _codeWriterSettings.Namer.ForeignKeyCollectionName(_table.Name, foreignKeyChild, fk);

                var fkColumn = fk.Columns.FirstOrDefault();

                var bag = new XElement(_xmlns + "bag");
                bag.SetAttributeValue("name", propertyName);
                //bag.SetAttributeValue("access", "nosetter.camelcase-underscore");
                bag.SetAttributeValue("table", SqlSafe(foreignKeyTable));
                if (foreignKeyChild.SchemaOwner != null)
                    bag.SetAttributeValue("schema", SqlSafe(foreignKeyChild.SchemaOwner));
                bag.SetAttributeValue("cascade", "all-delete-orphan");
                //assume child always controls collection
                bag.SetAttributeValue("inverse", "true");

                var key = new XElement(_xmlns + "key");
                key.SetAttributeValue("column", SqlSafe(fkColumn));
                key.SetAttributeValue("foreign-key", fk.Name);
                bag.Add(key);

                var one2Many = new XElement(_xmlns + "one-to-many");
                one2Many.SetAttributeValue("class", childClass);
                bag.Add(one2Many);

                _classElement.Add(bag);
            }
        }

        private void WriteColumns()
        {
            var columns = _table.Columns.Where(column => !column.IsPrimaryKey);
            columns = WriteNaturalKey(columns);
            foreach (var column in columns)
            {
                WriteColumn(column);
            }
        }

        private IEnumerable<DatabaseColumn> WriteNaturalKey(IEnumerable<DatabaseColumn> columns)
        {
            var uk = _table.UniqueKeys.FirstOrDefault();
            if (uk == null) return columns;

            //we only deal with one natural key
            var ukcols = uk.Columns.Select(x => _table.FindColumn(x)).ToList();
            //in databases unique keys can be nullable, but not in NHibernate
            //for now we don't want to deal with foreign keys either
            if (!ukcols.Any(x => x.IsForeignKey && x.Nullable))
            {
                var naturalKey = new XElement(_xmlns + "natural-id", new XAttribute("mutable", "true"));
                _classElement.Add(naturalKey);

                var list = columns.ToList();
                foreach (var ukcol in ukcols)
                {
                    var property = CreateColumn(ukcol);
                    //add columns to natural key
                    naturalKey.Add(property);
                    //remove uk columns from list
                    list.RemoveAll(c => c.Name == ukcol.Name);
                }
                columns = list;
            }

            return columns;
        }

        private void WriteColumn(DatabaseColumn column)
        {
            if (column.IsForeignKey)
            {
                var many2One = CreateForeignKey(column);
                _classElement.Add(many2One);
                return;
            }

            var property = CreateColumn(column);

            _classElement.Add(property);
        }

        private XElement CreateColumn(DatabaseColumn column)
        {
            var propertyName = column.NetName;

            var property = new XElement(_xmlns + "property",
                                        new XAttribute("name", propertyName));
            if (propertyName != column.Name)
            {
                property.SetAttributeValue("column", SqlSafe(column.Name));
            }

            if (column.IsComputed)
            {
                property.SetAttributeValue("generated", "always");
            }
            else
            {
                var dt = column.DataType;
                if (dt != null)
                {
                    var dataType = dt.NetCodeName(column);
                    property.SetAttributeValue("type", dataType);
                    //nvarchar(max) may be -1
                    if (dt.IsString && column.Length > 0)
                    {
                        property.SetAttributeValue("length", column.Length.ToString());
                    }
                }

                if (!column.Nullable)
                {
                    property.SetAttributeValue("not-null", "true");
                }
            }
            return property;
        }

        private XElement CreateForeignKey(DatabaseColumn column)
        {
            var propertyName = column.NetName;
            var dataType = column.ForeignKeyTable.NetName;
            var property = new XElement(_xmlns + "many-to-one",
               new XAttribute("name", propertyName));
            if (propertyName != column.Name)
            {
                property.SetAttributeValue("column", SqlSafe(column.Name));
            }
            property.SetAttributeValue("class", dataType);
            //bad idea unless you expect the database to be inconsistent
            //property.SetAttributeValue("not-found", "ignore");
            return property;
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
                _classElement.Add(new XComment("TODO- you MUST add a primary key!"));
                return;
            }
            if (_table.HasCompositeKey)
            {
                _classElement.Add(new XComment("TODO- composite keys are a BAD IDEA"));
                AddCompositePrimaryKey();
                return;
            }

            var idColumn = _table.PrimaryKeyColumn;
            var dataType = idColumn.DataType;
            var id = new XElement(_xmlns + "id");
            id.SetAttributeValue("name", idColumn.NetName);
            if (idColumn.Name != idColumn.NetName)
            {
                id.SetAttributeValue("column", SqlSafe(idColumn.Name));
            }

            id.SetAttributeValue("type", dataType.NetCodeName(idColumn));
            if (dataType.IsString) id.SetAttributeValue("length", idColumn.Length.ToString());
            else if (dataType.IsNumeric) id.SetAttributeValue("unsaved-value", "0");
            ////using most common "_myId" format
            //if (idColumn.IsAutoNumber)
            //{
            //    id.SetAttributeValue("access", "nosetter.camelcase-underscore");
            //}

            var gen = new XElement(_xmlns + "generator");
            if (idColumn.IsAutoNumber)
            {
                gen.SetAttributeValue("class", "native");
            }
            else if (dataType.IsString)
            {
                gen.SetAttributeValue("class", "assigned");
            }
            else if (dataType.GetNetType() == typeof(Guid))
            {
                gen.SetAttributeValue("class", "guid.comb");
            }
            else
            {
                //otherwise decide your identity strategy
                gen.SetAttributeValue("class", "native");
            }

            id.Add(gen);

            _classElement.Add(id);

            if (idColumn.IsForeignKey)
            {
                var fk = _table.ForeignKeys.FirstOrDefault(x => x.RefersToTable == idColumn.ForeignKeyTableName);
                if (fk == null) return;
                var propertyName = _codeWriterSettings.Namer.ForeignKeyName(_table, fk);
                gen.SetAttributeValue("class", "foreign");
                gen.Add(new XElement(_xmlns + "param",
                    new XAttribute("name", "property"),
                    propertyName));
                var one2One = new XElement(_xmlns + "one-to-one");
                one2One.SetAttributeValue("name", propertyName);
                _classElement.Add(one2One);
            }
        }

        private void AddCompositePrimaryKeyForView()
        {
            var id = new XElement(_xmlns + "composite-id");
            foreach (var column in _table.Columns)
            {
                var key = new XElement(_xmlns + "key-property");
                key.SetAttributeValue("column", SqlSafe(column.Name));
                key.SetAttributeValue("name", column.NetName);
                id.Add(key);
            }
            _classElement.Add(id);
        }

        private void AddCompositePrimaryKey()
        {
            var id = new XElement(_xmlns + "composite-id");
            id.SetAttributeValue("name", "Key");
            id.SetAttributeValue("class", _table.NetName + "Key");

            foreach (string colName in _table.PrimaryKey.Columns)
            {
                //two possibilities:
                //* <key-property name="OrderId" column="Order_Id" />
                //* <key-many-to-one name="Order" column="Order_Id" />
                var keyType = "key-property"; //"key-many-to-one";
                var column = _table.FindColumn(colName);
                if (column == null) continue; //corruption in our model

                //if (column.ForeignKeyTable == null)
                //{
                //    //the database may be missing a fk reference
                //    keyType = "key-property";
                //}
                var propertyName = _codeWriterSettings.Namer.PrimaryKeyName(column);

                var key = new XElement(_xmlns + keyType);
                key.SetAttributeValue("column", SqlSafe(colName));
                key.SetAttributeValue("name", propertyName);
                //if (column.ForeignKeyTable != null)
                //    key.SetAttributeValue("class", column.ForeignKeyTable.NetName);
                id.Add(key);
            }

            _classElement.Add(id);
        }
    }
}
