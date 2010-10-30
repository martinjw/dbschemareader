using System;
using System.Diagnostics;

namespace DatabaseSchemaReader.DataSchema
{
    [Serializable]
    public class DatabaseColumn
    {
        #region Fields

        #endregion

        #region Properties

        public DatabaseSchema DatabaseSchema { get; set; }

        public DataType DataType { get; set; }

        public string DbDataType { get; set; }

        public string ForeignKeyTableName { get; set; }

        public DatabaseTable ForeignKeyTable { get; set; }

        public bool IsForeignKey { get; set; }

        public bool IsIdentity { get; set; }

        public bool IsIndexed { get; set; }

        public bool IsPrimaryKey { get; set; }

        public bool IsUniqueKey { get; set; }

        public int? Length { get; set; }

        public string Name { get; set; }

        public bool Nullable { get; set; }

        public int Ordinal { get; set; }

        public int? Precision { get; set; }

        public int? Scale { get; set; }

        public int? DateTimePrecision { get; set; }

        public string TableName { get; set; }

        public string DefaultValue { get; set; }



        #endregion Data Members

        #region Utility methods


        public override string ToString()
        {
            return Name + " (" + DbDataType + ")"
                + (IsPrimaryKey ? " PK" : "")
                + (IsIdentity ? " Identity" : "");
        }

        #endregion
    }
}
