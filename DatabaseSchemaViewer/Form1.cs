using System;
using System.ComponentModel;
using System.Data.Common;
using System.Windows.Forms;
using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaViewer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            var dt = DbProviderFactories.GetFactoryClasses();
            DataProviders.DataSource = dt;
            DataProviders.DisplayMember = "InvariantName";
            DataProviders.ValueMember = "InvariantName";
            DataProviders.SelectedValue = "System.Data.SqlClient";
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

        private void ReadSchemaClick(object sender, EventArgs e)
        {
            StartWaiting();
            var connectionString = ConnectionString.Text.Trim();
            var providerName = DataProviders.SelectedValue.ToString();
            var rdr = new DatabaseReader(connectionString, providerName);
            var owner = SchemaOwner.Text.Trim();
            if(!string.IsNullOrEmpty(owner))
                rdr.Owner = owner;

            backgroundWorker1.RunWorkerAsync(rdr);
        }

        private void StopWaiting()
        {
            ReadSchema.Visible = true;
            Cursor = Cursors.Default;
            treeView1.Visible = true;
            progressBar1.Visible = false;
        }

        private void StartWaiting()
        {
            Cursor = Cursors.WaitCursor;
            ReadSchema.Visible = false;
            progressBar1.Value = 0;
            progressBar1.Visible = true;
            treeView1.Visible = false;
            Update();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            var rdr = (DatabaseReader)e.Argument;
            e.Result = rdr.ReadAll();
        }

        private void BackgroundWorker1RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
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
                var schema = e.Result as DatabaseSchema;
                if (schema != null)
                {
                    SchemaToTreeview.PopulateTreeView(schema, treeView1);
                }
            }
            StopWaiting();
        }

    }
}
