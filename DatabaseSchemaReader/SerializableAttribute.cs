#if COREFX

using System;

namespace DatabaseSchemaReader
{
    /// <summary>
    /// Dummy attributes because these aren't in NetStandard1.5
    /// </summary>
    public class SerializableAttribute : Attribute
    {
    }
    /// <summary>
    /// Dummy attributes because these aren't in NetStandard1.5
    /// </summary>
    public class NonSerializedAttribute : Attribute
    {
    }

    /// <summary>
    /// Extensions to support stuff not in netStandard1.5
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Like TextInfo.ToTitleCase(value)
        /// </summary>
        public static string ToTitleCase(this System.Globalization.TextInfo textInfo, string value)
        {
//TODO - implementation
            return value;
        }
    }
}

namespace System.Runtime.Serialization
{
    
}

#endif
