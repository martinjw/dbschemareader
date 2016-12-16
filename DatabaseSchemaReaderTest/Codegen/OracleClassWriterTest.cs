using System;
using DatabaseSchemaReader.CodeGen;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.Codegen
{
    [TestClass]
    public class OracleClassWriterTest
    {
        /// <summary>
        /// EF with Oracle Managed Client doesn't like decimals that should be ints.
        /// </summary>
        [TestMethod]
        public void TestIntDataTypes()
        {
            //arrange
            var schema = new DatabaseSchema(null, SqlType.Oracle);
            schema.AddTable("DEPARTMENTS")
                .AddColumn("DEPARTMENT_ID", "NUMBER(4,0)").AddPrimaryKey()
                .AddColumn("DEPARTMENT_NAME","VARCHAR2(30)");
            var depts = schema.FindTableByName("DEPARTMENTS");
            var cw = new ClassWriter(depts, new CodeWriterSettings
            {
                CodeTarget = CodeTarget.PocoEntityCodeFirst,
            });

            //act
            //key line is DataTypeWriter.FindDataType(column) 
            // calling dt.NetDataTypeCSharpName
            //also mapping (HasPrecision is invalid for ints)
            var code = cw.Write();

            //assert
            Assert.IsTrue(code.Contains("public short DepartmentId { get; set; }"), "Type of Id should be short, not decimal");

        }
    }
}
