using System;
using System.ComponentModel;
using System.Data.Common;
using System.IO;
using System.Windows.Forms;
using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;

namespace CopyToSQLite
{
    public partial class CopyForm : Form
    {
        private string _providerName;
        private string _filePath;

        public CopyForm()
        {
            InitializeComponent();

            var dt = DbProviderFactories.GetFactoryClasses();
            DataProviders.DataSource = dt;
            DataProviders.DisplayMember = "InvariantName";
            DataProviders.ValueMember = "InvariantName";
        }

        private void Form1Load(object sender, EventArgs e)
        {
            DataProviders.SelectedValue = Properties.Settings.Default.Provider;
        }

        private void ConnectionStringValidating(object sender, CancelEventArgs e)
        {
            string connectionString = ConnectionString.Text.Trim();
            if (string.IsNullOrEmpty(connectionString))
            {
                e.Cancel = true;
                errorProvider1.SetError(ConnectionString, "Should not be empty");
            }
            else
            {
                errorProvider1.SetError(ConnectionString, string.Empty);

            }
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
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                e.Cancel = true;
                errorProvider1.SetError(txtFilePath, "Directory does not exist");
                return;
            }
            if (File.Exists(path))
            {
                e.Cancel = true;
                errorProvider1.SetError(txtFilePath, "File already exists");
                return;
            }
            errorProvider1.SetError(txtFilePath, string.Empty);
        }

        private void ReadSchemaClick(object sender, EventArgs e)
        {
            StartWaiting();
            var connectionString = ConnectionString.Text.Trim();
            _providerName = DataProviders.SelectedValue.ToString();
            _filePath = txtFilePath.Text.Trim();
            var rdr = new DatabaseReader(connectionString, _providerName);
            var owner = SchemaOwner.Text.Trim();
            if (!string.IsNullOrEmpty(owner))
                rdr.Owner = owner;

            backgroundWorker1.RunWorkerAsync(rdr);
        }

        private void StopWaiting()
        {
            ReadSchema.Visible = true;
            Cursor = Cursors.Default;
            progressBar1.Visible = false;
        }

        private void StartWaiting()
        {
            Cursor = Cursors.WaitCursor;
            ReadSchema.Visible = false;
            progressBar1.Value = 0;
            progressBar1.Style = ProgressBarStyle.Marquee;
            progressBar1.Visible = true;
            Update();
        }

        private SqlType OriginSqlType()
        {
            if (_providerName.Equals("System.Data.SqlClient", StringComparison.OrdinalIgnoreCase))
                return SqlType.SqlServer;
            if (_providerName.Equals("System.Data.SQLite", StringComparison.OrdinalIgnoreCase))
                return SqlType.SqLite;
            if (_providerName.IndexOf("Oracle", StringComparison.OrdinalIgnoreCase) != -1)
                return SqlType.Oracle;
            if (_providerName.Equals("MySql.Data.MySqlClient", StringComparison.OrdinalIgnoreCase))
                return SqlType.MySql;
            //could be something we don't have a direct syntax for
            return SqlType.SqlServer;
        }

        private void BackgroundDoWork(object sender, DoWorkEventArgs e)
        {
            var rdr = (DatabaseReader)e.Argument;
            var runner = new Runner(rdr, _filePath, OriginSqlType());
            //pass thru the event to background worker
            runner.ProgressChanged += (s1, e1) => backgroundWorker1.ReportProgress(e1.ProgressPercentage, e1.UserState);
            var result = runner.Execute();
            e.Result = result;
        }

        private void BackgroundCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                //it errored
                errorProvider1.SetError(ConnectionString, e.Error.Message);
                toolStripStatusLabel1.Text = e.Error.Message;
            }
            else
            {
                //it worked
                var itWorked = (bool)e.Result;
                toolStripStatusLabel1.Text = itWorked ? "Database created" : "Something went wrong";
            }
            StopWaiting();
        }

        private void BackgroundProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            toolStripStatusLabel1.Text = e.UserState.ToString();
            var percentage = e.ProgressPercentage;
            if (percentage > 0)
            {
                progressBar1.Style = ProgressBarStyle.Continuous;
                progressBar1.Minimum = 0;
                progressBar1.Maximum = 100;
                progressBar1.Value = e.ProgressPercentage;
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

        private void CopyFormFormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = false; //even if there's a validation message, we will close
            Properties.Settings.Default.Provider = DataProviders.SelectedValue.ToString();
            Properties.Settings.Default.Save();
        }

    }
}
