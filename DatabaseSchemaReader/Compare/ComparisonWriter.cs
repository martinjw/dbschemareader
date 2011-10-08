using System;
using System.Text.RegularExpressions;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;

namespace DatabaseSchemaReader.Compare
{
    /// <summary>
    /// Quick facade over migration generator
    /// </summary>
    class ComparisonWriter
    {
        private readonly IMigrationGenerator _migration;
        private readonly SqlType _sqlType;

        public ComparisonWriter(SqlType sqlType)
        {
            _sqlType = sqlType;
            var ddlFactory = new DdlGeneratorFactory(sqlType);
            _migration = ddlFactory.MigrationGenerator();
        }

        public string AddTable(DatabaseTable databaseTable)
        {
            return _migration.AddTable(databaseTable);
        }

        public string AddColumn(DatabaseTable databaseTable, DatabaseColumn databaseColumn)
        {
            return _migration.AddColumn(databaseTable, databaseColumn);
        }

        public string AddConstraint(DatabaseTable databaseTable, DatabaseConstraint databaseConstraint)
        {
            return _migration.AddConstraint(databaseTable, databaseConstraint);
        }

        public string AlterColumn(DatabaseTable databaseTable, DatabaseColumn databaseColumn, DatabaseColumn originalColumn)
        {
            return _migration.AlterColumn(databaseTable, databaseColumn, originalColumn);
        }

        public string DropConstraint(DatabaseTable databaseTable, DatabaseConstraint databaseConstraint)
        {
            return _migration.DropConstraint(databaseTable, databaseConstraint);
        }

        public string DropColumn(DatabaseTable databaseTable, DatabaseColumn databaseColumn)
        {
            return _migration.DropColumn(databaseTable, databaseColumn);
        }

        public string DropTable(DatabaseTable databaseTable)
        {
            return _migration.DropTable(databaseTable);
        }

        public string AddView(DatabaseView view)
        {
            return _migration.AddView(view);
        }

        public string DropView(DatabaseView view)
        {
            return _migration.DropView(view);
        }

        /// <summary>
        /// Sanitized comparison of the view sql
        /// </summary>
        /// <param name="view1">The view1.</param>
        /// <param name="view2">The view2.</param>
        /// <returns></returns>
        public bool CompareView(string view1, string view2)
        {
            //trim any extra whitespace around the sql
            var sql1 = (view1 == null) ? string.Empty : view1.Trim();
            var sql2 = view2.Trim();
            if (_sqlType != SqlType.SqlServerCe && _sqlType != SqlType.SqlServer)
            {
                return sql1 == sql2;
            }

            //the create view could take many forms:
            //create view "Alphabetical list of products" AS ...
            //create view [dbo].[Alphabetical list of products] AS ...

            //let's strip that bit for the comparison...

            sql1 = SanitizeSql.StripComments(sql1);
            sql2 = SanitizeSql.StripComments(sql2);

            var reg = new Regex(@"\bCREATE VIEW\b(.*?)(?=\bAS\b)", RegexOptions.IgnoreCase);
            var match = reg.Match(sql1);
            if (match.Success)
            {
                sql1 = sql1.Remove(match.Index, match.Length);
            }
            match = reg.Match(sql2);
            if (match.Success)
            {
                sql2 = sql2.Remove(match.Index, match.Length);
            }
            return sql1 == sql2;
        }

        public bool CompareProcedure(string procedure1, string procedure2)
        {
            //trim any extra whitespace around the sql
            var sql1 = procedure1.Trim();
            var sql2 = procedure2.Trim();
            if (_sqlType != SqlType.SqlServerCe && _sqlType != SqlType.SqlServer)
            {
                return sql1 == sql2;
            }

            sql1 = SanitizeSql.StripComments(sql1);
            sql2 = SanitizeSql.StripComments(sql2);

            return sql1 == sql2;
        }

        public string AddProcedure(DatabaseStoredProcedure procedure)
        {
            return _migration.AddProcedure(procedure);
        }

        public string DropProcedure(DatabaseStoredProcedure procedure)
        {
            return _migration.DropProcedure(procedure);
        }

        public string AddIndex(DatabaseTable databaseTable, DatabaseIndex index)
        {
            return _migration.AddIndex(databaseTable, index);
        }

        public string DropIndex(DatabaseTable databaseTable, DatabaseIndex index)
        {
            return _migration.DropIndex(databaseTable, index);
        }

        public string AddTrigger(DatabaseTable databaseTable, DatabaseTrigger trigger)
        {
            return _migration.AddTrigger(databaseTable, trigger);
        }

        public string DropTrigger(DatabaseTable databaseTable, DatabaseTrigger trigger)
        {
            return _migration.DropTrigger(trigger);
        }

        public string RunStatements()
        {
            return _migration.RunStatements();
        }

        public string DropSequence(DatabaseSequence sequence)
        {
            return _migration.DropSequence(sequence);
        }

        public string AddSequence(DatabaseSequence sequence)
        {
            return _migration.AddSequence(sequence);
        }

        public string DropFunction(DatabaseFunction function)
        {
            return _migration.DropFunction(function);
        }

        public string AddFunction(DatabaseFunction function)
        {
            return _migration.AddFunction(function);
        }

        public string AddPackage(DatabasePackage package)
        {
            return _migration.AddPackage(package);
        }

        public string DropPackage(DatabasePackage package)
        {
            return _migration.DropPackage(package);
        }

    }
}
