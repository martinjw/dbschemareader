namespace CopyToSQLite
{
    partial class CopyForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.DataProviders = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.ConnectionString = new System.Windows.Forms.TextBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ReadSchema = new System.Windows.Forms.Button();
            this.SchemaOwner = new System.Windows.Forms.TextBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.label3 = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.WriteToFileLabel = new System.Windows.Forms.Label();
            this.btnFolderPicker = new System.Windows.Forms.Button();
            this.txtFilePath = new System.Windows.Forms.TextBox();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.WarningLabel = new System.Windows.Forms.Label();
            this.TargetGroupBox = new System.Windows.Forms.GroupBox();
            this.SqlServerCE4Radio = new System.Windows.Forms.RadioButton();
            this.SQLLiteRadio = new System.Windows.Forms.RadioButton();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.TargetGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // DataProviders
            // 
            this.DataProviders.DisplayMember = "dbo";
            this.DataProviders.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.DataProviders.FormattingEnabled = true;
            this.DataProviders.Location = new System.Drawing.Point(96, 33);
            this.DataProviders.Name = "DataProviders";
            this.DataProviders.Size = new System.Drawing.Size(183, 21);
            this.DataProviders.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Data Provider";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 60);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(91, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Connection String";
            // 
            // ConnectionString
            // 
            this.ConnectionString.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ConnectionString.ContextMenuStrip = this.contextMenuStrip1;
            this.ConnectionString.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::CopyToSQLite.Properties.Settings.Default, "ConnectionString", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.ConnectionString.Location = new System.Drawing.Point(6, 75);
            this.ConnectionString.Name = "ConnectionString";
            this.ConnectionString.Size = new System.Drawing.Size(570, 20);
            this.ConnectionString.TabIndex = 4;
            this.ConnectionString.Text = global::CopyToSQLite.Properties.Settings.Default.ConnectionString;
            this.ConnectionString.Validating += new System.ComponentModel.CancelEventHandler(this.ConnectionStringValidating);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // ReadSchema
            // 
            this.ReadSchema.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ReadSchema.Location = new System.Drawing.Point(6, 220);
            this.ReadSchema.Name = "ReadSchema";
            this.ReadSchema.Size = new System.Drawing.Size(570, 61);
            this.ReadSchema.TabIndex = 11;
            this.ReadSchema.Text = "Read Schema";
            this.ReadSchema.UseVisualStyleBackColor = true;
            this.ReadSchema.Click += new System.EventHandler(this.ReadSchemaClick);
            // 
            // SchemaOwner
            // 
            this.SchemaOwner.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::CopyToSQLite.Properties.Settings.Default, "SchemaOwner", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.SchemaOwner.Location = new System.Drawing.Point(96, 102);
            this.SchemaOwner.Name = "SchemaOwner";
            this.SchemaOwner.Size = new System.Drawing.Size(100, 20);
            this.SchemaOwner.TabIndex = 6;
            this.SchemaOwner.Text = global::CopyToSQLite.Properties.Settings.Default.SchemaOwner;
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(6, 287);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(570, 23);
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar1.TabIndex = 12;
            this.progressBar1.Visible = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 105);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(80, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Schema Owner";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 367);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(625, 22);
            this.statusStrip1.TabIndex = 13;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 17);
            // 
            // WriteToFileLabel
            // 
            this.WriteToFileLabel.AutoSize = true;
            this.WriteToFileLabel.Location = new System.Drawing.Point(6, 178);
            this.WriteToFileLabel.Name = "WriteToFileLabel";
            this.WriteToFileLabel.Size = new System.Drawing.Size(95, 13);
            this.WriteToFileLabel.TabIndex = 8;
            this.WriteToFileLabel.Text = "Write to SQLite file";
            // 
            // btnFolderPicker
            // 
            this.btnFolderPicker.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFolderPicker.Location = new System.Drawing.Point(586, 192);
            this.btnFolderPicker.Name = "btnFolderPicker";
            this.btnFolderPicker.Size = new System.Drawing.Size(27, 23);
            this.btnFolderPicker.TabIndex = 10;
            this.btnFolderPicker.Text = "...";
            this.btnFolderPicker.UseVisualStyleBackColor = true;
            this.btnFolderPicker.Click += new System.EventHandler(this.FolderPickerClick);
            // 
            // txtFilePath
            // 
            this.txtFilePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFilePath.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::CopyToSQLite.Properties.Settings.Default, "SqLiteFilePath", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.txtFilePath.Location = new System.Drawing.Point(6, 194);
            this.txtFilePath.Name = "txtFilePath";
            this.txtFilePath.Size = new System.Drawing.Size(570, 20);
            this.txtFilePath.TabIndex = 9;
            this.txtFilePath.Text = global::CopyToSQLite.Properties.Settings.Default.SqLiteFilePath;
            this.txtFilePath.Validating += new System.ComponentModel.CancelEventHandler(this.FilePathValidating);
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerReportsProgress = true;
            this.backgroundWorker1.WorkerSupportsCancellation = true;
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BackgroundDoWork);
            this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.BackgroundProgressChanged);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.BackgroundCompleted);
            // 
            // WarningLabel
            // 
            this.WarningLabel.AutoSize = true;
            this.WarningLabel.ForeColor = System.Drawing.Color.Red;
            this.WarningLabel.Location = new System.Drawing.Point(9, 4);
            this.WarningLabel.Name = "WarningLabel";
            this.WarningLabel.Size = new System.Drawing.Size(416, 13);
            this.WarningLabel.TabIndex = 0;
            this.WarningLabel.Text = "Copies tables and data to SQLite database. DO NOT USE FOR LARGE DATABASES!";
            // 
            // TargetGroupBox
            // 
            this.TargetGroupBox.Controls.Add(this.SqlServerCE4Radio);
            this.TargetGroupBox.Controls.Add(this.SQLLiteRadio);
            this.TargetGroupBox.Location = new System.Drawing.Point(9, 128);
            this.TargetGroupBox.Name = "TargetGroupBox";
            this.TargetGroupBox.Size = new System.Drawing.Size(567, 47);
            this.TargetGroupBox.TabIndex = 7;
            this.TargetGroupBox.TabStop = false;
            this.TargetGroupBox.Text = "Target Type";
            this.TargetGroupBox.Visible = false;
            // 
            // SqlServerCE4Radio
            // 
            this.SqlServerCE4Radio.AutoSize = true;
            this.SqlServerCE4Radio.Location = new System.Drawing.Point(107, 24);
            this.SqlServerCE4Radio.Name = "SqlServerCE4Radio";
            this.SqlServerCE4Radio.Size = new System.Drawing.Size(97, 17);
            this.SqlServerCE4Radio.TabIndex = 1;
            this.SqlServerCE4Radio.Text = "SqlServer CE 4";
            this.SqlServerCE4Radio.UseVisualStyleBackColor = true;
            this.SqlServerCE4Radio.CheckedChanged += new System.EventHandler(this.SqlServerCe4RadioCheckedChanged);
            // 
            // SQLLiteRadio
            // 
            this.SQLLiteRadio.AutoSize = true;
            this.SQLLiteRadio.Checked = true;
            this.SQLLiteRadio.Location = new System.Drawing.Point(19, 24);
            this.SQLLiteRadio.Name = "SQLLiteRadio";
            this.SQLLiteRadio.Size = new System.Drawing.Size(57, 17);
            this.SQLLiteRadio.TabIndex = 0;
            this.SQLLiteRadio.TabStop = true;
            this.SQLLiteRadio.Text = "SQLite";
            this.SQLLiteRadio.UseVisualStyleBackColor = true;
            this.SQLLiteRadio.CheckedChanged += new System.EventHandler(this.SqlLiteRadioCheckedChanged);
            // 
            // CopyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(625, 389);
            this.Controls.Add(this.TargetGroupBox);
            this.Controls.Add(this.WarningLabel);
            this.Controls.Add(this.btnFolderPicker);
            this.Controls.Add(this.txtFilePath);
            this.Controls.Add(this.WriteToFileLabel);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.SchemaOwner);
            this.Controls.Add(this.ReadSchema);
            this.Controls.Add(this.ConnectionString);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.DataProviders);
            this.Name = "CopyForm";
            this.Text = "Copy To SQLite";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CopyFormFormClosing);
            this.Load += new System.EventHandler(this.Form1Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.TargetGroupBox.ResumeLayout(false);
            this.TargetGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox DataProviders;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox ConnectionString;
        private System.Windows.Forms.Button ReadSchema;
        private System.Windows.Forms.TextBox SchemaOwner;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.Label WriteToFileLabel;
        private System.Windows.Forms.Button btnFolderPicker;
        private System.Windows.Forms.TextBox txtFilePath;
        private System.Windows.Forms.ErrorProvider errorProvider1;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.Label WarningLabel;
        private System.Windows.Forms.GroupBox TargetGroupBox;
        private System.Windows.Forms.RadioButton SqlServerCE4Radio;
        private System.Windows.Forms.RadioButton SQLLiteRadio;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
    }
}

