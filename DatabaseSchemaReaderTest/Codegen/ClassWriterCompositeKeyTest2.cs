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
            //this schema model has a primary key which is also in 2 different foreign keys
            //AND one of the foreign keys is not in the model (exclusions)
            //this confused the code writer...
            var dept = Arrange();

            var settings = new CodeWriterSettings
            {
                CodeTarget = CodeTarget.PocoEntityCodeFirst,
                Namer = new PluralizingNamer()
            };
            var cw = new ClassWriter(dept, settings);

            //act
            var txt = cw.Write();

            //assert
            //what should this return?
            Console.WriteLine(txt);
            Assert.IsTrue(txt.IndexOf("public int BusinessUnitId { get; set; }", StringComparison.Ordinal) > 0,
                "Should have xId scalar key");
            Assert.IsTrue(txt.IndexOf("public virtual BusinessUnit BusinessUnit { get; set; }", StringComparison.Ordinal) > 0,
                "Should have x reference property");
        }

        private static DatabaseTable Arrange()
        {
            var schema = new DatabaseSchema(null, SqlType.SqlServer);
            schema.DataTypes.Add(new DataType("NUMBER", "System.Int32"));
            schema.DataTypes.Add(new DataType("VARCHAR2", "System.String"));

            var bu = schema.AddTable("BUSINESS_UNITS")
                .AddColumn("BUSINESS_UNIT_ID", "NUMBER").AddPrimaryKey().AddIdentity()
                .AddColumn("DESCRIPTION", "VARCHAR2")
                .Table;
            var dept = schema.AddTable("DEPARTMENTS")
                .AddColumn("BUSINESS_UNIT_ID", "NUMBER")
                .AddColumn("DEPARTMENT_ID", "VARCHAR2").AddLength(4)
                .AddColumn("GROUP_ID", "NUMBER")
                .Table;
            var businessUnitFk = new DatabaseConstraint
            {
                ConstraintType = ConstraintType.ForeignKey,
                RefersToTable = bu.Name
            };
            businessUnitFk.Columns.Add("BUSINESS_UNIT_ID");
            dept.AddConstraint(businessUnitFk);
            //This one IS NOT in the model, so there are broken fk links
            var groupFk = new DatabaseConstraint
            {
                ConstraintType = ConstraintType.ForeignKey,
                RefersToTable = "GROUPS"
            };
            groupFk.Columns.Add("BUSINESS_UNIT_ID");
            groupFk.Columns.Add("DEPARTMENT_ID");
            groupFk.Columns.Add("GROUP_ID");
            dept.AddConstraint(groupFk);
            var pk = new DatabaseConstraint { ConstraintType = ConstraintType.PrimaryKey };
            pk.Columns.Add("BUSINESS_UNIT_ID");
            pk.Columns.Add("DEPARTMENT_ID");
            dept.AddConstraint(pk);

            DatabaseSchemaFixer.UpdateDataTypes(schema);

            return dept;
        }
    }
}