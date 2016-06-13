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

            var dict = new Dictionary<string, string>();
            //pascalcase
            dict.Add("START_DATE", "StartDate"); //underscore
            dict.Add("EMPLOYEE_ID", "EmployeeId");
            dict.Add("EMPLOYEE ID", "EmployeeId"); //spaces
            dict.Add("CategoryId", "CategoryId"); //if mixed case, preserve it
            //singularization
            dict.Add("Cars", "Car");
            dict.Add("rates", "Rate");
            dict.Add("Boxes", "Box");
            dict.Add("Categories", "Category");
            dict.Add("queries", "Query");
            dict.Add("Statuses", "Status");
            dict.Add("People", "Person");
            //complex titlecase
            dict.Add("les naufragés d'ythaq", "LesNaufragésDythaq");
            dict.Add("Database_IO", "DatabaseIO"); //mixed case with uppercase acronym
            //Id recognition
            dict.Add("CategoryID", "CategoryId");
            //weird db names
            dict.Add("$NAME", "Name");

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

            var dict = new Dictionary<string, string>();
            //pascalcase
            dict.Add("START_DATE", "startDate"); //underscore
            dict.Add("EMPLOYEE_ID", "employeeId");
            dict.Add("EMPLOYEE ID", "employeeId"); //spaces
            dict.Add("CategoryId", "categoryId"); //if mixed case, preserve it
            //no singularization
            dict.Add("Cars", "cars");
            //complex titlecase
            dict.Add("les naufragés d'ythaq", "lesNaufragésDythaq");
            dict.Add("Database_IO", "databaseIO"); //mixed case with uppercase acronym
            //Id recognition
            dict.Add("CategoryID", "categoryId");
            //weird db names
            dict.Add("$NAME", "name");
            //c# keywords
            dict.Add("NAMESPACE", "@namespace");
            dict.Add("CLASS", "@class");

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
