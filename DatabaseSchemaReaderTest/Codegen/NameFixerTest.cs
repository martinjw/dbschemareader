using System.Collections.Generic;
using DatabaseSchemaReader.CodeGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace DatabaseSchemaReaderTest.Codegen
{
    /// <summary>
    ///Test NameFixer 
    ///</summary>
    [TestClass]
    public class NameFixerTest
    {
        /// <summary>
        ///A test for ToPascalCase
        ///</summary>
        [TestMethod]
        public void ToPascalCaseTest()
        {
            //ARRANGE

            var dict = new Dictionary<string, string>
            {
                {"START_DATE", "StartDate"},
                {"EMPLOYEE_ID", "EmployeeId"},
                {"EMPLOYEE ID", "EmployeeId"},
                {"CategoryId", "CategoryId"},
                {"Cars", "Car"},
                {"rates", "Rate"},
                {"Boxes", "Box"},
                {"Categories", "Category"},
                {"queries", "Query"},
                {"Statuses", "Status"},
                {"People", "Person"},
                {"les naufragés d'ythaq", "LesNaufragésDythaq"},
                {"Database_IO", "DatabaseIO"},
                {"CategoryID", "CategoryId"},
                {"$NAME", "Name"}
            };
            //pascalcase
            //underscore
            //spaces
            //if mixed case, preserve it
            //singularization
            //complex titlecase
            //mixed case with uppercase acronym
            //Id recognition
            //weird db names

            foreach (var item in dict)
            {
                var name = item.Key;
                var expected = item.Value;

                //ACT
                var actual = NameFixer.ToPascalCase(name);

                //ASSERT
                Assert.AreEqual(expected, actual);
            }

        }

        [TestMethod]
        public void ToCamelCaseTest()
        {
            //ARRANGE

            var dict = new Dictionary<string, string>
            {
                {"START_DATE", "startDate"},
                {"EMPLOYEE_ID", "employeeId"},
                {"EMPLOYEE ID", "employeeId"},
                {"CategoryId", "categoryId"},
                {"Cars", "cars"},
                {"les naufragés d'ythaq", "lesNaufragésDythaq"},
                {"Database_IO", "databaseIO"},
                {"CategoryID", "categoryId"},
                {"$NAME", "name"},
                {"NAMESPACE", "@namespace"},
                {"CLASS", "@class"}
            };
            //pascalcase
            //underscore
            //spaces
            //if mixed case, preserve it
            //no singularization
            //complex titlecase
            //mixed case with uppercase acronym
            //Id recognition
            //weird db names
            //c# keywords

            foreach (var item in dict)
            {
                var name = item.Key;
                var expected = item.Value;

                //ACT
                var actual = NameFixer.ToCamelCase(name);

                //ASSERT
                Assert.AreEqual(expected, actual);
            }
        }
    }
}
