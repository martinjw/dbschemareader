using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.IO;
using System.Windows.Forms;
using DatabaseSchemaReader;
using DatabaseSchemaReader.Conversion;
using DatabaseSchemaReader.DataSchema;

namespace CopyToSQLite
{
    public partial class CopyForm : Form
    {
        private string _providerName;
        private string _filePath;
        private readonly List<string> _installedProviders = new List<string>();

        public CopyForm()
        {
            InitializeComponent();

            var dt = DbProviderFactories.GetFactoryClasses();
            const string invariantname = "InvariantName";
            foreach (System.Data.DataRow row in dt.Rows)
            {
                _installedProviders.Add((string)row[invariantname]);
            }
            _installedProviders.Sort();
            DataProviders.DataSource = _installedProviders;
            if (_installedProviders.Contains("System.Data.SqlServerCe.4.0"))
            {
                WarningLabel.Text = WarningLabel.Text.Replace("SQLite", "SQLite or SqlServer CE 4");
                WriteToFileLabel.Text = WriteToFileLabel.Text.Replace("SQLite", "SQLite or SqlServer CE 4");
                TargetGroupBox.Visible = true;
            }
        }

        private void Form1Load(object sender, EventArgs e)
        {
            var selectedValue = Properties.Settings.Default.Provider;
            //the last used provider has been uninstalled
            if (!_installedProviders.Contains(selectedValue)) selectedValue = "System.Data.SqlClient";
            DataProviders.SelectedItem = selectedValue;

            //add sample connection strings
            contextMenuStrip1.Items.Clear();
            if (_installedProviders.Contains("IBM.Data.DB2"))
            {
                contextMenuStrip1.Items.Add("DB2 (Sample)").Click += (s, ev) => FillConnectionString(@"Server=localhost:50000;UID=db2admin;pwd=db2;Database=Sample");
            }
            if (_installedProviders.Contains("FirebirdSql.Data.FirebirdClient"))
            {
                contextMenuStrip1.Items.Add("Firebird (Employee)").Click += (s, ev) => FillConnectionString(@"User=SYSDBA;Password=masterkey;Database=C:\Program Files\Firebird\Firebird_2_1\examples\empbuild\EMPLOYEE.FDB;Server=localhost; Connection lifetime=15;Pooling=true");
            }
            if (_installedProviders.Contains("MySql.Data.MySqlClient") || _installedProviders.Contains("Devart.Data.MySql"))
            {
                contextMenuStrip1.Items.Add("MySQL (Northwind)").Click += (s, ev) => FillConnectionString(@"Server=localhost;Uid=root;Pwd=mysql;Database=Northwind;Allow User Variables=True;");
                contextMenuStrip1.Items.Add("MySQL (Sakila)").Click += (s, ev) => FillConnectionString(@"Server=localhost;Uid=root;Pwd=mysql;Database=Sakila;Allow User Variables=True;", @"Sakila");
            }
            contextMenuStrip1.Items.Add("Oracle XE - TNS (HR)").Click += (s, ev) => FillConnectionString(@"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SID=XE)));User Id=hr;Password=hr;", @"HR");
            contextMenuStrip1.Items.Add("Oracle XE (HR)").Click += (s, ev) => FillConnectionString(
                @"Data Source=XE;User Id=hr;Password=hr;", @"HR");
            if (_installedProviders.Contains("Devart.Data.Oracle"))
            {
                contextMenuStrip1.Items.Add("Oracle XE - devart direct mode (HR)").Click += (s, ev) => FillConnectionString(
                    @"Data Source=localhost;SID=XE;direct=true;User Id=hr;Password=hr;", @"HR");
            }
            if (_installedProviders.Contains("Npgsql") || _installedProviders.Contains("Devart.Data.Devart.Data.PostgreSql"))
            {
                contextMenuStrip1.Items.Add("Postgresql (world)").Click += (s, ev) => FillConnectionString(@"Server=127.0.0.1;User id=postgres;password=sql;database=world;");
            }
            if (_installedProviders.Contains("System.Data.SQLite") || _installedProviders.Contains("Devart.Data.SQLite"))
            {
                contextMenuStrip1.Items.Add("SQLite (Northwind)").Click += (s, ev) => FillConnectionString(@"Data Source=C:\Data\northwind.db;");
                contextMenuStrip1.Items.Add("SQLite (Chinook)").Click += (s, ev) => FillConnectionString(@"Data Source=C:\Data\Chinook_Sqlite.sqlite;");
            }
            contextMenuStrip1.Items.Add("Sql Server Express (AdventureWorks)").Click += (s, ev) => FillConnectionString(
                @"Data Source=.\SQLEXPRESS;Integrated Security=true;Initial Catalog=AdventureWorks");
            contextMenuStrip1.Items.Add("Sql Server Express (Northwind)").Click += (s, ev) => FillConnectionString(@"Data Source=.\SQLEXPRESS;Integrated Security=true;Initial Catalog=Northwind");
            if (_installedProviders.Contains("System.Data.SqlServerCe.4.0"))
            {
                contextMenuStrip1.Items.Add("SqlServerCe (Chinook)").Click += (s, ev) => FillConnectionString(@"Data Source=""C:\Data\Chinook_SqlServerCompact.sdf"";");
                //don't use the C:\Program Files\Microsoft SQL Server Compact Edition\v4.0\Samples version as you need admin rights
                contextMenuStrip1.Items.Add("SqlServerCe (Northwind)").Click += (s, ev) => FillConnectionString(@"Data Source=""C:\Data\northwind.sdf"";");
            }
            AllowDrop = true;
            DragEnter += FormDragEnter;
            DragDrop += FormDragDrop;

            var path = txtFilePath.Text.Trim();
            if (string.IsNullOrEmpty(path))
            {
                txtFilePath.Text = @"C:\Temp\sql.db";
                return;
            }
            var dir = Path.GetDirectoryName(path);
            if (dir == null || !Directory.Exists(dir))
            {
                txtFilePath.Text = Path.Combine(Path.GetTempPath(), "sql.db");
                return;
            }
            if (File.Exists(path))
            {
                txtFilePath.Text = Path.Combine(dir, Path.GetFileNameWithoutExtension(path) + "_1" + Path.GetExtension(path));
            }
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
                    SelectProvider("System.Data.SqlClient");
                    SchemaOwner.Text = "dbo";
                    ReadSchemaClick(this, EventArgs.Empty);
                    break;
                case ".SDF": //SqlServer Ce
                    ConnectionString.Text = "Data Source=" + file;
                    SelectProvider("System.Data.SqlServerCe.4.0");
                    SchemaOwner.Text = "";
                    ReadSchemaClick(this, EventArgs.Empty);
                    break;
                case ".DB":
                case ".SQLITE": //SQLite (could be other extensions)
                    ConnectionString.Text = "Data Source=" + file;
                    var selectedValue = "System.Data.SQLite";
                    if (!_installedProviders.Contains(selectedValue)) selectedValue = "Devart.Data.SQLite";
                    SelectProvider(selectedValue);
                    SchemaOwner.Text = "";
                    ReadSchemaClick(this, EventArgs.Empty);
                    break;
                case ".FDB": //Firebird
                    ConnectionString.Text = "User=SYSDBA;Password=masterkey;Database="
                         + file +
                         ";Server=localhost; Connection lifetime=15;Pooling=true";
                    SelectProvider("FirebirdSql.Data.FirebirdClient");
                    SchemaOwner.Text = "";
                    break;
            }
        }

