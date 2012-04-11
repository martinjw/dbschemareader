using System;
using System.Globalization;
using System.Text;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.PostgreSql
{
    /// <summary>
    /// Turn a column defined as IDENTITY into a PostgreSql sequence
    /// </summary>
    class AutoIncrementWriter
    {
        private readonly DatabaseTable _table;

        public AutoIncrementWriter(DatabaseTable table)
        {
            _table = table;
        }

        public string Write()
        {
            if(!_table.HasIdentityColumn)
                return String.Empty;

            var sb = new StringBuilder();
            foreach (DatabaseColumn column in _table.Columns.FindAll(x => x.IsNonTrivialIdentity()))
            {
                sb.AppendLine("-- sequence for " + _table.Name + "." + column.Name);
                sb.AppendLine(WriteSequence(column));
                sb.AppendLine();
            }
            return sb.ToString();
        }

        internal static string GetSequenceName(DatabaseColumn column)
        {
            return column.TableName.ToLowerInvariant() + "_" + column.Name.ToLowerInvariant() + "_seq";
        }

        private static string WriteSequence(DatabaseColumn column)
        {
            const string sequence = @"CREATE SEQUENCE ""{0}"" MINVALUE {1} START {1} INCREMENT {2};
ALTER TABLE ""{3}"" ALTER COLUMN ""{4}"" SET DEFAULT nextval('""{0}""');";

            return string.Format(CultureInfo.InvariantCulture,
                sequence,
                GetSequenceName(column),
                column.IdentitySeed,
                column.IdentityIncrement,
                column.TableName,
                column.Name
                );
        }
    }
}
