using System;
using System.Diagnostics;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// Represents a database function (that is, a stored procedure that returns a value)
    /// </summary>
    [Serializable]
    public partial class DatabaseFunction : DatabaseStoredProcedure
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
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
                CheckArgumentsForReturnType();
                return _returnType;
            }
            set { _returnType = value; }
        }

        /// <summary>
        /// Checks the arguments for a return type and reassigns it. Once a return type is found, this doesn't do anything.
        /// </summary>
        public void CheckArgumentsForReturnType()
        {
            if (!string.IsNullOrEmpty(_returnType)) return;
            //look thru Arguments for a nameless out argument- that's the return type
            DatabaseArgument returnArgument = null;
            foreach (var argument in Arguments)
            {
                if ((argument.Out && string.IsNullOrEmpty(argument.Name)) ||
                    //MySql parameters may be marked "IN" so we look for ordinal and name
                    (argument.Name.Equals("RETURN_VALUE", StringComparison.OrdinalIgnoreCase) && 
                    argument.Ordinal == 0))
                {
                    returnArgument = argument;
                    break;
                }
            }
            //couldn't find one
            if (returnArgument == null) return;

            _returnType = returnArgument.DatabaseDataType;
            Arguments.Remove(returnArgument);
        }
    }
}
