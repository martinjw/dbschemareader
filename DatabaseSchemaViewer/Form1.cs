using System;
using System.ComponentModel;
using System.Configuration;
using System.Data.Common;
using System.Windows.Forms;
using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaViewer
{
    public partial class Form1 : Form
    {
        private DatabaseSchema _databaseSchema;

        public Form1()
        {
            InitializeComponent();

            var dt = DbProviderFactories.GetFactoryClasses();
            const string invariantname = "InvariantName";
            dt.DefaultView.Sort = invariantname;
            DataProviders.DataSource = dt;
            DataProviders.DisplayMember = invariantname;
            DataProviders.ValueMember = invariantname;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DataProviders.SelectedValue = Properties.Settings.Default.Provider;
            //add sample connection strings
            contextMenuStrip1.Items.Clear();
            contextMenuStrip1.Items.Add("Firebird (Employee)").Click += (s, ev) => FillConnectionString( @"User=SYSDBA;Password=masterkey;Database=C:\Program Files\Firebird\Firebird_2_1\examples\empbuild\EMPLOYEE.FDB;Server=localhost; Connection lifetime=15;Pooling=true");
            contextMenuStrip1.Items.Add("MySQL (Northwind)").Click += (s, ev) => FillConnectionString( @"Server=localhost;Uid=root;Pwd=mysql;Database=Northwind;Allow User Variables=True;");
            contextMenuStrip1.Items.Add("MySQL (Sakila)").Click += (s, ev) => FillConnectionString(@"Server=localhost;Uid=root;Pwd=mysql;Database=Sakila;Allow User Variables=True;", @"Sakila");
            contextMenuStrip1.Items.Add("Oracle XE - TNS (HR)").Click += (s, ev) => FillConnectionString( @"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SID=XE)));User Id=HR;Password=HR;", @"HR");
            contextMenuStrip1.Items.Add("Oracle XE (HR)").Click += (s, ev) => FillConnectionString(
                @"Data Source=XE;User Id=HR;Password=HR;", @"HR");
            contextMenuStrip1.Items.Add("Postgresql (world)").Click += (s, ev) => FillConnectionString( @"Server=127.0.0.1;User id=postgres;password=sql;database=world;");
            contextMenuStrip1.Items.Add("SQLite (Northwind)").Click += (s, ev) => FillConnectionString( @"Data Source=C:\Data\northwind.db;");
            contextMenuStrip1.Items.Add("SQLite (Chinook)").Click += (s, ev) => FillConnectionString( @"Data Source=C:\Data\Chinook_Sqlite.sqlite;");
            contextMenuStrip1.Items.Add("Sql Server Express (AdventureWorks)").Click += (s, ev) => FillConnectionString(
                @"Data Source=.\SQLEXPRESS;Integrated Security=true;Initial Catalog=AdventureWorks");
            contextMenuStrip1.Items.Add("Sql Server Express (Northwind)").Click += (s, ev) => FillConnectionString( @"Data Source=.\SQLEXPRESS;Integrated Security=true;Initial Catalog=Northwind");
            contextMenuStrip1.Items.Add("SqlServerCe (Chinook)").Click += (s, ev) => FillConnectionString( @"Data Source=""C:\Data\Chinook_SqlServerCompact.sdf"";");
            //don't use the C:\Program Files\Microsoft SQL Server Compact Edition\v4.0\Samples version as you need admin rights
            contextMenuStrip1.Items.Add("SqlServerCe (Northwind)").Click += (s, ev) => FillConnectionString( @"Data Source=""C:\Data\northwind.sdf"";");
        }

        private void FillConnectionString(string connectionString)
        {
            FillConnectionString(connectionString, string.Empty);
        }
        private void FillConnectionString(string connectionString, string schema)
        {
            ConnectionString.Text = connectionString;
            SchemaOwner.Text = schema;
        }

        private bool IsConnectionStringValid()
        {
            string connectionString = ConnectionString.Text.Trim();
            if (string.IsNullOrEmpty(connectionString))
            {
                errorProvider1.SetError(ConnectionString, "Should not be empty");
                return false;
            }
            try
            {
                var factory = DbProviderFactories.GetFactory(DataProviders.SelectedValue.ToString());
                var csb = factory.CreateConnectionStringBuilder();
                csb.ConnectionString = connectionString;
            }
            catch (ArgumentException)
            {
                errorProvider1.SetError(ConnectionString, "Invalid connection string");
                return false;
            }
            catch (ConfigurationErrorsException)
            {
                errorProvider1.SetError(DataProviders, "This provider isn't available");
                return false;
            }

            errorProvider1.SetError(DataProviders, string.Empty);
            errorProvider1.SetError(ConnectionString, string.Empty);
            return true;
        }

        private void ReadSchemaClick(object sender, EventArgs e)
        {
            if (!IsConnectionStringValid()) return;

            var connectionString = ConnectionString.Text.Trim();
            var providerName = DataProviders.SelectedValue.ToString();

            StartWaiting();
            var rdr = new DatabaseReader(connectionString, providerName);
            var owner = SchemaOwner.Text.Trim();
            if (!string.IsNullOrEmpty(owner))
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
                _databaseSchema = e.Result as DatabaseSchema;
                if (_databaseSchema != null)
                {
                    SchemaToTreeview.PopulateTreeView(_databaseSchema, treeView1);
                    toolStripButton1.Enabled = true;
                    toolStripButton2.Enabled = true;
                }
            }
            StopWaiting();
        }

        private void CodeGenClick(object sender, EventArgs e)
        {
            if (_databaseSchema == null) return;

            using (var f = new CodeGenForm(_databaseSchema))
            {
                f.ShowDialog();
            }
        }

        private void CompareClick(object sender, EventArgs e)
        {
            if (_databaseSchema == null) return;

            using (var f = new CompareForm(_databaseSchema))
            {
                f.ShowDialog();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.Cancel)
            {
                e.Cancel = false;
                return;
            }
            Properties.Settings.Default.Provider = DataProviders.SelectedValue.ToString();

            Properties.Settings.Default.Save();
        }
    }
}
