using System;

namespace DatabaseSchemaReader.CodeGen
{
    /// <summary>
    /// Names collections by trying to pluralize them. Use with caution!
    /// </summary>
    public class PluralizingNamer : Namer
    {
        #region Override Implementation of ICollectionNamer

        /// <summary>
        /// Names the collection.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <returns></returns>
        public override string NameCollection(string className)
        {
            //you can reference System.Data.Entity.Design.dll
            //use System.Data.Entity.Design.PluralizationServices.PluralizationService.CreateService(CultureInfo.GetCultureInfo("en-us"))
            //it'll be a bit better than this! (though English pluralizing rules are more complex...)
            if (className.EndsWith("ss", StringComparison.OrdinalIgnoreCase))
            {
                return className + "es"; //Addresses
            }
            if (className.EndsWith("o", StringComparison.OrdinalIgnoreCase))
            {
                return className + "es"; //heroes, but not photos, kimonos
            }
            if (className.EndsWith("x", StringComparison.OrdinalIgnoreCase))
            {
                return className + "es"; //Boxes
            }
            if (className.EndsWith("y", StringComparison.OrdinalIgnoreCase))
            {
                return className.Substring(0, className.Length - 1) + "ies"; //Categories
            }
            if (className.Equals("Person", StringComparison.OrdinalIgnoreCase))
            {
                className = "People"; //add other irregulars.
            }
            //sorry for farming applications which will have sheeps
            return className + "s";
        }

        #endregion
    }
}
