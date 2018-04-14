using System;
using System.ComponentModel;
using System.Data.Common;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaViewer
{
    public partial class CompareForm : Form
    {
        private DatabaseSchema _compareSchema;
        private readonly DatabaseSchema _databaseSchema;
        private bool _inverseComparison;

        public CompareForm(DatabaseSchema databaseSchema)
        {
            _databaseSchema = databaseSchema;

            InitializeComponent();
        }

        private void CompareFormLoad(object sender, EventArgs e)
        {
            labOriginDatabase.Text = _databaseSchema.ConnectionString;
            ConnectionString.Text = _databaseSchema.ConnectionString;
            SchemaOwner.Text = _databaseSchema.Owner;

            contextMenuStrip1.Items.Clear();
            contextMenuStrip1.Items.Add(_databaseSchema.ConnectionString).Click += (s, ev) => ConnectionString.Text = _databaseSchema.ConnectionString;
            AllowDrop = true;
            DragEnter += FormDragEnter;
            DragDrop += FormDragDrop;
        }


        private void FormDragDrop(object sender, DragEventArgs e)
        {
            //Windows7 User Interface Privilege Isolation stops this if RunAs Admin
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files == null || files.Length != 1)
            {
                var cs = e.Data.GetData(DataFormats.UnicodeText) as string;
                if (!string.IsNullOrEmpty(cs))
                    ConnectionString.Text = cs.Replace("\n", "").Replace("\r", "");
                return;
            }
            var file = files[0];
            var type = Path.GetExtension(file);
            if (string.IsNullOrEmpty(type)) return;
            switch (type.ToUpperInvariant())
            {
                case ".MDF": //local Sql Express database
                    ConnectionString.Text = @"Server=.\SQLExpress;AttachDbFilename=" + file + ";User Instance=true;Integrated Security=true;";
                    break;
                case ".SDF": //SqlServer Ce
                    ConnectionString.Text = "Data Source=" + file;
                    break;
                case ".DB":
                case ".SQLITE": //SQLite (could be other extensions)
                    ConnectionString.Text = "Data Source=" + file;
                    break;
                case ".FDB": //Firebird
                    ConnectionString.Text = "User=SYSDBA;Password=masterkey;Database="
                         + file +
                         ";Server=localhost; Connection lifetime=15;Pooling=true";
                    break;
            }
        }

        private void FormDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            if (e.Data.GetDataPresent(DataFormats.UnicodeText))
                e.Effect = DragDropEffects.Copy;
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
            _inverseComparison = chkInverse.Checked;
            if (UseSavedSchema.Checked && _compareSchema != null)
            {
                //the compare schema is already loaded
                RunCompare();
                StopWaiting();
                return;
            }
            var providerName = _databaseSchema.Provider;
            var rdr = new DatabaseReader(connectionString, providerName);
            var owner = SchemaOwner.Text.Trim();
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
            var first = _databaseSchema;
            var second = _compareSchema;
            if (_inverseComparison)
            {
                first = _compareSchema;
                second = _databaseSchema;
            }
            if (runner.RunCompare(first, second))
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

        private void OpenSavedClick(object sender, EventArgs e)
        {
            using (var picker = new OpenFileDialog())
            {
                picker.DefaultExt = ".dbschema";
                picker.Title = "Open saved schema.";
                picker.InitialDirectory =
                            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var result = picker.ShowDialog();
                if (result == DialogResult.OK)
                {
                    UseSavedSchema.Text = "Use " + Path.GetFileName(picker.FileName);
                    UseSavedSchema.Visible = true;
                    UseSavedSchema.Checked = true;
                    StartWaiting();
                    using (var stream = picker.OpenFile())
                    {
                        var f = new BinaryFormatter();
                        try
                        {
                            _compareSchema = f.Deserialize(stream) as DatabaseSchema;
                        }
                        catch (SerializationException)
                        {
                            toolStripStatusLabel1.Text = "Invalid serialization format";
                        }
                    }
                    if (_compareSchema != null)
                    {
                        ConnectionString.Text = _databaseSchema.ConnectionString;
                        SchemaOwner.Text = _databaseSchema.Owner;
                        RunCompare();
                    }
                }
            }
            StopWaiting();
        }
    }
}
