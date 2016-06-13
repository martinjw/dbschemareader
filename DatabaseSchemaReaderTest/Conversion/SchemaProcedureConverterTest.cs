using System.Data;
using System.IO;
using DatabaseSchemaReader.Conversion;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.Conversion
{
    [TestClass]
    public class SchemaProcedureConverterTest
    {
        [TestMethod]
        public void UpdateArgumentsOracleTest()
        {
            //arrange
            var converter = new SchemaProcedureConverter();
            var schema = new DatabaseSchema(null, null);
            #region deserialize datatable
            //from Oracle HR. Captured from var datatable.WriteXml(StringWriter)
            const string data = @"<DocumentElement>
  <ProcedureParameters>
    <Schema>HR</Schema>
    <Procedure>GET_CURRENT_TIME</Procedure>
    <Name>P_DATE</Name>
    <Position>1</Position>
    <Sequence>1</Sequence>
    <DataType>DATE</DataType>
    <Direction>IN/OUT</Direction>
  </ProcedureParameters>
  <ProcedureParameters>
    <Schema>HR</Schema>
    <Procedure>ADD_JOB_HISTORY</Procedure>
    <Name>P_EMP_ID</Name>
    <Position>1</Position>
    <Sequence>1</Sequence>
    <DataType>NUMBER</DataType>
    <Direction>IN</Direction>
    <Length>22</Length>
    <Precision>6</Precision>
  </ProcedureParameters>
  <ProcedureParameters>
    <Schema>HR</Schema>
    <Procedure>ADD_JOB_HISTORY</Procedure>
    <Name>P_START_DATE</Name>
    <Position>2</Position>
    <Sequence>2</Sequence>
    <DataType>DATE</DataType>
    <Direction>IN</Direction>
  </ProcedureParameters>
  <ProcedureParameters>
    <Schema>HR</Schema>
    <Procedure>ADD_JOB_HISTORY</Procedure>
    <Name>P_END_DATE</Name>
    <Position>3</Position>
    <Sequence>3</Sequence>
    <DataType>DATE</DataType>
    <Direction>IN</Direction>
  </ProcedureParameters>
  <ProcedureParameters>
    <Schema>HR</Schema>
    <Procedure>ADD_JOB_HISTORY</Procedure>
    <Name>P_JOB_ID</Name>
    <Position>4</Position>
    <Sequence>4</Sequence>
    <DataType>VARCHAR2</DataType>
    <Direction>IN</Direction>
    <Length>10</Length>
    <Charset>CHAR_CS</Charset>
  </ProcedureParameters>
  <ProcedureParameters>
    <Schema>HR</Schema>
    <Procedure>ADD_JOB_HISTORY</Procedure>
    <Name>P_DEPARTMENT_ID</Name>
    <Position>5</Position>
    <Sequence>5</Sequence>
    <DataType>NUMBER</DataType>
    <Direction>IN</Direction>
    <Length>22</Length>
    <Precision>4</Precision>
  </ProcedureParameters>
</DocumentElement>
";
            var sr = new StringReader(data);
            var ds = new DataSet();
            ds.ReadXml(sr);
            var dataTable = ds.Tables[0];
            #endregion

            //act
            converter.UpdateArguments(schema, dataTable);

            //assert
            var sprocs = schema.StoredProcedures;
            Assert.AreEqual(2, sprocs.Count);

            var addHistory = sprocs.Find(x => x.Name == "ADD_JOB_HISTORY");
            Assert.AreEqual(5, addHistory.Arguments.Count);
            var empId = addHistory.Arguments.Find(x => x.Name == "P_EMP_ID");
            Assert.AreEqual(1, empId.Ordinal);
            Assert.AreEqual("NUMBER", empId.DatabaseDataType);
            Assert.AreEqual(true, empId.In);
            Assert.AreEqual(false, empId.Out);


            var currentTime = sprocs.Find(x => x.Name == "GET_CURRENT_TIME");
            var date = currentTime.Arguments.Find(x => x.Name == "P_DATE");
            Assert.AreEqual(1, date.Ordinal);
            Assert.AreEqual("DATE", date.DatabaseDataType);
            //inout type!
            Assert.AreEqual(true, date.In);
            Assert.AreEqual(true, date.Out);
        }

        [TestMethod]
        public void UpdateArgumentsSqlServerTest()
        {
            //arrange
            var converter = new SchemaProcedureConverter();
            var schema = new DatabaseSchema(null, null);
            #region deserialize datatable
            //from SqlServer Northwind. Captured from var datatable.WriteXml(StringWriter)
            const string data = @"<DocumentElement>
  <ProcedureParameters>
    <SPECIFIC_CATALOG>Northwind</SPECIFIC_CATALOG>
    <SPECIFIC_SCHEMA>dbo</SPECIFIC_SCHEMA>
    <SPECIFIC_NAME>GetWeekDay</SPECIFIC_NAME>
    <ORDINAL_POSITION>0</ORDINAL_POSITION>
    <PARAMETER_MODE>OUT</PARAMETER_MODE>
    <IS_RESULT>YES</IS_RESULT>
    <AS_LOCATOR>NO</AS_LOCATOR>
    <PARAMETER_NAME />
    <DATA_TYPE>int</DATA_TYPE>
    <NUMERIC_PRECISION>10</NUMERIC_PRECISION>
    <NUMERIC_PRECISION_RADIX>10</NUMERIC_PRECISION_RADIX>
    <NUMERIC_SCALE>0</NUMERIC_SCALE>
  </ProcedureParameters>
  <ProcedureParameters>
    <SPECIFIC_CATALOG>Northwind</SPECIFIC_CATALOG>
    <SPECIFIC_SCHEMA>dbo</SPECIFIC_SCHEMA>
    <SPECIFIC_NAME>GetWeekDay</SPECIFIC_NAME>
    <ORDINAL_POSITION>1</ORDINAL_POSITION>
    <PARAMETER_MODE>IN</PARAMETER_MODE>
    <IS_RESULT>NO</IS_RESULT>
    <AS_LOCATOR>NO</AS_LOCATOR>
    <PARAMETER_NAME>@Date</PARAMETER_NAME>
    <DATA_TYPE>datetime</DATA_TYPE>
    <DATETIME_PRECISION>3</DATETIME_PRECISION>
  </ProcedureParameters>
  <ProcedureParameters>
    <SPECIFIC_CATALOG>Northwind</SPECIFIC_CATALOG>
    <SPECIFIC_SCHEMA>dbo</SPECIFIC_SCHEMA>
    <SPECIFIC_NAME>GetCurrentDate</SPECIFIC_NAME>
    <ORDINAL_POSITION>1</ORDINAL_POSITION>
    <PARAMETER_MODE>INOUT</PARAMETER_MODE>
    <IS_RESULT>NO</IS_RESULT>
    <AS_LOCATOR>NO</AS_LOCATOR>
    <PARAMETER_NAME>@p1</PARAMETER_NAME>
    <DATA_TYPE>datetime2</DATA_TYPE>
    <DATETIME_PRECISION>7</DATETIME_PRECISION>
  </ProcedureParameters>
  <ProcedureParameters>
    <SPECIFIC_CATALOG>Northwind</SPECIFIC_CATALOG>
    <SPECIFIC_SCHEMA>dbo</SPECIFIC_SCHEMA>
    <SPECIFIC_NAME>Sales by Year</SPECIFIC_NAME>
    <ORDINAL_POSITION>1</ORDINAL_POSITION>
    <PARAMETER_MODE>IN</PARAMETER_MODE>
    <IS_RESULT>NO</IS_RESULT>
    <AS_LOCATOR>NO</AS_LOCATOR>
    <PARAMETER_NAME>@Beginning_Date</PARAMETER_NAME>
    <DATA_TYPE>datetime</DATA_TYPE>
    <DATETIME_PRECISION>3</DATETIME_PRECISION>
  </ProcedureParameters>
  <ProcedureParameters>
    <SPECIFIC_CATALOG>Northwind</SPECIFIC_CATALOG>
    <SPECIFIC_SCHEMA>dbo</SPECIFIC_SCHEMA>
    <SPECIFIC_NAME>Sales by Year</SPECIFIC_NAME>
    <ORDINAL_POSITION>2</ORDINAL_POSITION>
    <PARAMETER_MODE>IN</PARAMETER_MODE>
    <IS_RESULT>NO</IS_RESULT>
    <AS_LOCATOR>NO</AS_LOCATOR>
    <PARAMETER_NAME>@Ending_Date</PARAMETER_NAME>
    <DATA_TYPE>datetime</DATA_TYPE>
    <DATETIME_PRECISION>3</DATETIME_PRECISION>
  </ProcedureParameters>
</DocumentElement>";
            var sr = new StringReader(data);
            var ds = new DataSet();
            ds.ReadXml(sr);
            var dataTable = ds.Tables[0];
            #endregion

            //act
            converter.UpdateArguments(schema, dataTable);

            //assert
            var sprocs = schema.StoredProcedures;
            Assert.AreEqual(2, sprocs.Count); //function GetWeekDay is ignored
            Assert.AreEqual(1, schema.Functions.Count); //here's where GetWeekDay is

            var salesByYear = sprocs.Find(x => x.Name == "Sales by Year");
            Assert.AreEqual(2, salesByYear.Arguments.Count);
            var date = salesByYear.Arguments.Find(x => x.Name == "@Beginning_Date");
            Assert.AreEqual(1, date.Ordinal);
            Assert.AreEqual("datetime", date.DatabaseDataType);
            Assert.AreEqual(true, date.In);
            Assert.AreEqual(false, date.Out);


            var currentTime = sprocs.Find(x => x.Name == "GetCurrentDate");
            Assert.AreEqual(1, currentTime.Arguments.Count);
            var p1 = currentTime.Arguments.Find(x => x.Name == "@p1");
            Assert.AreEqual(1, p1.Ordinal);
            Assert.AreEqual("datetime2", p1.DatabaseDataType);
            //in-out type!
            Assert.AreEqual(true, p1.In);
            Assert.AreEqual(true, p1.Out);

            var fun = schema.Functions[0];
            Assert.AreEqual("int", fun.ReturnType);
            Assert.AreEqual(1, fun.Arguments.Count); //even though we had 2 in the data table
            var input = fun.Arguments[0];
            Assert.AreEqual("@Date", input.Name);
            Assert.AreEqual("datetime", input.DatabaseDataType);
            Assert.AreEqual(true, input.In);

        }
    }
}
