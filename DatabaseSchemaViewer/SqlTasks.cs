using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DatabaseSchemaReader;
using DatabaseSchemaReader.CodeGen;
using DatabaseSchemaReader.CodeGen.Procedures;
using DatabaseSchemaReader.Data;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using DatabaseSchemaReader.Utilities;

namespace DatabaseSchemaViewer
{
    /// <summary>
    /// Writes DDL and DML sql to the clipboard. Used by the treeview.
    /// </summary>
    class SqlTasks
    {
        private readonly IMigrationGenerator _migrationGenerator;
        private readonly SqlType _sqlType;

        public SqlTasks(SqlType sqlType, bool escapeNames)
        {
            _sqlType = sqlType;
            _migrationGenerator = new DdlGeneratorFactory(sqlType).MigrationGenerator();
            EscapeNames = _migrationGenerator.EscapeNames = escapeNames;
        }

        public bool EscapeNames { get; set; }

        public void BuildAllTableDdl(DatabaseSchema databaseSchema)
        {
            var tg = new DdlGeneratorFactory(_sqlType).AllTablesGenerator(databaseSchema);
            tg.IncludeSchema = false;
            tg.EscapeNames = EscapeNames;
            try
            {
                var txt = tg.Write();
                Clipboard.SetText(txt, TextDataFormat.UnicodeText);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }

        public void DeleteAllData(DatabaseSchema databaseSchema)
        {
            try
            {
                var orderedTables = SchemaTablesSorter.TopologicalSort(databaseSchema).Reverse();
                var sb = new StringBuilder();
                sb.AppendLine("-- delete data from all tables in safe order");
                foreach (var databaseTable in orderedTables)
                {
                    if (databaseTable.ForeignKeyChildren.Contains(databaseTable))
                    {
                        sb.AppendLine("-- WARNING: " + databaseTable.Name + " has SELF-JOIN");
                    }
                    var sqlWriter = new SqlWriter(databaseTable, _sqlType);
                    sb.AppendLine("DELETE FROM " + (EscapeNames ? sqlWriter.EscapedTableName : databaseTable.Name));
                }
                Clipboard.SetText(sb.ToString(), TextDataFormat.UnicodeText);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }

        public void GetData(DatabaseTable databaseTable, string connectionString, string providerName)
        {
            try
            {
                var sw = new ScriptWriter();
                sw.IncludeBlobs = false;
                sw.IncludeIdentity = true;
                sw.PageSize = 100;
                sw.EscapeNames = EscapeNames;
                var txt = sw.ReadTable(databaseTable, connectionString, providerName);
                Clipboard.SetText(txt, TextDataFormat.UnicodeText);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }

        public void BuildTableDdl(DatabaseTable databaseTable)
        {
            var tg = new DdlGeneratorFactory(_sqlType).TableGenerator(databaseTable);
            tg.IncludeSchema = false;
            tg.EscapeNames = EscapeNames;
            try
            {
                var txt = tg.Write();
                Clipboard.SetText(txt, TextDataFormat.UnicodeText);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }

        public void BuildDropTable(DatabaseTable table)
        {
            try
            {
                var txt = _migrationGenerator.DropTable(table);
                Clipboard.SetText(txt, TextDataFormat.UnicodeText);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }

        private SqlWriter BuildSqlWriter(DatabaseTable databaseTable)
        {
            var sqlWriter = new SqlWriter(databaseTable, _sqlType);
            sqlWriter.EscapeNames = EscapeNames;
            return sqlWriter;
        }

        public void BuildTableSelect(DatabaseTable databaseTable)
        {
            var sqlWriter = BuildSqlWriter(databaseTable);
            try
            {
                var txt = sqlWriter.SelectAllSql();
                Clipboard.SetText(txt, TextDataFormat.UnicodeText);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }


        public void BuildTableSelectPaged(DatabaseTable databaseTable)
        {
            var sqlWriter = BuildSqlWriter(databaseTable);
            try
            {
                var txt = sqlWriter.SelectPageSql();
                Clipboard.SetText(txt, TextDataFormat.UnicodeText);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }
        public void BuildTableInsert(DatabaseTable databaseTable)
        {
            var sqlWriter = BuildSqlWriter(databaseTable);
            try
            {
                var txt = sqlWriter.InsertSqlWithoutOutputParameter();
                Clipboard.SetText(txt, TextDataFormat.UnicodeText);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }
        public void BuildTableUpdate(DatabaseTable databaseTable)
        {
            var sqlWriter = BuildSqlWriter(databaseTable);
            try
            {
                var txt = sqlWriter.UpdateSql();
                Clipboard.SetText(txt, TextDataFormat.UnicodeText);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }

        public void BuildView(DatabaseView view)
        {
            try
            {
                var txt = _migrationGenerator.AddView(view);
                Clipboard.SetText(txt, TextDataFormat.UnicodeText);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }

        public void BuildPackage(DatabasePackage package)
        {
            try
            {
                var txt = _migrationGenerator.AddPackage(package);
                Clipboard.SetText(txt, TextDataFormat.UnicodeText);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }

        public void BuildProcedure(DatabaseStoredProcedure storedProcedure)
        {
            try
            {
                var txt = _migrationGenerator.AddProcedure(storedProcedure);
                Clipboard.SetText(txt, TextDataFormat.UnicodeText);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }

        public void BuildFunction(DatabaseFunction databaseFunction)
        {
            try
            {
                var txt = _migrationGenerator.AddFunction(databaseFunction);
                Clipboard.SetText(txt, TextDataFormat.UnicodeText);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }

        public void BuildAddColumn(DatabaseColumn column)
        {
            try
            {
                var txt = _migrationGenerator.AddColumn(column.Table, column);
                Clipboard.SetText(txt, TextDataFormat.UnicodeText);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }

        public void BuildAlterColumn(DatabaseColumn column)
        {
            try
            {
                var txt = _migrationGenerator.AlterColumn(column.Table, column, null);
                Clipboard.SetText(txt, TextDataFormat.UnicodeText);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }

        public void BuildAddConstraint(DatabaseTable databaseTable, DatabaseConstraint constraint)
        {
            try
            {
                var txt = _migrationGenerator.AddConstraint(databaseTable, constraint);
                Clipboard.SetText(txt, TextDataFormat.UnicodeText);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }


        public void BuildDropConstraint(DatabaseTable databaseTable, DatabaseConstraint constraint)
        {
            try
            {
                var txt = _migrationGenerator.DropConstraint(databaseTable, constraint);
                Clipboard.SetText(txt, TextDataFormat.UnicodeText);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }

        public void BuildAddTrigger(DatabaseTable databaseTable, DatabaseTrigger trigger)
        {
            try
            {
                var txt = _migrationGenerator.AddTrigger(databaseTable, trigger);
                Clipboard.SetText(txt, TextDataFormat.UnicodeText);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }

        public void BuildDropTrigger(DatabaseTrigger trigger)
        {
            try
            {
                var txt = _migrationGenerator.DropTrigger(trigger);
                Clipboard.SetText(txt, TextDataFormat.UnicodeText);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }

        public void BuildAddIndex(DatabaseTable databaseTable, DatabaseIndex index)
        {
            try
            {
                var txt = _migrationGenerator.AddIndex(databaseTable, index);
                Clipboard.SetText(txt, TextDataFormat.UnicodeText);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }

        public void BuildDropIndex(DatabaseTable databaseTable, DatabaseIndex index)
        {
            try
            {
                var txt = _migrationGenerator.DropIndex(databaseTable, index);
                Clipboard.SetText(txt, TextDataFormat.UnicodeText);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }

        public void BuildClass(DatabaseTable databaseTable)
        {
            try
            {
                var cw = new ClassWriter(databaseTable, new CodeWriterSettings());
                var txt = cw.Write();
                Clipboard.SetText(txt, TextDataFormat.UnicodeText);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
        }

        public void BuildProcedureCode(DatabaseSchema databaseSchema, DatabaseStoredProcedure databaseStoredProcedure)
        {
            try
            {
                //grab the data
                if (databaseStoredProcedure.ResultSets.Count == 0)
                {
                    //Delete sprocs won't have resultsets, so will get called multiple times
                    var sprocRunner = new DatabaseSchemaReader.Procedures.ResultSetReader(databaseSchema);
                    sprocRunner.ExecuteProcedure(databaseStoredProcedure);
                }

                //write it
                var sprocWriter = new ProcedureWriter(databaseStoredProcedure, "Domain");
                var txt = sprocWriter.Write();

                Clipboard.SetText(txt, TextDataFormat.UnicodeText);
            }
            catch (Exception exception)
            {
                Clipboard.SetText("//sorry, not available - " + exception.Message,
                    TextDataFormat.UnicodeText);
                Debug.WriteLine(exception.Message);
            }
        }
    }
}
