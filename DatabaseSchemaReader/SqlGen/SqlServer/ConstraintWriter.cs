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
                //UNIQUEIDENTIFIERs may have NON CLUSTERED indexes
                //the pk index will have IndexType of PRIMARY NONCLUSTERED
                var pkIndex = Table.Indexes.Find(x => x.IndexType?.IndexOf("PRIMARY", StringComparison.OrdinalIgnoreCase) != -1);
                if (pkIndex != null &&
                    pkIndex.IndexType?.IndexOf("NONCLUSTERED", StringComparison.OrdinalIgnoreCase) > -1)
                {
                    nonClustered = "NONCLUSTERED";
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
    }
}
