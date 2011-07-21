using System.Data;

namespace DatabaseSchemaReader.Conversion.KeyMaps
{
    internal class ArgumentsKeyMap
    {
        public ArgumentsKeyMap(DataTable arguments)
        {
            //oracle
            OwnerKey = "SPECIFIC_SCHEMA";
            SprocName = "SPECIFIC_NAME";
            ParameterName = "PARAMETER_NAME";
            InoutKey = "PARAMETER_MODE";
            DatatypeKey = "DATA_TYPE";
            PackageKey = "PACKAGE_NAME";
            OrdinalKey = "ORDINAL_POSITION";
            LengthKey = "CHARACTER_MAXIMUM_LENGTH";
            PrecisionKey = "NUMERIC_PRECISION";
            ScaleKey = "NUMERIC_SCALE";

            //sql server
            if (!arguments.Columns.Contains(SprocName)) SprocName = "OBJECT_NAME";
            if (!arguments.Columns.Contains(OwnerKey)) OwnerKey = "OWNER";
            if (!arguments.Columns.Contains(ParameterName)) ParameterName = "ARGUMENT_NAME";
            if (!arguments.Columns.Contains(InoutKey)) InoutKey = "IN_OUT";
            if (!arguments.Columns.Contains(PackageKey)) PackageKey = null;
            if (!arguments.Columns.Contains(OrdinalKey)) OrdinalKey = "POSITION";
            if (!arguments.Columns.Contains(LengthKey)) LengthKey = "DATA_LENGTH";
            if (!arguments.Columns.Contains(PrecisionKey)) PrecisionKey = "DATA_PRECISION";
            if (!arguments.Columns.Contains(ScaleKey)) ScaleKey = "DATA_SCALE";

            //Devart.Data.Oracle
            CheckDevart(arguments);

            //oledb and firebird and db2
            if (!arguments.Columns.Contains(SprocName)) SprocName = "PROCEDURE_NAME";
            if (!arguments.Columns.Contains(OwnerKey)) OwnerKey = "PROCEDURE_SCHEMA";
            //firebird
            CheckFirebird(arguments);
            //db2
            CheckDb2(arguments);

            //Intersystems.Cache
            if (!arguments.Columns.Contains(OwnerKey)) OwnerKey = "PROCEDURE_OWNER";
            if (!arguments.Columns.Contains(OrdinalKey)) OrdinalKey = null;
            if (arguments.Columns.Contains("TYPE_NAME")) DatatypeKey = "TYPE_NAME";

            //not provided
            if (!arguments.Columns.Contains(PrecisionKey)) PrecisionKey = null;
            if (!arguments.Columns.Contains(LengthKey)) LengthKey = null;
            if (!arguments.Columns.Contains(ScaleKey)) ScaleKey = null;
            if (!arguments.Columns.Contains(InoutKey)) InoutKey = null;
        }

        private void CheckFirebird(DataTable arguments)
        {
            if (!arguments.Columns.Contains(DatatypeKey)) DatatypeKey = "PARAMETER_DATA_TYPE";
            if (!arguments.Columns.Contains(InoutKey)) InoutKey = "PARAMETER_DIRECTION";
            if (!arguments.Columns.Contains(LengthKey)) LengthKey = "CHARACTER_MAX_LENGTH";
            if (!arguments.Columns.Contains(PrecisionKey)) PrecisionKey = "NUMERIC_PRECISION";
            if (!arguments.Columns.Contains(ScaleKey)) ScaleKey = "NUMERIC_SCALE";
        }

        private void CheckDb2(DataTable arguments)
        {
            IsDb2 = arguments.Columns.Contains("PROCEDURE_MODULE");
            if (!arguments.Columns.Contains(ParameterName)) ParameterName = "COLUMN_NAME";
            if (arguments.Columns.Contains("PROVIDER_TYPE_NAME")) DatatypeKey = "PROVIDER_TYPE_NAME";
            Db2ColumnTypeKey = null;
            if (arguments.Columns.Contains("COLUMN_TYPE_NAME")) Db2ColumnTypeKey = "COLUMN_TYPE_NAME";
            if (!arguments.Columns.Contains(LengthKey)) LengthKey = "COLUMN_SIZE";
            if (!arguments.Columns.Contains(PrecisionKey)) PrecisionKey = "COLUMN_SIZE";
            if (!arguments.Columns.Contains(ScaleKey)) ScaleKey = "DECIMAL_DIGITS";
        }

        private void CheckDevart(DataTable arguments)
        {
            if (!arguments.Columns.Contains(ParameterName)) ParameterName = "NAME";
            if (!arguments.Columns.Contains(SprocName)) SprocName = "PROCEDURE";
            if (!arguments.Columns.Contains(OwnerKey)) OwnerKey = "SCHEMA";
            if (PackageKey == null && arguments.Columns.Contains("PACKAGE")) PackageKey = "PACKAGE";
            if (!arguments.Columns.Contains(DatatypeKey)) DatatypeKey = "DATATYPE";
            if (!arguments.Columns.Contains(PrecisionKey)) PrecisionKey = "PRECISION";
            if (!arguments.Columns.Contains(LengthKey)) LengthKey = "LENGTH";
            if (!arguments.Columns.Contains(ScaleKey)) ScaleKey = "SCALE";
            if (!arguments.Columns.Contains(InoutKey)) InoutKey = "DIRECTION";
        }

        public bool IsDb2 { get; private set; }

        public string InoutKey { get; private set; }

        public string DatatypeKey { get; private set; }

        public string OrdinalKey { get; private set; }

        public string Db2ColumnTypeKey { get; private set; }

        public string ScaleKey { get; private set; }

        public string PrecisionKey { get; private set; }

        public string LengthKey { get; private set; }

        public string PackageKey { get; private set; }

        public string ParameterName { get; private set; }

        public string OwnerKey { get; private set; }

        public string SprocName { get; private set; }
    }
}