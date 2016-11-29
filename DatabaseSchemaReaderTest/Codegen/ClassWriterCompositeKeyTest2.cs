using System;
using DatabaseSchemaReader.CodeGen;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.Codegen
{
    [TestClass]
    public class ClassWriterCompositeKeyTest2
    {
        [TestMethod]
        public void TestCompositeKey()
        {
            //based on Github issue #21
            //arrange
            var dept = Arrange();

            var cw = new ClassWriter(dept, new CodeWriterSettings { CodeTarget = CodeTarget.PocoEntityCodeFirst});

            //act
            var txt = cw.Write();

            //assert
            //what should this return?
            Console.WriteLine(txt);
        }

        private static DatabaseTable Arrange()
        {
            var schema = new DatabaseSchema(null, SqlType.SqlServer);
            schema.DataTypes.Add(new DataType("INT", "System.Int32"));
            schema.DataTypes.Add(new DataType("NVARCHAR", "System.String"));

            var bu = schema.AddTable("BusinessUnit")
                .AddColumn("BusinessUnitId", "INT").AddPrimaryKey().AddIdentity()
                .AddColumn("UnitName", "NVARCHAR")
                .Table;
            var dept = schema.AddTable("Department")
                .AddColumn("DepartmentId", "INT").AddPrimaryKey()
                .AddColumn("BusinessUnitId", "INT").AddForeignKey("FK", "BusinessUnit")
                .Table;
            var pk = new DatabaseConstraint {ConstraintType = ConstraintType.PrimaryKey};
            pk.Columns.Add("DepartmentId");
            pk.Columns.Add("BusinessUnitId");
            dept.AddConstraint(pk);

            DatabaseSchemaFixer.UpdateDataTypes(schema);
            //make sure .Net names are assigned
            PrepareSchemaNames.Prepare(schema, new Namer());
            return dept;
        }
    }
}
