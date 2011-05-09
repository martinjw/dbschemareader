using System;
using System.ComponentModel;
using System.Data.Common;
using System.Windows.Forms;
using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaViewer
{
    public partial class CompareForm : Form
    {
        private DatabaseSchema _compareSchema;
        private readonly DatabaseSchema _databaseSchema;

        public CompareForm(DatabaseSchema databaseSchema)
        {
            _databaseSchema = databaseSchema;

            InitializeComponent();
        }

        private void CompareFormLoad(object sender, EventArgs e)
        {
            labOriginDatabase.Text = _databaseSchema.ConnectionString;

            contextMenuStrip1.Items.Clear();
            contextMenuStrip1.Items.Add(_databaseSchema.ConnectionString).Click += (s, ev) => ConnectionString.Text = _databaseSchema.ConnectionString;
        }


        private void ConnectionStringValidating(object sender, CancelEventArgs e)
        {
            string connectionString = ConnectionString.Text.Trim();
            if (string.IsNullOrEmpty(connectionString))
            {
                e.Cancel = true;
                errorProvider1.SetError(ConnectionString, "Should not be empty");
                return;
            }
            try
            {
                var factory = DbProviderFactories.GetFactory(_databaseSchema.Provider);
                var csb = factory.CreateConnectionStringBuilder();
                csb.ConnectionString = connectionString;
            }
            catch (ArgumentException)
            {
                e.Cancel = true;
                errorProvider1.SetError(ConnectionString, "Invalid connection string");
                return;
            }

            errorProvider1.SetError(ConnectionString, string.Empty);
        }


        private void CompareSchemaClick(object sender, EventArgs e)
        {
            StartWaiting();
            var connectionString = ConnectionString.Text.Trim();
            var providerName = _databaseSchema.Provider;
            var rdr = new DatabaseReader(connectionString, providerName);
            var owner = _databaseSchema.Owner;
            if (!string.IsNullOrEmpty(owner))
                rdr.Owner = owner;

            backgroundWorker1.RunWorkerAsync(rdr);
        }

        private void BackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            var rdr = (DatabaseReader)e.Argument;
            e.Result = rdr.ReadAll();
        }

        private void BackgroundWorker1RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            StopWaiting();
            if (e.Error != null)
            {
                //it errored
                errorProvider1.SetError(ConnectionString, e.Error.Message);
                toolStripStatusLabel1.Text = e.Error.Message;
            }
            else
            {
                //it worked
                _compareSchema = e.Result as DatabaseSchema;
                if (_compareSchema != null)
                {
                    RunCompare();
                }
            }
        }

        private void RunCompare()
        {
            var runner = new CompareRunner();
            if (runner.RunCompare(_databaseSchema, _compareSchema))
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

        private void StopWaiting()
        {
            CompareSchema.Visible = true;
            Cursor = Cursors.Default;
            progressBar1.Visible = false;
        }

        private void StartWaiting()
        {
            Cursor = Cursors.WaitCursor;
            CompareSchema.Visible = false;
            progressBar1.Value = 0;
            progressBar1.Visible = true;
            Update();
        }

        private void CompareForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.Cancel)
            {
                e.Cancel = false;
                return;
            }
        }
    }
}
