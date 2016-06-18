
using System;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Oracle
{
    static class Conversion
    {

        /// <summary>
        /// Does the column default value look like a sequence allocation ("mysequence.NextVal")?
        /// </summary>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static bool LooksLikeAutoNumberDefaults(string defaultValue)
        {
            if (string.IsNullOrEmpty(defaultValue)) return false;
            //simple cases only. If the sequence.nextval is cast/converted, 
            return defaultValue.IndexOf(".NEXTVAL", StringComparison.OrdinalIgnoreCase) != -1 ||
                defaultValue.IndexOf(".CURRVAL", StringComparison.OrdinalIgnoreCase) != -1;
        }
    }
}
