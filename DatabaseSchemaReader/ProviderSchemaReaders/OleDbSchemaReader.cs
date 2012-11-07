using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Linq;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders
{
    class OleDbSchemaReader : SchemaExtendedReader
    {
        public OleDbSchemaReader(string connectionString, string providerName)
            : base(connectionString, providerName)
        {
        }

        private static DataTable GetOleDbSchemaTable(Guid schema, string tableName, DbConnection connection, string collectionName)
        {
            //this is the old .Net 1.1 provider specific way of doing it
            var oleDbConnection = connection as OleDbConnection;
            if (oleDbConnection == null) return CreateDataTable(collectionName);

            try
            {
                var dt = oleDbConnection.GetOleDbSchemaTable(schema, new object[] { null, null, tableName });
                dt.TableName = collectionName;
                return dt;
            }
            catch (ArgumentException)
            {
                //may not be implemented
                return CreateDataTable(collectionName);
            }
        }

        protected override DataTable PrimaryKeys(string tableName, DbConnection connection)
        {
            var schemaTable = GetOleDbSchemaTable(OleDbSchemaGuid.Primary_Keys, tableName, connection, PrimaryKeysCollectionName);
            return schemaTable;
        }

        protected override DataTable ForeignKeys(string tableName, DbConnection connection)
        {
            var schemaTable = GetOleDbSchemaTable(OleDbSchemaGuid.Foreign_Keys, tableName, connection, ForeignKeysCollectionName);
            return schemaTable;
        }

        protected override DataTable CheckConstraints(string tableName, DbConnection connection)
        {
            var schemaTable = GetOleDbSchemaTable(OleDbSchemaGuid.Check_Constraints, null, connection, CheckConstraintsCollectionName);

            //let's try to read the table name from the constraint name
            const string tableColumnKey = "TABLE_NAME";
            schemaTable.Columns.Add(tableColumnKey, typeof(string));
            foreach (DataRow row in schemaTable.Rows)
            {
                var name = row["CONSTRAINT_NAME"].ToString();
                if (string.IsNullOrEmpty(name)) continue;
                var brace = name.IndexOf("[", StringComparison.Ordinal);
                var endBrace = name.IndexOf("].", StringComparison.Ordinal);
                if (brace == -1 || endBrace == -1) continue;
                var table = name.Substring(brace + 1, endBrace - brace - 1);
                row[tableColumnKey] = table;

            }
            return schemaTable;
        }

        protected override DataTable Indexes(string tableName, DbConnection connection)
        {
            var schemaTable = GetOleDbSchemaTable(OleDbSchemaGuid.Indexes, null, connection, IndexesCollectionName);
            return schemaTable;
        }

        protected override DataTable StoredProcedureArguments(string storedProcedureName, DbConnection connection) 
        {
            //for JET and ACE providers this isn't supported, but we'll try anyway
            var schemaTable = GetOleDbSchemaTable(OleDbSchemaGuid.Procedure_Parameters, storedProcedureName, connection, ProcedureParametersCollectionName);
            return schemaTable;
        }

        protected override DataTable DataTypes(DbConnection connection)
        {
            var dataTypes = base.DataTypes(connection);
            //add some types which Jet doesn't return
            if (!dataTypes.AsEnumerable().Any(row => row.Field<int>("ProviderDbType") == 130))
            {
                var row = dataTypes.NewRow();
                row.SetField("ProviderDbType", 130);
                row.SetField("TypeName", "TEXT"); //TEXT or MEMO
                row.SetField("DataType", typeof(string).FullName);
                dataTypes.Rows.Add(row);
            }
            if (!dataTypes.AsEnumerable().Any(row => row.Field<int>("ProviderDbType") == 128))
            {
                var row = dataTypes.NewRow();
                row.SetField("ProviderDbType", 128);
                row.SetField("TypeName", "Binary"); //OLE Object
                row.SetField("DataType", typeof(Byte[]).FullName);
                dataTypes.Rows.Add(row);
            }
            return dataTypes;
        }

        public override void PostProcessing(DatabaseSchema databaseSchema)
        {
            //check if no datatypes loaded
            if (databaseSchema.DataTypes.Count == 0) return;

            foreach (DatabaseTable table in databaseSchema.Tables)
            {
                UpdateColumnDataTypes(table.Columns);
            }
            foreach (DatabaseView view in databaseSchema.Views)
            {
                UpdateColumnDataTypes(view.Columns);
            }

            //ignore temporary internal queries
            databaseSchema.StoredProcedures
                .RemoveAll(procedure =>
                          procedure.Name.StartsWith("~sq_", StringComparison.OrdinalIgnoreCase));


        }

        private static void UpdateColumnDataTypes(IEnumerable<DatabaseColumn> columns)
        {
            foreach (DatabaseColumn column in columns)
            {
                if (column.DataType != null)
                {
                    //change the type name from "130" to "VARCHAR" etc
                    column.DbDataType = column.DataType.TypeName;
                }
            }
        }
    }
}