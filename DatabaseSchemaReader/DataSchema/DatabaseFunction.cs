using System;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// Represents a database function (that is, a stored procedure that returns a value)
    /// </summary>
    [Serializable]
    public partial class DatabaseFunction : DatabaseStoredProcedure
    {
        private string _returnType;

        /// <summary>
        /// Gets or sets the return type. Will be null for table-valued functions.
        /// </summary>
        /// <value>
        /// The return type.
        /// </value>
        public string ReturnType
        {
            get
            {
                if (!string.IsNullOrEmpty(_returnType))
                    return _returnType;
                //look thru Arguments for a nameless out argument- that's the return type
                foreach (var argument in Arguments)
                {
                    if (argument.Out && string.IsNullOrEmpty(argument.Name))
                    {
                        _returnType = argument.DatabaseDataType;
                        return _returnType;
                    }
                }
                return null;
            }
            set { _returnType = value; }
        }
    }
}
