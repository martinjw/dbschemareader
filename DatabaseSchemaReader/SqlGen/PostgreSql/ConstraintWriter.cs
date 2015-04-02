using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.PostgreSql
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

        protected override string ConstraintName(DatabaseConstraint constraint)
        {
            var name = constraint.Name;
            if (string.IsNullOrEmpty(name))
            {
                var columnNames = string.Join("_", constraint.Columns.ToArray());
                //suffixes - http://stackoverflow.com/questions/4107915/postgresql-default-constraint-names/4108266#4108266
                //pkey for a Primary Key constraint
                //key for a Unique constraint
                //excl for an Exclusion constraint
                //idx for any other kind of index
                //fkey for a Foreign key
                //check for a Check constraint
                string suffix;
                switch (constraint.ConstraintType)
                {
                    case ConstraintType.PrimaryKey:
                        suffix = "pkey";
                        break;
                    case ConstraintType.ForeignKey:
                        suffix = "fkey";
                        break;
                    case ConstraintType.UniqueKey:
                        suffix = "key";
                        break;
                    case ConstraintType.Check:
                        suffix = "check";
                        break;
                    default:
                        suffix = "idx";
                        break;
                }

                return Table.Name + "_" + columnNames + "_" + suffix;
            }
            return name;
        }

        #endregion

    }
}
