using System;
using System.Linq;
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

        public override string WritePrimaryKey(DatabaseConstraint constraint)
        {
            if (constraint == null) return null;
            var columnList = GetColumnList(constraint.Columns);

            var pkName = ConstraintName(constraint);
            var nonClustered = "";
            if (constraint.Columns.Count == 1)
            {
                //UNIQUEIDENTIFIERs should have NON CLUSTERED indexes
                var colName = constraint.Columns[0];
                var col = Table.FindColumn(colName);
                if (col != null)
                {
                    colName = col.NetName;
                    if (string.Equals(col.DbDataType, "UNIQUEIDENTIFIER", StringComparison.OrdinalIgnoreCase))
                    {
                        nonClustered = "NONCLUSTERED ";
                    }
                }
                if ("guid".Equals(colName, StringComparison.OrdinalIgnoreCase))
                {
                    nonClustered = "NONCLUSTERED ";
                }
            }

            return string.Format(CultureInfo.InvariantCulture,
                                 @"ALTER TABLE {0} ADD CONSTRAINT {1} PRIMARY KEY {2}({3})",
                                 TableName(Table),
                                 EscapeName(pkName),
                                 nonClustered,
                                 columnList) + SqlFormatProvider().LineEnding();
        }

        protected override string WriteDefaultConstraint(DatabaseConstraint constraint)
        {
            var column = EscapeName(constraint.Columns.FirstOrDefault());
            return string.Format(CultureInfo.InvariantCulture,
                                 @"ALTER TABLE {0} ADD CONSTRAINT {1} DEFAULT {2} FOR {3}",
                                 TableName(Table),
                                 EscapeName(constraint.Name),
                                 constraint.Expression,
                                 column) + SqlFormatProvider().LineEnding();
        }

        #endregion

    }
}
