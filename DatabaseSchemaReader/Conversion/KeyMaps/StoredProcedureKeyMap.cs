using System.Data;

namespace DatabaseSchemaReader.Conversion.KeyMaps
{
    internal class StoredProcedureKeyMap
    {
        public StoredProcedureKeyMap(DataTable dt)
        {
            //sql server
             Key = "ROUTINE_NAME";
             OwnerKey = "ROUTINE_SCHEMA";
             RoutineTypeKey = "ROUTINE_TYPE";
            if (!dt.Columns.Contains(RoutineTypeKey)) RoutineTypeKey = "PROCEDURE_TYPE_NAME";
            if (!dt.Columns.Contains(RoutineTypeKey)) RoutineTypeKey = null;
            //oracle
            if (!dt.Columns.Contains(Key)) Key = "OBJECT_NAME";
            if (!dt.Columns.Contains(OwnerKey)) OwnerKey = "OWNER";
             PackageKey = "PACKAGE_NAME";
            if (!dt.Columns.Contains(PackageKey)) PackageKey = null; //sql
            //jet (and firebird)
            if (!dt.Columns.Contains(Key)) Key = "PROCEDURE_NAME";
            if (!dt.Columns.Contains(OwnerKey)) OwnerKey = "PROCEDURE_SCHEMA";
             Sql = "PROCEDURE_DEFINITION";
            if (!dt.Columns.Contains(Sql)) Sql = "ROUTINE_DEFINITION"; //MySql
            if (!dt.Columns.Contains(Sql)) Sql = "SOURCE"; //firebird
            if (!dt.Columns.Contains(Sql)) Sql = null;
            //Devart.Data.Oracle
            if (!dt.Columns.Contains(Key)) Key = "NAME";
            if (!dt.Columns.Contains(OwnerKey)) OwnerKey = "SCHEMA";
            if (PackageKey == null && dt.Columns.Contains("PACKAGE")) PackageKey = "PACKAGE";
            if (!dt.Columns.Contains(OwnerKey)) OwnerKey = "DATABASE";
            //Intersystems Cache
            if (!dt.Columns.Contains(OwnerKey)) OwnerKey = "PROCEDURE_SCHEM";
            IsDb2 = dt.Columns.Contains("PROCEDURE_MODULE");
            RoutineTypeKey = RoutineTypeKey;
            PackageKey = PackageKey;
            Sql = Sql;
            Key = Key;
            IsDb2 = IsDb2;
            OwnerKey = OwnerKey;
        }

        public string RoutineTypeKey { get; private set; }

        public string PackageKey { get; private set; }

        public string Sql { get; private set; }

        public string Key { get; private set; }

        public bool IsDb2 { get; private set; }

        public string OwnerKey { get; private set; }
    }
}