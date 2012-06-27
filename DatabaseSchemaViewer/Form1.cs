using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.Common;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using DatabaseSchemaReader;
using DatabaseSchemaReader.Conversion;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaViewer
{
    public partial class Form1 : Form
    {
        private DatabaseSchema _databaseSchema;
        private readonly List<string> _installedProviders = new List<string>();

        public Form1()
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
            //DataProviders.DisplayMember = invariantname;
            //DataProviders.ValueMember = invariantname;
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

        private void Form1_Load(object sender, EventArgs e)
        {
            var selectedValue = Properties.Settings.Default.Provider;
            //the last used provider has been uninstalled
            if (!_installedProviders.Contains(selectedValue)) selectedValue = "System.Data.SqlClient";
            DataProviders.SelectedItem = selectedValue;
            //add sample connection strings
            connectionContext.Items.Clear();
            if (_installedProviders.Contains("IBM.Data.DB2"))
            {
                connectionContext.Items.Add("DB2 (Sample)").Click += (s, ev) => FillConnectionString(@"Server=localhost:50000;UID=db2admin;pwd=db2;Database=Sample");
            }
            if (_installedProviders.Contains("FirebirdSql.Data.FirebirdClient"))
            {
                connectionContext.Items.Add("Firebird (Employee)").Click += (s, ev) => FillConnectionString(@"User=SYSDBA;Password=masterkey;Database=C:\Program Files\Firebird\Firebird_2_1\examples\empbuild\EMPLOYEE.FDB;Server=localhost; Connection lifetime=15;Pooling=true");
            }
            if (_installedProviders.Contains("MySql.Data.MySqlClient") || _installedProviders.Contains("Devart.Data.MySql"))
            {
                connectionContext.Items.Add("MySQL (Northwind)").Click += (s, ev) => FillConnectionString(@"Server=localhost;Uid=root;Pwd=mysql;Database=Northwind;Allow User Variables=True;");
                connectionContext.Items.Add("MySQL (Sakila)").Click += (s, ev) => FillConnectionString(@"Server=localhost;Uid=root;Pwd=mysql;Database=Sakila;Allow User Variables=True;", @"Sakila");
            }
            connectionContext.Items.Add("Oracle XE - TNS (HR)").Click += (s, ev) => FillConnectionString(@"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SID=XE)));User Id=hr;Password=hr;", @"HR");
            connectionContext.Items.Add("Oracle XE (HR)").Click += (s, ev) => FillConnectionString(
                @"Data Source=XE;User Id=hr;Password=hr;", @"HR");
            if (_installedProviders.Contains("Devart.Data.Oracle"))
            {
                connectionContext.Items.Add("Oracle XE - devart direct mode (HR)").Click += (s, ev) => FillConnectionString(
                    @"Data Source=localhost;SID=XE;direct=true;User Id=hr;Password=hr;", @"HR");
            }
            if (_installedProviders.Contains("Npgsql") || _installedProviders.Contains("Devart.Data.Devart.Data.PostgreSql"))
            {
                connectionContext.Items.Add("Postgresql (world)").Click += (s, ev) => FillConnectionString(@"Server=127.0.0.1;User id=postgres;password=sql;database=world;");
            }
            if (_installedProviders.Contains("System.Data.SQLite") || _installedProviders.Contains("Devart.Data.SQLite"))
            {
                connectionContext.Items.Add("SQLite (Northwind)").Click += (s, ev) => FillConnectionString(@"Data Source=C:\Data\northwind.db;");
                connectionContext.Items.Add("SQLite (Chinook)").Click += (s, ev) => FillConnectionString(@"Data Source=C:\Data\Chinook_Sqlite.sqlite;");
            }
            connectionContext.Items.Add("Sql Server Express (AdventureWorks)").Click += (s, ev) => FillConnectionString(
                @"Data Source=.\SQLEXPRESS;Integrated Security=true;Initial Catalog=AdventureWorks");
            connectionContext.Items.Add("Sql Server Express (Northwind)").Click += (s, ev) => FillConnectionString(@"Data Source=.\SQLEXPRESS;Integrated Security=true;Initial Catalog=Northwind");
            if (_installedProviders.Contains("System.Data.SqlServerCe.4.0"))
            {
                connectionContext.Items.Add("SqlServerCe (Chinook)").Click += (s, ev) => FillConnectionString(@"Data Source=""C:\Data\Chinook_SqlServerCompact.sdf"";");
                //don't use the C:\Program Files\Microsoft SQL Server Compact Edition\v4.0\Samples version as you need admin rights
                connectionContext.Items.Add("SqlServerCe (Northwind)").Click += (s, ev) => FillConnectionString(@"Data Source=""C:\Data\northwind.sdf"";");
            }
            AllowDrop = true;
            DragEnter += FormDragEnter;
            DragDrop += FormDragDrop;
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
            catch (NotSupportedException)
            {
                errorProvider1.SetError(ConnectionString, "Invalid connection string");
                return false;
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
            toolStripStatusLabel1.Text = "Reading...";

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

        private void BackgroundWorker1DoWork(object sender, DoWorkEventArgs e)
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
                toolStripStatusLabel1.Text = string.Empty;
                _databaseSchema = e.Result as DatabaseSchema;
                if (_databaseSchema != null)
                {
                    SchemaToTreeview.PopulateTreeView(_databaseSchema, treeView1);
                    toolStripButton1.Enabled = true;
                    toolStripButton2.Enabled = true;
                    saveSchema.Enabled = true;
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

        private TreeNode _lastSelectedNode;
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

        private void TreeMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            var p = new Point(e.X, e.Y);

            var node = treeView1.GetNodeAt(p);
            if (node == null) return;

            var tag = node.Tag;

            if (tag == null) return;

            _lastSelectedNode = node;
            treeView1.SelectedNode = node;

            var menu = treeContext;
            menu.Items.Clear();

            var sqlType = FindSqlType();

            var schema = node.Tag as DatabaseSchema;
            var view = node.Tag as DatabaseView;
            var table = node.Tag as DatabaseTable;
            var column = node.Tag as DatabaseColumn;
            var pack = node.Tag as DatabasePackage;
            var sproc = node.Tag as DatabaseStoredProcedure;
            var fun = node.Tag as DatabaseFunction;
            var constraint = node.Tag as DatabaseConstraint;
            var trigger = node.Tag as DatabaseTrigger;
            var index = node.Tag as DatabaseIndex;
            if (schema != null)
            {
                var create = new ToolStripMenuItem("CREATE TABLEs to clipboard");
                create.Click += (s, ea) => new SqlTasks(sqlType).BuildAllTableDdl(schema);
                menu.Items.Add(create);
            }
            else if (view != null)
            {
                //views are based on tables, so do them first
                BuildViewMenu(menu, view, sqlType);
            }
            else if (table != null)
            {
                BuildTableMenu(menu, table, sqlType);
            }
            else if (pack != null)
            {
                var create = new ToolStripMenuItem("CREATE PACKAGE " + pack.Name + " to clipboard");
                create.Click += (s, ea) => new SqlTasks(sqlType).BuildPackage(pack);
                menu.Items.Add(create);
            }
            else if (fun != null)
            {
                var create = new ToolStripMenuItem("CREATE FUNCTION " + fun.Name + " to clipboard");
                create.Click += (s, ea) => new SqlTasks(sqlType).BuildFunction(fun);
                menu.Items.Add(create);
            }
            else if (sproc != null)
            {
                if (!string.IsNullOrEmpty(sproc.Sql))
                {
                    var create = new ToolStripMenuItem("CREATE STORED PROC " + sproc.Name + " to clipboard");
                    create.Click += (s, ea) => new SqlTasks(sqlType).BuildProcedure(sproc);
                    menu.Items.Add(create);

                    var bar = new ToolStripSeparator();
                    menu.Items.Add(bar);
                }

                var code = new ToolStripMenuItem("C# class to clipboard");
                code.Click += (s, ea) => new SqlTasks(sqlType).BuildProcedureCode(_databaseSchema, sproc);
                menu.Items.Add(code);

            }
            else if (column != null)
            {
                var add = new ToolStripMenuItem("ADD COLUMN " + column.Name + " to clipboard");
                add.Click += (s, ea) => new SqlTasks(sqlType).BuildAddColumn(column);
                menu.Items.Add(add);
                var alter = new ToolStripMenuItem("ALTER COLUMN " + column.Name + " to clipboard");
                alter.Click += (s, ea) => new SqlTasks(sqlType).BuildAlterColumn(column);
                menu.Items.Add(alter);
            }
            else if (constraint != null)
            {
                var parentTable = node.Parent.Parent.Tag as DatabaseTable;
                var add = new ToolStripMenuItem("ADD CONSTRAINT " + constraint.Name + " to clipboard");
                add.Click += (s, ea) => new SqlTasks(sqlType).BuildAddConstraint(parentTable, constraint);
                menu.Items.Add(add);
                var drop = new ToolStripMenuItem("DROP CONSTRAINT " + constraint.Name + " to clipboard");
                drop.Click += (s, ea) => new SqlTasks(sqlType).BuildDropConstraint(parentTable, constraint);
                menu.Items.Add(drop);
            }
            else if (trigger != null)
            {
                var parentTable = node.Parent.Parent.Tag as DatabaseTable;
                var add = new ToolStripMenuItem("ADD TRIGGER " + trigger.Name + " to clipboard");
                add.Click += (s, ea) => new SqlTasks(sqlType).BuildAddTrigger(parentTable, trigger);
                menu.Items.Add(add);
                var drop = new ToolStripMenuItem("DROP TRIGGER " + trigger.Name + " to clipboard");
                drop.Click += (s, ea) => new SqlTasks(sqlType).BuildDropTrigger(trigger);
                menu.Items.Add(drop);
            }
            else if (index != null)
            {
                var parentTable = node.Parent.Parent.Tag as DatabaseTable;
                var add = new ToolStripMenuItem("ADD INDEX " + index.Name + " to clipboard");
                add.Click += (s, ea) => new SqlTasks(sqlType).BuildAddIndex(parentTable, index);
                menu.Items.Add(add);
                var drop = new ToolStripMenuItem("DROP INDEX " + index.Name + " to clipboard");
                drop.Click += (s, ea) => new SqlTasks(sqlType).BuildDropIndex(parentTable, index);
                menu.Items.Add(drop);
            }


            menu.Show(treeView1, p);

            treeView1.SelectedNode = _lastSelectedNode;
            _lastSelectedNode = null;

        }

        private void BuildViewMenu(ToolStrip menu, DatabaseView view, SqlType sqlType)
        {
            if (!string.IsNullOrEmpty(view.Sql))
            {
                var create = new ToolStripMenuItem("CREATE VIEW " + view.Name + " to clipboard");
                create.Click += (s, ea) => new SqlTasks(sqlType).BuildView(view);
                menu.Items.Add(create);

                var bar = new ToolStripSeparator();
                menu.Items.Add(bar);
            }

            var select = new ToolStripMenuItem("SELECT VIEW to clipboard");
            select.Click += (s, ea) => new SqlTasks(sqlType).BuildTableSelect(view);
            menu.Items.Add(select);

            var selectPaged = new ToolStripMenuItem("SELECT VIEW PAGED to clipboard");
            selectPaged.Click += (s, ea) => new SqlTasks(sqlType).BuildTableSelectPaged(view);
            menu.Items.Add(selectPaged);
        }

        private void BuildTableMenu(ToolStrip menu, DatabaseTable table, SqlType sqlType)
        {
            if (menu == null) throw new ArgumentNullException("menu");
            var create = new ToolStripMenuItem("CREATE TABLE " + table.Name + " to clipboard");
            create.Click += (s, ea) => new SqlTasks(sqlType).BuildTableDdl(table);
            menu.Items.Add(create);

            var drop = new ToolStripMenuItem("DROP TABLE " + table.Name + " to clipboard");
            drop.Click += (s, ea) => new SqlTasks(sqlType).BuildDropTable(table);
            menu.Items.Add(drop);

            var bar = new ToolStripSeparator();
            menu.Items.Add(bar);

            var select = new ToolStripMenuItem("SELECT TABLE to clipboard");
            select.Click += (s, ea) => new SqlTasks(sqlType).BuildTableSelect(table);
            menu.Items.Add(select);

            var selectPaged = new ToolStripMenuItem("SELECT TABLE PAGED to clipboard");
            selectPaged.Click += (s, ea) => new SqlTasks(sqlType).BuildTableSelectPaged(table);
            menu.Items.Add(selectPaged);

            var insert = new ToolStripMenuItem("INSERT TABLE to clipboard");
            insert.Click += (s, ea) => new SqlTasks(sqlType).BuildTableInsert(table);
            menu.Items.Add(insert);

            var update = new ToolStripMenuItem("UPDATE TABLE to clipboard");
            update.Click += (s, ea) => new SqlTasks(sqlType).BuildTableUpdate(table);
            menu.Items.Add(update);

            var bar2 = new ToolStripSeparator();
            menu.Items.Add(bar2);

            var code = new ToolStripMenuItem("C# class to clipboard");
            code.Click += (s, ea) => new SqlTasks(sqlType).BuildClass(table);
            menu.Items.Add(code);
        }

        private SqlType FindSqlType()
        {
            var sqlType = ProviderToSqlType.Convert(_databaseSchema.Provider);
            return !sqlType.HasValue ? SqlType.SqlServer : sqlType.Value;
        }

        private void SaveSchemaClick(object sender, EventArgs e)
        {
            using (var picker = new SaveFileDialog())
            {
                picker.FileName = "db" +
                    DateTime.Now.ToString("yyyyMMddThhmmss", CultureInfo.InvariantCulture);
                picker.DefaultExt = ".dbschema";
                picker.Title = "Save schema to file.";
                picker.InitialDirectory =
                            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var result = picker.ShowDialog();
                if (result == DialogResult.OK)
                {
                    using (var stream = picker.OpenFile())
                    {
                        var f = new BinaryFormatter();
                        f.Serialize(stream, _databaseSchema);
                    }
                }
            }
        }

        private void OpenSchemaClick(object sender, EventArgs e)
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
                    using (var stream = picker.OpenFile())
                    {
                        var f = new BinaryFormatter();
                        try
                        {
                            _databaseSchema = f.Deserialize(stream) as DatabaseSchema;
                        }
                        catch (SerializationException)
                        {
                            toolStripStatusLabel1.Text = "Invalid serialization format";
                        }
                    }
                    if (_databaseSchema != null)
                    {
                        ConnectionString.Text = _databaseSchema.ConnectionString;
                        SchemaOwner.Text = _databaseSchema.Owner;
                        SelectProvider(_databaseSchema.Provider);
                        SchemaToTreeview.PopulateTreeView(_databaseSchema, treeView1);
                        toolStripButton1.Enabled = true;
                        toolStripButton2.Enabled = true;
                        saveSchema.Enabled = true;
                    }
                }
            }
        }

    }
}
