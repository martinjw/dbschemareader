using System;
using System.Text.RegularExpressions;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.SqlServerCe
{
    /// <summary>
    /// Table generator based on full SqlServer but using a derived DataTypeWriter for the subset of datatypes
    /// </summary>
    class TableGenerator : SqlServer.TableGenerator
    {
        public TableGenerator(DatabaseTable table)
            : base(table)
        {
            DataTypeWriter = new DataTypeWriter();
        }

        protected override ISqlFormatProvider SqlFormatProvider()
        {
            return new SqlServerCeFormatProvider();
        }

        protected override string AddTableDescription()
        {
            return null;
        }
        protected override string AddColumnDescriptions()
        {
            return null;
        }

        protected override ConstraintWriterBase CreateConstraintWriter()
        {
            //check constraints don't seem to be included so ignore them all
            var constraintWriter = new ConstraintWriter(Table)
                                   {
                                       IncludeSchema = IncludeSchema,
                                       CheckConstraintExcluder = check => true
                                   };
            return constraintWriter;
        }

        protected override string FixDefaultValue(string defaultValue)
        {
            if (string.IsNullOrEmpty(defaultValue)) return defaultValue;
            //we only have getdate() function
            var getDate = defaultValue.IndexOf("current_timestamp", StringComparison.OrdinalIgnoreCase);
            if (getDate != -1)
            {
                defaultValue = defaultValue.Remove(getDate, "current_timestamp".Length).Insert(getDate, "getdate()");
            }
            var sysDateTime = defaultValue.IndexOf("sysdatetime()", StringComparison.OrdinalIgnoreCase);
            if (sysDateTime != -1)
            {
                defaultValue = defaultValue.Remove(sysDateTime, "sysdatetime()".Length).Insert(sysDateTime, "getdate()");
            }
            //remove braces around numbers
            defaultValue = SqlTranslator.RemoveParenthesis(defaultValue);
            defaultValue = Regex.Replace(defaultValue, @"\((\d+)\)", "$1");
            return defaultValue;
        }

        /// <summary>
        /// Gets a value indicating whether supports "next value for [sequence]" (SQLServer 2012+). 
        /// </summary>
        protected override bool SupportsNextValueForSequence { get { return false; } }

        protected override IMigrationGenerator CreateMigrationGenerator()
        {
            //this will ensure the add constraints don't have schema naming
            return new SqlServerCeMigrationGenerator();
        }

        protected override bool HandleComputed(DatabaseColumn column)
        {
            return false; //computed columns aren't supported
        }
    }
}
