using System.Data;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Conversion.KeyMaps
{
    internal class ConstraintKeyMap
    {
        public ConstraintKeyMap(DataTable dt, ConstraintType constraintType)
        {
            //all same, my custom sql
            Key = "CONSTRAINT_NAME";
            SchemaKey = "CONSTRAINT_SCHEMA";
            TableKey = "TABLE_NAME";
            ColumnKey = "COLUMN_NAME";
            OrdinalKey = "ORDINAL_POSITION";
            RefersToKey = "UNIQUE_CONSTRAINT_NAME";
            RefersToTableKey = "FK_TABLE";
            RefersToSchemaKey = "FK_SCHEMA";
            ExpressionKey = "EXPRESSION";
            DeleteRuleKey = "DELETE_RULE";
            UpdateRuleKey = "UPDATE_RULE";
            //oracle
            if (!dt.Columns.Contains(Key)) Key = "FOREIGN_KEY_CONSTRAINT_NAME";
            if (!dt.Columns.Contains(TableKey)) TableKey = "FOREIGN_KEY_TABLE_NAME";
            if (!dt.Columns.Contains(RefersToTableKey)) RefersToTableKey = "PRIMARY_KEY_TABLE_NAME";
            if (!dt.Columns.Contains(RefersToKey)) RefersToKey = "PRIMARY_KEY_CONSTRAINT_NAME";
            //devart.data.postgresql
            if (!dt.Columns.Contains(Key)) Key = "NAME";
            if (!dt.Columns.Contains(TableKey)) TableKey = "TABLE";

            //firebird
            CheckFirebird(dt, constraintType);
            //sqlite
            CheckSqLite(dt);

            //db2
            CheckDb2(dt, constraintType);

            //oledb
            if (!dt.Columns.Contains(Key)) Key = "FK_NAME";
            if (!dt.Columns.Contains(OrdinalKey)) OrdinalKey = "ORDINAL";
            if (!dt.Columns.Contains(TableKey)) TableKey = "FK_TABLE_NAME";
            if (!dt.Columns.Contains(ColumnKey)) ColumnKey = "FK_COLUMN_NAME";
            if (!dt.Columns.Contains(RefersToTableKey)) RefersToTableKey = "PK_TABLE_NAME";
            if (!dt.Columns.Contains(RefersToKey)) RefersToKey = "PK_NAME";

            //oldedb schema. Thanks to safepage.
            if (!dt.Columns.Contains(SchemaKey)) SchemaKey = "TABLE_SCHEMA";
            if (!dt.Columns.Contains(SchemaKey)) SchemaKey = "PK_TABLE_SCHEMA";

            if (!dt.Columns.Contains(SchemaKey)) SchemaKey = null;
            if (!dt.Columns.Contains(RefersToKey)) RefersToKey = null;
            if (!dt.Columns.Contains(RefersToTableKey)) RefersToTableKey = null;
            if (!dt.Columns.Contains(DeleteRuleKey)) DeleteRuleKey = null;
            if (!dt.Columns.Contains(UpdateRuleKey)) UpdateRuleKey = null;
            //not present if separate foreign key columns
            if (!dt.Columns.Contains(ColumnKey)) ColumnKey = null;
            if (!dt.Columns.Contains(OrdinalKey)) OrdinalKey = null;
            if (!dt.Columns.Contains(ExpressionKey)) ExpressionKey = null;
            if (!dt.Columns.Contains(Key)) Key = null;
            if (!dt.Columns.Contains(RefersToSchemaKey)) RefersToSchemaKey = null;
        }

        private void CheckSqLite(DataTable dt)
        {
            if (!dt.Columns.Contains(ColumnKey)) ColumnKey = "FKEY_FROM_COLUMN";
            if (!dt.Columns.Contains(OrdinalKey)) OrdinalKey = "FKEY_FROM_ORDINAL_POSITION";
            if (!dt.Columns.Contains(RefersToTableKey)) RefersToTableKey = "FKEY_TO_TABLE";
        }

        private void CheckDb2(DataTable dt, ConstraintType constraintType)
        {
            if (constraintType == ConstraintType.PrimaryKey && !dt.Columns.Contains(Key)) Key = "PK_NAME";
            if (constraintType == ConstraintType.ForeignKey && !dt.Columns.Contains(Key)) Key = "FK_NAME";
            if (!dt.Columns.Contains(TableKey)) TableKey = "FKTABLE_NAME";
            if (!dt.Columns.Contains(RefersToTableKey)) RefersToTableKey = "PKTABLE_NAME";
            if (!dt.Columns.Contains(RefersToKey)) RefersToKey = "PK_NAME";
            if (!dt.Columns.Contains(ColumnKey)) ColumnKey = "FKCOLUMN_NAME";
            if (!dt.Columns.Contains(OrdinalKey)) OrdinalKey = "KEY_SEQ";
            if (!dt.Columns.Contains(ExpressionKey)) ExpressionKey = "CHECK_CLAUSE";
        }

        private void CheckFirebird(DataTable dt, ConstraintType constraintType)
        {
            if (constraintType == ConstraintType.PrimaryKey && !dt.Columns.Contains(Key)) Key = "PK_NAME";
            if (constraintType == ConstraintType.ForeignKey && !dt.Columns.Contains(Key)) Key = "UK_NAME";
            if (!dt.Columns.Contains(RefersToTableKey)) RefersToTableKey = "REFERENCED_TABLE_NAME";
            //a firebird typo!
            if (dt.Columns.Contains("CHECK_CLAUSULE")) ExpressionKey = "CHECK_CLAUSULE";
        }

        public string RefersToTableKey { get; private set; }

        public string RefersToSchemaKey { get; private set; }

        public string RefersToKey { get; private set; }

        public string ExpressionKey { get; private set; }

        public string UpdateRuleKey { get; private set; }

        public string ColumnKey { get; private set; }

        public string DeleteRuleKey { get; private set; }

        public string OrdinalKey { get; private set; }

        public string TableKey { get; private set; }

        public string SchemaKey { get; private set; }

        public string Key { get; private set; }
    }
}