        private void SelectProvider(string provider)
        {
            if (!_installedProviders.Contains(provider)) provider = "System.Data.SqlClient";
            DataProviders.SelectedItem = provider;
        }

        private void FormDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            if (e.Data.GetDataPresent(DataFormats.UnicodeText))
                e.Effect = DragDropEffects.Copy;
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

        private void ConnectionStringValidating(object sender, CancelEventArgs e)
        {
            e.Cancel = !IsConnectionStringValid();
        }

        private bool IsConnectionStringValid()
        {
            string connectionString = ConnectionString.Text.Trim();
            if (string.IsNullOrEmpty(connectionString))
            {
                errorProvider1.SetError(ConnectionString, "Should not be empty");
                return false;
            }
            errorProvider1.SetError(ConnectionString, string.Empty);
            return true;
        }

        private void FilePathValidating(object sender, CancelEventArgs e)
        {
            e.Cancel = IsFilePathInvalid();
        }

        private bool IsFilePathInvalid()
        {
            var path = txtFilePath.Text.Trim();
            if (string.IsNullOrEmpty(path))
            {
                errorProvider1.SetError(txtFilePath, "Must be entered");
                return true;
            }
            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                errorProvider1.SetError(txtFilePath, "Directory does not exist");
                return true;
            }
            if (File.Exists(path))
            {
                errorProvider1.SetError(txtFilePath, "File already exists");
                return true;
            }
            errorProvider1.SetError(txtFilePath, string.Empty);
            return false;
        }

        private void ReadSchemaClick(object sender, EventArgs e)
        {
            if (IsFilePathInvalid()) return;
            if (!IsConnectionStringValid()) return;

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
            var sqlType = ProviderToSqlType.Convert(_providerName);
            return !sqlType.HasValue ? SqlType.SqlServer : sqlType.Value;
        }

        private void BackgroundDoWork(object sender, DoWorkEventArgs e)
        {
            var rdr = (DatabaseReader)e.Argument;
            var runner = new Runner(rdr, _filePath, OriginSqlType(), SqlServerCE4Radio.Checked);
            //pass thru the event to background worker
            runner.ProgressChanged += (s1, e1) => backgroundWorker1.ReportProgress(e1.ProgressPercentage, e1.UserState);
            var result = runner.Execute();
            if (!result) e.Result = runner.LastErrorMessage;
            else e.Result = null;
            rdr.Dispose();
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
                var message = (string)e.Result;
                if (string.IsNullOrEmpty(message))
                    toolStripStatusLabel1.Text = @"Database created";
                else
                    toolStripStatusLabel1.Text = message;
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

        private void SqlServerCe4RadioCheckedChanged(object sender, EventArgs e)
        {
            ChangeExtension(".db", ".sdf");
        }

        private void SqlLiteRadioCheckedChanged(object sender, EventArgs e)
        {
            ChangeExtension(".sdf", ".db");
        }

        private void ChangeExtension(string @from, string to)
        {
            var filePath = txtFilePath.Text.Trim();
            if (string.IsNullOrEmpty(filePath)) return;
            var dir = Path.GetDirectoryName(filePath);
            if (dir == null) return;
            if (Path.GetExtension(filePath).Equals(@from, StringComparison.OrdinalIgnoreCase))
            {
                filePath = Path.Combine(dir, Path.GetFileNameWithoutExtension(filePath) + to);
                txtFilePath.Text = filePath;
            }
        }
    }
}
