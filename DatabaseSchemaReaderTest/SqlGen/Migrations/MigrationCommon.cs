using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReaderTest.SqlGen.Migrations
{
    internal class MigrationCommon
    {
        public static DatabaseTable CreateTestTable(string tableName)
        {
            //we only need a schema because MySQL foreign key references do not allow just the foreign key table name- they need the columns too
            var schema = new DatabaseSchema(null, null);

            var testTable = new DatabaseTable { Name = tableName, Description = "This is a test table" };
            schema.Tables.Add(testTable);
            testTable.DatabaseSchema = schema; //the migration will discover this and know how to link the self referencing table

            var intDataType = new DataType("INT", "System.Int32");
            var idColumn = new DatabaseColumn
            {
                Name = "Id",
                DbDataType = "int",
                DataType = intDataType,
                Nullable = false,
                Description = "Primary key",
            };
            testTable.Columns.Add(idColumn);

            var parentColumn = new DatabaseColumn
            {
                Name = "Parent", //for a self-referencing foreign key
                DbDataType = "int",
                DataType = intDataType,
                Nullable = true,
                Description = "Self referencing foreign key",
            };
            testTable.Columns.Add(parentColumn);

            var nameColumn = new DatabaseColumn
            {
                Name = "NAME",
                DbDataType = "VARCHAR",
                Length = 10,
                DataType = new DataType("VARCHAR", "string"),
                Description = "Simple varchar column",
            };
            testTable.Columns.Add(nameColumn);

            var primaryKey = new DatabaseConstraint { ConstraintType = ConstraintType.PrimaryKey, Name = "PK_" + tableName };
            primaryKey.Columns.Add("Id");
            testTable.PrimaryKey = primaryKey;

            return testTable;
        }

        public static DatabaseColumn CreateNewColumn()
        {
            return new DatabaseColumn
            {
                Name = "COUNTRY",
                DbDataType = "VARCHAR",
                Length = 20,
                DataType = new DataType("VARCHAR", "string"),
                Nullable = false //DB2 doesn't allow unique constraints on nullable columns. Others are fine with it.
            };
        }

        public static DatabaseConstraint CreateUniqueConstraint(DatabaseColumn column)
        {
            var constraint = new DatabaseConstraint
            {
                Name = "UK_COUNTRY",
                ConstraintType = ConstraintType.UniqueKey,
            };
            constraint.Columns.Add(column.Name);
            return constraint;
        }

        public static DatabaseIndex CreateUniqueIndex(DatabaseColumn column, string name)
        {
            //a unique index isn't exactly the same as a unique constraint (except in MySql)
            var index = new DatabaseIndex
            {
                Name = "UI_" + name,
                IsUnique = true
            };
            index.Columns.Add(column);
            return index;
        }

        public static DatabaseConstraint CreateForeignKey(DatabaseTable databaseTable)
        {
            var constraint = new DatabaseConstraint
            {
                Name = "FK_" + databaseTable.Name,
                ConstraintType = ConstraintType.ForeignKey,
                RefersToTable = databaseTable.Name
            };
            constraint.Columns.Add("Parent");
            return constraint;
        }
    }
}