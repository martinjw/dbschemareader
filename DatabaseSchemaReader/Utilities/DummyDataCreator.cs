using System;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Utilities
{
    /// <summary>
    /// Helpers to create dummy data (for testing)
    /// </summary>
    public static class DummyDataCreator
    {

        /// <summary>
        /// Creates sample data from a <see cref="DatabaseSchemaReader.DataSchema.DataType"/>
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Column is a non-nullable foreign key</exception>
        public static object CreateData(DatabaseColumn column)
        {
            if (column.IsForeignKey)
            {
                if (column.Nullable)
                    return null;
                throw new InvalidOperationException("Column is a non-nullable foreign key - cannot generate data");
            }
            return CreateData(column.DataType, column.Length, column.Precision, column.Scale);
        }

        /// <summary>
        /// Creates sample data from a <see cref="DatabaseSchemaReader.DataSchema.DataType"/>
        /// </summary>
        /// <param name="dataType">Type of the data.</param>
        /// <param name="maxLength">Maximum length of a string</param>
        /// <param name="precision">The precision (total digits).</param>
        /// <param name="scale">The scale (digits after decimal point).</param>
        /// <returns></returns>
        public static object CreateData(DataType dataType, int? maxLength, int? precision, int? scale)
        {
            if (dataType == null)
                return null;
            if (dataType.IsNumeric)
            {
                if (precision == scale) return 0.1;
                return 1;
            }
            if (dataType.IsString)
            {
                var length = maxLength.GetValueOrDefault();
                //a weird error in the devart postgresql provider puts length of char fields in precision
                if (length < 1 && precision > 1) length = precision.Value;
                return GenerateString(length);
            }
            if (dataType.IsDateTime)
                return DateTime.Now;
            if (dataType.GetNetType() == typeof(byte[]))
                return new byte[] { };
            return null;
        }

        private static string GenerateString(int length)
        {
            //try to generate a unique string
            var s = Guid.NewGuid().ToString("N");
            if (length > 8000 || length < 1) return s; //clobs or varchar(max)
            if (32 > length) return s.Substring(0, length);
            return s + new string('s', length - 32);
        }

    }
}
