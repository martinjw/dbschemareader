using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Utilities
{

    /// <summary>
    /// Convert an EF storage model into a DSR schema model.
    /// </summary>
    /// <remarks>You can inherit from this and override <see cref="FixColumn"/></remarks>
    public class EntityFrameworkImporter
    {
        //the EF writes DDL using a t4 template
        // %ProgramFiles%\Microsoft Visual Studio 10.0\Common7\IDE\Extensions\Microsoft\Entity Framework Tools\DBGen\SSDLToSQL10.tt
        //there's an include with some of the logic at
        // %ProgramFiles%\Microsoft Visual Studio 10.0\Common7\IDE\Extensions\Microsoft\Entity Framework Tools\Templates\Includes\GenerateTSQL.Utility.ttinclude
        //You can customize it by copying it and including it in your project
        //On the EDMX designer, right click for properties and look under Database Script Generation for "DDL Generation Template"

        //The VS template actually seems to use objects from the conceptional model, not the storage model
        //Getting the store model out of context.MetaDataWorkspace is kinda tricky
        //Here we use the store model xml directly

        //EF has a Code First Migrations pack in development (at time of writing)
        //http://blogs.msdn.com/b/adonet/archive/tags/entity+framework/
        //Also see the EF Database Generation Power Pack http://visualstudiogallery.msdn.microsoft.com/df3541c3-d833-4b65-b942-989e7ec74c87
        //This works quite well for migrations, but it's end-of-life now.

        private XNamespace _schema;
        private readonly XNamespace _store = "http://schemas.microsoft.com/ado/2007/12/edm/EntityStoreSchemaGenerator";
        private XNamespace _edmx;

        /// <summary>
        /// Reads the EDMX xml.
        /// </summary>
        /// <param name="edmxFilePath">The edmx file path.</param>
        /// <returns>A <see cref="DatabaseSchema"/></returns>
        public DatabaseSchema ReadEdmx(string edmxFilePath)
        {
            if (string.IsNullOrEmpty(edmxFilePath))
                throw new ArgumentNullException("edmxFilePath", @"No ssdl/edmx path");
            if (!File.Exists(edmxFilePath))
                throw new ArgumentException(@"File does not exist", "edmxFilePath");

            var doc = XDocument.Load(edmxFilePath);
            return ReadEdmx(doc);
        }

        /// <summary>
        /// Reads the edmx from an xml document
        /// </summary>
        /// <param name="edmx">The edmx.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">No edmx document</exception>
        public DatabaseSchema ReadEdmx(XDocument edmx)
        {
            if (edmx == null)
                throw new ArgumentNullException("edmx", "No edmx document");

            var root = edmx.Root;
            if (root == null)
                throw new ArgumentException("No root element found", "edmx");
            _edmx = root.GetNamespaceOfPrefix("edmx");

            var storageModel =
                root.Element(_edmx + "Runtime").Element(_edmx + "StorageModels");
            var storage = storageModel.Descendants().First(x => x.Name.LocalName == "Schema");
            _schema = storage.GetDefaultNamespace();
            return ReadEntityFramework(storage);
        }

        /// <summary>
        /// Reads the SSDL xml.
        /// </summary>
        /// <param name="ssdlFilePath">The SSDL file path.</param>
        /// <returns>A <see cref="DatabaseSchema"/></returns>
        public DatabaseSchema ReadSsdl(string ssdlFilePath)
        {
            if (string.IsNullOrEmpty(ssdlFilePath))
                throw new ArgumentNullException("ssdlFilePath", @"No ssdl/edmx path");
            if (!File.Exists(ssdlFilePath))
                throw new ArgumentException(@"File does not exist", "ssdlFilePath");


            var doc = XDocument.Load(ssdlFilePath);
            _edmx = doc.Root.GetNamespaceOfPrefix("edmx");
            return ReadSsdl(doc);
        }

        /// <summary>
        /// Reads the SSDL.
        /// </summary>
        /// <param name="ssdlDocument">The SSDL document.</param>
        /// <returns>A <see cref="DatabaseSchema"/></returns>
        public DatabaseSchema ReadSsdl(XDocument ssdlDocument)
        {

            var storage = ssdlDocument.Root;
            _edmx = storage.GetNamespaceOfPrefix("edmx");
            if (storage.Name.LocalName != "Schema")
                throw new InvalidOperationException("SSDL file does not have expected structure");
            _schema = storage.GetDefaultNamespace();

            return ReadEntityFramework(storage);
        }

        private DatabaseSchema ReadEntityFramework(XElement storageSchema)
        {
            var databaseSchema = new DatabaseSchema(null, SqlType.SqlServer);

            var entityContainer = storageSchema.Element(_schema + "EntityContainer");
            foreach (var entitySet in entityContainer.Elements(_schema + "EntitySet"))
            {
                var name = (string)entitySet.Attribute("Name");
                var schema = (string)entitySet.Attribute("Schema");
                var storeName = (string)entitySet.Attribute(_store + "Name");
                var storeSchema = (string)entitySet.Attribute(_store + "Schema");
                var type = (string)entitySet.Attribute(_store + "Type");

                DatabaseTable table;
                if (type.Equals("Tables", StringComparison.OrdinalIgnoreCase))
                {
                    table = databaseSchema.AddTable(name);
                    table.SchemaOwner = schema;
                }
                else if (type.Equals("Views", StringComparison.OrdinalIgnoreCase))
                {
                    var view = new DatabaseView { Name = storeName, SchemaOwner = storeSchema };
                    databaseSchema.Views.Add(view);
                    table = view;
                }
                else
                {
                    //some other type eg something with a DefiningQuery
                    continue;
                }

                AddProperties(storageSchema, table);
            }


            AddForeignKeys(storageSchema, databaseSchema);

            return databaseSchema;
        }

        private void AddForeignKeys(XElement storageSchema, DatabaseSchema databaseSchema)
        {
            foreach (var association in storageSchema.Elements(_schema + "Association"))
            {
                var name = (string)association.Attribute("Name");
                var referentialConstraint = association.Element(_schema + "ReferentialConstraint");
                //referentialConstraint is optional
                if (referentialConstraint == null) continue;
                var principal = referentialConstraint.Element(_schema + "Principal");
                var fkRole = (string)principal.Attribute("Role");
                var fkTable = AssociationTypeToTableName(storageSchema, association, fkRole);
                var dependent = referentialConstraint.Element(_schema + "Dependent");
                var role = (string)dependent.Attribute("Role");
                var tableName = AssociationTypeToTableName(storageSchema, association, role);

                //the EF DDL generator (GenerateTSQL.Utility.ttinclude WriteFKConstraintName) adds the prefix
                if (!name.StartsWith("FK_", StringComparison.OrdinalIgnoreCase))
                    name = "FK_" + name;
                var fk = new DatabaseConstraint
                             {
                                 ConstraintType = ConstraintType.ForeignKey,
                                 Name = name,
                                 TableName = tableName,
                                 RefersToTable = fkTable
                             };
                fk.Columns.AddRange(
                    dependent.Elements(_schema + "PropertyRef")
                        .Select(e => (string)e.Attribute("Name")));
                var deleteRule =
                    association.Elements(_schema + "End")
                    .First(e => (string)e.Attribute("Role") == fkRole)
                    .Element(_schema + "OnDelete");
                if (deleteRule != null && (string)deleteRule.Attribute("Action") == "Cascade")
                {
                    fk.DeleteRule = "Cascade";
                }
                var databaseTable = databaseSchema.FindTableByName(tableName);
                databaseTable.AddConstraint(fk);
                var cols = fk.Columns.Select(col => databaseTable.FindColumn(col));

                //SSDLToSQL10.tt creates a nonclustered index (IX_name) for the foreign key
                databaseTable.AddIndex("IX_" + name, cols);
            }
        }

        private string AssociationTypeToTableName(XContainer storageSchema, XContainer association, string role)
        {
            //from <Principal Role="Category">
            //to <End Role="Category" Type="Catalog.Store.Categories" Multiplicity="1">
            //to <EntitySet Name="Categories" EntityType="Catalog.Store.Categories" store:Type="Tables" Schema="dbo" />
            var end = association.Elements(_schema + "End")
                .First(e => (string)e.Attribute("Role") == role);
            var type = (string)end.Attribute("Type");
            var entitySets = storageSchema
                .Element(_schema + "EntityContainer")
                .Elements(_schema + "EntitySet");
            return entitySets
                .First(es => (string)es.Attribute("EntityType") == type)
                .Attribute("Name").Value;
        }

        private void AddProperties(XContainer storageSchema, DatabaseTable table)
        {
            var tableName = table.Name;
            //get the related entity type
            var entityType =
                storageSchema.Elements(_schema + "EntityType")
                    .Where(et => (string)et.Attribute("Name") == tableName)
                    .FirstOrDefault();
            if (entityType == null) return;

            foreach (var property in entityType.Elements(_schema + "Property"))
            {
                ReadProperty(property, table);
            }

            AddPrimaryKey(table, entityType);
        }

        private void AddPrimaryKey(DatabaseTable table, XElement entityType)
        {
            var key = entityType.Element(_schema + "Key");
            if (key == null) return;
            var pkColumns =
                key.Elements(_schema + "PropertyRef").Select(propertyRef => (string)propertyRef.Attribute("Name"));
            var primaryKey = new DatabaseConstraint { ConstraintType = ConstraintType.PrimaryKey };
            primaryKey.Columns.AddRange(pkColumns);
            table.PrimaryKey = primaryKey;
        }

        private void ReadProperty(XElement property, DatabaseTable table)
        {
            var name = (string)property.Attribute("Name");
            var type = (string)property.Attribute("Type");

            var nullable = (bool?)property.Attribute("Nullable") ?? true;
            var maxLength = (int?)property.Attribute("MaxLength");
            var precision = (int?)property.Attribute("Precision");
            var scale = (int?)property.Attribute("Scale");
            var defaultValue = (string)property.Attribute("DefaultValue");
            var storeGeneratedPattern = (string)property.Attribute("StoreGeneratedPattern");

            if (type.EndsWith("(max)", StringComparison.OrdinalIgnoreCase))
            {
                type = type.Substring(0, type.Length - 5);
                maxLength = -1;
            }
            //timestamp is a varbinary(8) computed
            if (type == "binary" && maxLength == 8 && (string)property.Attribute("StoreGeneratedPattern") == "Computed")
            {
                type = "timestamp";
                maxLength = null;
            }

            var column = table.AddColumn(name, type);
            column.DefaultValue = defaultValue;
            column.Length = maxLength;
            column.Precision = precision;
            column.Scale = scale;
            column.Nullable = nullable;

            if (storeGeneratedPattern == "Identity") column.IsIdentity = true;

            FixColumn(column);

        }

        /// <summary>
        /// Correct the column datatype and related properties
        /// </summary>
        /// <param name="column">The column.</param>
        /// <remarks>Extension point: inherit this class and override this</remarks>
        protected virtual void FixColumn(DatabaseColumn column)
        {
            //the EF ssdl doesn't specify some precision/scale/length settings for certain types
            //if you're comparing this to an actual schema you'll need these values or you get false positive "ALTER COLUMN"s
            //we assuming EF's SqlServer here, but other databases also have EF providers which need customization

            switch (column.DbDataType.ToLowerInvariant())
            {
                case "int":
                    column.Precision = 10;
                    column.Scale = 0;
                    break;
                case "smallint":
                    column.Precision = 5;
                    column.Scale = 0;
                    break;
                case "money":
                    column.Precision = 19;
                    column.Scale = 4;
                    break;
                case "real":
                    column.Precision = 24;
                    break;
                case "float":
                    column.Precision = 53;
                    break;
                case "ntext":
                    column.Length = 1073741823;
                    break;
                case "image":
                    column.Length = int.MaxValue;
                    break;

            }
        }
    }
}
