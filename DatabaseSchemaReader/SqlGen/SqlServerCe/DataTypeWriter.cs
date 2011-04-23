namespace DatabaseSchemaReader.SqlGen.SqlServerCe
{

    /// <summary>
    /// SqlServer CE is a subset of SqlServer, so we derive from it and downgrade certain types
    /// </summary>
    class DataTypeWriter : SqlServer.DataTypeWriter
    {
        protected override string ConvertOtherPlatformTypes(string dataType, int providerType, int? length, int? precision, int? scale)
        {
            dataType = base.ConvertOtherPlatformTypes(dataType, providerType, length, precision, scale);

            //do the subset of SqlServer features of CE
            //MSDN list = http://msdn.microsoft.com/en-us/library/ms172424%28v=SQL.110%29.aspx

            switch (dataType)
            {
                case "VARCHAR":
                    dataType = "NVARCHAR";
                    break;
                case "CHAR":
                    dataType = "NCHAR";
                    break;
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
                    dataType = "NVARCHAR";
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
    }
}
