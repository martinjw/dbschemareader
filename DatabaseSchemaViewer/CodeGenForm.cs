using System;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using DatabaseSchemaReader.Conversion;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaViewer
{
    public partial class CodeGenForm : Form
    {
        private readonly DatabaseSchema _databaseSchema;

        public CodeGenForm(DatabaseSchema databaseSchema)
        {
            _databaseSchema = databaseSchema;
            InitializeComponent();

            if (txtFilePath.Text == string.Empty)
                txtFilePath.Text = Environment.CurrentDirectory;

            cmbDialect.DataSource = Enum.GetValues(typeof(SqlType));

            if (databaseSchema.Tables.Count == 0)
            {
                radSprocs.Visible = false;
            }
            cmbTables.DisplayMember = "Name";
            cmbTables.DataSource = databaseSchema.Tables;
            RadioCheckedChanged(this, EventArgs.Empty);

            errorProvider1.SetError(txtFilePath, string.Empty);
        }


        private void CodeGenFormLoad(object sender, EventArgs e)
        {
            var sqlType = ProviderToSqlType.Convert(_databaseSchema.Provider);
            if (sqlType.HasValue)
                cmbDialect.SelectedItem = sqlType;
        }

        private void GenerateClick(object sender, EventArgs e)
        {
            if (!ValidateChildren()) return;

            StartWaiting();

            var directory = new DirectoryInfo(txtFilePath.Text.Trim());
            var ns = txtNamespace.Text.Trim();
            var dialect = (SqlType)cmbDialect.SelectedItem;

            if (radCSharp.Checked)
            {
                //this launches in background worker
                RunCodeWriter(directory, ns, chkReadSprocs.Checked);
                return;
            }
            if (radDdl.Checked)
            {
                RunTableDdl(directory, dialect);
            }
            else if (radSprocs.Checked)
            {
                var table = (DatabaseTable)cmbTables.SelectedItem;
                RunSprocs(directory, dialect, table);
            }
            else if (radData.Checked)
            {
                var table = (DatabaseTable)cmbTables.SelectedItem;
                RunData(directory, dialect, table);
            }
            StopWaiting();
        }

        private void RunData(DirectoryInfo directory, SqlType dialect, DatabaseTable table)
        {
            var runner = new TaskRunner(_databaseSchema);
            if (runner.RunData(directory, dialect, table))
            {
                toolStripStatusLabel1.Text = runner.Message;
            }
            else
            {
                MessageBox.Show(
                    runner.Message,
                    @"Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1);
            }
        }

        private void RunSprocs(DirectoryInfo directory, SqlType dialect, DatabaseTable table)
        {
            var runner = new TaskRunner(_databaseSchema);
            if (runner.RunSprocs(directory, dialect, table))
            {
                toolStripStatusLabel1.Text = runner.Message;
            }
            else
            {
                MessageBox.Show(
                    runner.Message,
                    @"Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1);
            }

        }


        private void RunTableDdl(DirectoryInfo directory, SqlType dialect)
        {
            var runner = new TaskRunner(_databaseSchema);
            if (runner.RunTableDdl(directory, dialect))
            {
                toolStripStatusLabel1.Text = runner.Message;
            }
            else
            {
                MessageBox.Show(
                    runner.Message,
                    @"Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1);
            }
        }


        private void RunCodeWriter(DirectoryInfo directory, string ns, bool readStoredProcedures)
        {
            var runner = new CodeWriterRunner(_databaseSchema, directory, ns, readStoredProcedures);
            if (readStoredProcedures)
            {
                toolStripStatusLabel1.Text = @"Reading stored procedures";
            }
            if (!backgroundWorker1.IsBusy)
            {
                backgroundWorker1.RunWorkerAsync(runner);
            }
        }

        private void StopWaiting()
        {
            btnGenerate.Visible = true;
            progressBar1.Visible = false;
            Cursor = Cursors.Default;
        }

        private void StartWaiting()
        {
            btnGenerate.Visible = false;
            progressBar1.Visible = true;
            Cursor = Cursors.WaitCursor;
            Refresh();
        }

        private void FilePathValidating(object sender, CancelEventArgs e)
        {
            var path = txtFilePath.Text.Trim();
            if (string.IsNullOrEmpty(path))
            {
                e.Cancel = true;
                errorProvider1.SetError(txtFilePath, "Must be entered");
                return;
            }
            if (!Directory.Exists(path))
            {
                e.Cancel = true;
                errorProvider1.SetError(txtFilePath, "Directory does not exist");
                return;
            }
            errorProvider1.SetError(txtFilePath, string.Empty);
        }

        private void NamespaceValidating(object sender, CancelEventArgs e)
        {
            var ns = txtNamespace.Text.Trim();
            //a simplistic regex just to exclude weird punctuation/spaces etc
            if (Regex.IsMatch(ns, @"^([\w]+)([\w\.])$"))
            {
                errorProvider1.SetError(txtNamespace, string.Empty);
            }
            else
            {
                e.Cancel = true;
                errorProvider1.SetError(txtNamespace, "Invalid namespace");
            }
        }

        private void DialectValidating(object sender, CancelEventArgs e)
        {
            var dialect = (SqlType)cmbDialect.SelectedItem;

            //SQLite and SqlServerCE do not have stored procedures
            var nok = ((dialect == SqlType.SQLite || dialect == SqlType.SqlServerCe)
                && radSprocs.Checked);
            if (nok)
            {
                e.Cancel = true;
                errorProvider1.SetError(cmbDialect, "SQLite and SqlServerCe have no stored procedures");
            }
            else
            {
                errorProvider1.SetError(cmbDialect, string.Empty);
            }

        }

        private void FolderPickerClick(object sender, EventArgs e)
        {
            using (var picker = new FolderBrowserDialog())
            {
                picker.Description = @"Select the directory to write the files to.";
                //set to MyDocuments
                picker.RootFolder = Environment.SpecialFolder.MyComputer;
                picker.SelectedPath = txtFilePath.Text;

                var result = picker.ShowDialog();
                if (result == DialogResult.OK)
                {
                    txtFilePath.Text = picker.SelectedPath;
                }
            }
        }

        private void RadioCheckedChanged(object sender, EventArgs e)
        {
            if (radCSharp.Checked)
            {
                cmbDialect.Visible = false;
                labDialect.Visible = false;
                txtNamespace.Visible = true;
                labNamespace.Visible = true;
                panelTables.Visible = false;
                chkReadSprocs.Visible = true;
            }
            else
            {
                cmbDialect.Visible = true;
                labDialect.Visible = true;
                panelTables.Visible = (radSprocs.Checked || radData.Checked);
                chkReadSprocs.Visible = false;

                txtNamespace.Visible = false;
                labNamespace.Visible = false;
                if (txtNamespace.Text.Trim() == string.Empty)
                    txtNamespace.Text = @"Domain";
            }
        }


        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            var runner = (CodeWriterRunner)e.Argument;
            runner.RunCodeWriter();
            e.Result = runner;
        }

        private void BackgroundWorker1RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var runner = (CodeWriterRunner)e.Result;
            if (runner.Result)
            {
                toolStripStatusLabel1.Text = runner.Message;
            }
            else
            {
                MessageBox.Show(
                    runner.Message,
                    @"Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1);
            }
            StopWaiting();
        }

        private void CodeGenFormFormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.Cancel)
            {
                e.Cancel = false;
                return;
            }
            Properties.Settings.Default.Save();
        }
    }
}
