using System;
using System.Globalization;
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

        public ComparisonWriter(SqlType sqlType)
        {
            var ddlFactory = new DdlGeneratorFactory(sqlType);
            _migration = ddlFactory.MigrationGenerator();
        }

        public string WriteTable(DatabaseTable databaseTable)
        {
            return _migration.CreateTable(databaseTable);
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
    }
}
