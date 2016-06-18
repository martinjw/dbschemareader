#if COREFX

using System;

namespace DatabaseSchemaReader
{
    public class SerializableAttribute : Attribute
    {
    }
    public class NonSerializedAttribute : Attribute
    {
    }

    public static class Extensions
    {
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
