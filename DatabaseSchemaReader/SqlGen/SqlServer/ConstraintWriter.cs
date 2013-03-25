using System;
using DatabaseSchemaReader.DataSchema;
using System.Globalization;

namespace DatabaseSchemaReader.SqlGen.SqlServer
{
    class ConstraintWriter : ConstraintWriterBase
    {
        public ConstraintWriter(DatabaseTable table)
            : base(table)
        {
        }

        #region Overrides of ConstraintWriterBase

        protected override ISqlFormatProvider SqlFormatProvider()
        {
            return new SqlFormatProvider();
        }

        protected override bool IsSelfReferencingCascadeAllowed()
        {
            return false;
        }

        public override string WritePrimaryKey()
        {
            if (Table.PrimaryKey == null) return null;
            var columnList = GetColumnList(Table.PrimaryKey.Columns);

            var pkName = ConstraintName(Table.PrimaryKey.Name);
            string nonClustered = "";
            if (Table.PrimaryKey.Columns.Count == 1 &&
                "guid".Equals(Table.PrimaryKeyColumn.NetName, StringComparison.OrdinalIgnoreCase))
            {
                nonClustered = "NON CLUSTERED ";
            }

            return string.Format(CultureInfo.InvariantCulture,
                                 @"ALTER TABLE {0} ADD CONSTRAINT {1} PRIMARY KEY {2}({3})",
                                 TableName(Table),
                                 EscapeName(pkName),
                                 nonClustered,
                                 columnList) + SqlFormatProvider().LineEnding();
        }

        #endregion

    }
}
