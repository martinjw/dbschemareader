using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.SqlServerCe
{

    /// <summary>
    /// SqlServer CE is a subset of SqlServer, so we derive from it and downgrade certain types
    /// </summary>
    class DataTypeWriter : SqlServer.DataTypeWriter
    {
        public override string WriteDataType(DatabaseColumn column)
        {
            var dataType = base.WriteDataType(column);
            if (dataType == "TEXT")
            {
                dataType = "NTEXT";
            }
            return dataType;
        }

        protected override string ConvertOtherPlatformTypes(string dataType, int providerType, int? length, int? precision, int? scale)
        {
            if (dataType == "BINARY")
            {
                //should not be varbinary
                return WriteDataTypeWithLength(dataType, length);
            }
            dataType = base.ConvertOtherPlatformTypes(dataType, providerType, length, precision, scale);

            //do the subset of SqlServer features of CE
            //MSDN list = http://msdn.microsoft.com/en-us/library/ms172424%28v=SQL.110%29.aspx

            switch (dataType)
            {
                case "DECIMAL":
                    dataType = "NUMERIC";
                    break;
                case "VARCHAR":
                    dataType = "NVARCHAR";
                    break;
                case "CHAR":
                    dataType = "NCHAR";
                    break;
                case "NVARCHAR (MAX)":
                case "TEXT":
                    dataType = "NTEXT";
                    break;
                case "XML":
                    dataType = "NTEXT";
                    break;
                case "SMALLDATETIME":
                    dataType = "DATETIME";
                    break;
                //newer date formats aren't supported
                case "DATETIME2":
                case "DATE":
                case "TIME":
                case "DATETIMEOFFSET":
                    dataType = "DATETIME";
                    break;
            }



            if (length == -1)
            {
                //VARCHAR(MAX) should be NTEXT
                if (dataType == "NVARCHAR") //we've already changed VARCHAR above
                {
                    dataType = "NTEXT";
                }
                //VARBINARY(MAX) should be IMAGE
                else if (dataType == "VARBINARY")
                {
                    dataType = "IMAGE";
                }
            }


            return dataType;
        }

        protected override string WriteDataTypeWithLength(string dataType, int? length)
        {
            if (length == 0) length = -1; //a zero length varchar doesn't make sense
            if (length == -1)
            {
                switch (dataType)
                {
                    case "VARBINARY":
                    case "BINARY":
                        return "IMAGE";
                    case "NVARCHAR":
                    case "NCHAR":
                        return "NTEXT";
                    case "VARCHAR":
                    case "CHAR":
                        return "TEXT";
                }
            }
            dataType = dataType + " (" + length + ")";
            return dataType;
        }

    }
}
