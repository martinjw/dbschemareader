using System;

namespace DatabaseSchemaReaderTest
{
    /// <summary>
    /// Visual Studio 2008 ONLY - it doesn't have TestCategory. 
    /// </summary>
    public class TestCategoryAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestCategoryAttribute"/> class.
        /// </summary>
        /// <param name="testCategory">The test category.</param>
        public TestCategoryAttribute(string testCategory)
        {
            TestCategory = testCategory;
        }

        /// <summary>
        /// Gets or sets the test category.
        /// </summary>
        /// <value>
        /// The test category.
        /// </value>
        public string TestCategory { get; set; }
    }
}
