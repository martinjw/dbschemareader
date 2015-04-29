using System.Data;

namespace DatabaseSchemaReader.Conversion.KeyMaps
{
    internal class FunctionKeyMap
    {
        public FunctionKeyMap(DataTable dt)
        {
            //oracle
            Key = "OBJECT_NAME";
            OwnerKey = "OWNER";
            SqlKey = "SQL";
            LangKey = "LANGUAGE";
            ReturnKey = "RETURNTYPE";
            //devart
            if (!dt.Columns.Contains(Key)) Key = "NAME";
            if (!dt.Columns.Contains(OwnerKey)) OwnerKey = "SCHEMA";
            if (!dt.Columns.Contains(SqlKey)) SqlKey = "BODY";
            //firebird
            if (!dt.Columns.Contains(Key)) Key = "FUNCTION_NAME";
            if (!dt.Columns.Contains(OwnerKey)) OwnerKey = "FUNCTION_SCHEMA";
            if (!dt.Columns.Contains(ReturnKey)) ReturnKey = "RETURN_ARGUMENT";
            //other
            if (!dt.Columns.Contains(OwnerKey)) OwnerKey = null;
            if (!dt.Columns.Contains(SqlKey)) SqlKey = null;
            if (!dt.Columns.Contains(LangKey)) LangKey = null;
            if (!dt.Columns.Contains(ReturnKey)) ReturnKey = null;
            LangKey = LangKey;
            SqlKey = SqlKey;
            ReturnKey = ReturnKey;
            OwnerKey = OwnerKey;
            Key = Key;
        }

        public string LangKey { get; private set; }

        public string SqlKey { get; private set; }

        public string ReturnKey { get; private set; }

        public string OwnerKey { get; private set; }

        public string Key { get; private set; }
    }
}