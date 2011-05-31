using System;
using System.Data;
using System.Data.Common;
using System.Globalization;

namespace DatabaseSchemaReader
{
    class SchemaRestrictions : IDisposable
    {
        private DataTable _restrictions;
        private readonly string _owner;

        public SchemaRestrictions(string owner)
        {
            _owner = owner;
        }

        /// <summary>
        /// Get the schema/ owner restriction. This is the simplest.
        /// </summary>
        public string[] ForOwner(DbConnection connection,
            string collectionName)
        {
            return GetSchemaRestrictions(connection, collectionName, null);
        }

        /// <summary>
        /// Get the schema/ owner restriction with schema/ owner and tableName. There are multiple aliases for table name.
        /// </summary>
        public string[] ForTable(DbConnection connection,
            string collectionName,
            string tableName)
        {
            return GetSchemaRestrictions(connection, collectionName, tableName, "TABLE", "TABLE_NAME", "TABLENAME");
        }

        public string[] ForRoutine(DbConnection connection,
            string collectionName,
            string routineName)
        {
            return GetSchemaRestrictions(connection, collectionName, routineName, "SPECIFIC_NAME", "OBJECT_NAME");
        }

        public string[] ForSpecific(DbConnection connection,
            string collectionName,
            string value,
            string restrictionName)
        {
            return GetSchemaRestrictions(connection, collectionName, value, restrictionName);
        }

        /// <summary>
        /// Gets the schema restrictions. There are different restrictions for each dataprovider :(
        /// </summary>
        private string[] GetSchemaRestrictions(DbConnection connection, string restrictionType, string value, params string[] restrictionName)
        {
            //there are no restrictions
            if (string.IsNullOrEmpty(_owner) && string.IsNullOrEmpty(value))
                return null;
            //get the restrictions collection
            DataView dv = GetRestrictions(connection, restrictionType);
            bool hasParameterName = dv.Table.Columns.Contains("ParameterName");
            string paramName = string.Empty;

            string[] restrictions = new string[dv.Count];
            bool usedRestriction = false;
            //loop through the collection looking for the restriction
            for (int i = 0; i < dv.Count; i++)
            {
                string name = (string)dv[i].Row["RestrictionName"];
                //Oracle has an alternative column name
                if (hasParameterName)
                    paramName = (string)dv[i].Row["ParameterName"];
                bool found = false;
                //if set for owner restriction, apply it here
                if (!string.IsNullOrEmpty(_owner))
                {
                    if (name.Equals("OWNER", StringComparison.OrdinalIgnoreCase) ||
                        paramName.Equals("OWNER", StringComparison.OrdinalIgnoreCase) ||
                        name.Equals("TABLE_SCHEMA", StringComparison.OrdinalIgnoreCase) ||
                        name.Equals("PROCEDURE_SCHEMA", StringComparison.OrdinalIgnoreCase) ||
                        //Devart MySql
                        name.Equals("DATABASENAME", StringComparison.OrdinalIgnoreCase) ||
                        //Postgresql uses "Schema"
                        name.Equals("Schema", StringComparison.OrdinalIgnoreCase))
                    {
                        restrictions[i] = _owner;
                        usedRestriction = true;
                        continue;
                    }

                }
                //other restrictions: different possible names
                foreach (string rname in restrictionName)
                {
                    if (name.Equals(rname, StringComparison.OrdinalIgnoreCase))
                    {
                        found = true;
                        restrictions[i] = value;
                        usedRestriction = true;
                        break;
                    }
                }
                if (!found) restrictions[i] = null;
            }
            if (!usedRestriction) restrictions = null;
            return restrictions;
        }

        /// <summary>
        /// Gets all the restrictions. Caches it.
        /// </summary>
        private DataView GetRestrictions(DbConnection connection, string restrictionType)
        {
            //get table of restrictions
            if (_restrictions == null)
                LoadRestrictions(connection);

            //get the dataview (the defaultview from the datatable)
            DataView dv = _restrictions.DefaultView;
            dv.RowFilter = "CollectionName = '" + restrictionType + "'";
            dv.Sort = "RestrictionNumber";
            return dv;
        }

        private void LoadRestrictions(DbConnection connection)
        {
            try
            {
                _restrictions = connection.GetSchema(DbMetaDataCollectionNames.Restrictions);
            }
            catch (NotSupportedException)
            {
                //SqlLite provider doesn't support this
                //recreate it- the first is always 
                var collections = connection.GetSchema(DbMetaDataCollectionNames.MetaDataCollections);
                _restrictions = new DataTable("Restrictions");
                _restrictions.Locale = CultureInfo.InvariantCulture;
                _restrictions.Columns.Add("CollectionName", typeof(string));
                _restrictions.Columns.Add("RestrictionNumber", typeof(int));
                _restrictions.Columns.Add("RestrictionName", typeof(string));

                foreach (DataRow row in collections.Rows)
                {
                    //every collections has catalog/ owner/ table restrictions
                    _restrictions.Rows.Add(row["CollectionName"].ToString(), 0, "OWNER");
                    _restrictions.Rows.Add(row["CollectionName"].ToString(), 1, "NA");
                    _restrictions.Rows.Add(row["CollectionName"].ToString(), 2, "TABLE");
                }
            }
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if(_restrictions != null)
            {
                _restrictions.Dispose();
                _restrictions = null;
            }
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
