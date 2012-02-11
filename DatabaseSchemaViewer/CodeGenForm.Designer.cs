namespace DatabaseSchemaViewer
{
    partial class CodeGenForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.labExplanation = new System.Windows.Forms.Label();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.btnFolderPicker = new System.Windows.Forms.Button();
            this.labDialect = new System.Windows.Forms.Label();
            this.cmbDialect = new System.Windows.Forms.ComboBox();
            this.labNamespace = new System.Windows.Forms.Label();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.radCSharp = new System.Windows.Forms.RadioButton();
            this.radDdl = new System.Windows.Forms.RadioButton();
            this.radSprocs = new System.Windows.Forms.RadioButton();
            this.panelTables = new System.Windows.Forms.Panel();
            this.cmbTables = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.radData = new System.Windows.Forms.RadioButton();
            this.panelCodeGen = new System.Windows.Forms.Panel();
            this.labProjectType = new System.Windows.Forms.Label();
            this.cmbProjectType = new System.Windows.Forms.ComboBox();
            this.chkReadSprocs = new System.Windows.Forms.CheckBox();
            this.txtNamespace = new System.Windows.Forms.TextBox();
            this.txtFilePath = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.panelTables.SuspendLayout();
            this.panelCodeGen.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "File path";
            // 
            // labExplanation
            // 
            this.labExplanation.AutoSize = true;
            this.labExplanation.Location = new System.Drawing.Point(15, 4);
            this.labExplanation.Name = "labExplanation";
            this.labExplanation.Size = new System.Drawing.Size(408, 13);
            this.labExplanation.TabIndex = 0;
            this.labExplanation.Text = "Generate code (C# and NHibernate mapping, Table DDL, CRUD Stored Procedures)";
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 244);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(498, 22);
            this.statusStrip1.TabIndex = 16;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 17);
            // 
            // btnFolderPicker
            // 
            this.btnFolderPicker.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFolderPicker.Location = new System.Drawing.Point(459, 30);
            this.btnFolderPicker.Name = "btnFolderPicker";
            this.btnFolderPicker.Size = new System.Drawing.Size(27, 23);
            this.btnFolderPicker.TabIndex = 3;
            this.btnFolderPicker.Text = "...";
            this.btnFolderPicker.UseVisualStyleBackColor = true;
            this.btnFolderPicker.Click += new System.EventHandler(this.FolderPickerClick);
            // 
            // labDialect
            // 
            this.labDialect.AutoSize = true;
            this.labDialect.Location = new System.Drawing.Point(15, 126);
            this.labDialect.Name = "labDialect";
            this.labDialect.Size = new System.Drawing.Size(58, 13);
            this.labDialect.TabIndex = 9;
            this.labDialect.Text = "Sql Dialect";
            // 
            // cmbDialect
            // 
            this.cmbDialect.FormattingEnabled = true;
            this.cmbDialect.Location = new System.Drawing.Point(83, 123);
            this.cmbDialect.Name = "cmbDialect";
            this.cmbDialect.Size = new System.Drawing.Size(121, 21);
            this.cmbDialect.TabIndex = 10;
            this.cmbDialect.Validating += new System.ComponentModel.CancelEventHandler(this.DialectValidating);
            // 
            // labNamespace
            // 
            this.labNamespace.AutoSize = true;
            this.labNamespace.Location = new System.Drawing.Point(13, 100);
            this.labNamespace.Name = "labNamespace";
            this.labNamespace.Size = new System.Drawing.Size(64, 13);
            this.labNamespace.TabIndex = 7;
            this.labNamespace.Text = "Namespace";
            // 
            // btnGenerate
            // 
            this.btnGenerate.Location = new System.Drawing.Point(83, 205);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(75, 23);
            this.btnGenerate.TabIndex = 15;
            this.btnGenerate.Text = "Generate";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.GenerateClick);
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(83, 205);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(354, 23);
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar1.TabIndex = 16;
            this.progressBar1.Visible = false;
            // 
            // radCSharp
            // 
            this.radCSharp.AutoSize = true;
            this.radCSharp.Checked = true;
            this.radCSharp.Location = new System.Drawing.Point(83, 59);
            this.radCSharp.Name = "radCSharp";
            this.radCSharp.Size = new System.Drawing.Size(39, 17);
            this.radCSharp.TabIndex = 4;
            this.radCSharp.TabStop = true;
            this.radCSharp.Text = "C#";
            this.radCSharp.UseVisualStyleBackColor = true;
            this.radCSharp.CheckedChanged += new System.EventHandler(this.RadioCheckedChanged);
            // 
            // radDdl
            // 
            this.radDdl.AutoSize = true;
            this.radDdl.Location = new System.Drawing.Point(172, 59);
            this.radDdl.Name = "radDdl";
            this.radDdl.Size = new System.Drawing.Size(77, 17);
            this.radDdl.TabIndex = 5;
            this.radDdl.Text = "Table DDL";
            this.radDdl.UseVisualStyleBackColor = true;
            this.radDdl.CheckedChanged += new System.EventHandler(this.RadioCheckedChanged);
            // 
            // radSprocs
            // 
            this.radSprocs.AutoSize = true;
            this.radSprocs.Location = new System.Drawing.Point(289, 59);
            this.radSprocs.Name = "radSprocs";
            this.radSprocs.Size = new System.Drawing.Size(103, 17);
            this.radSprocs.TabIndex = 6;
            this.radSprocs.Text = "SQL Procedures";
            this.radSprocs.UseVisualStyleBackColor = true;
            this.radSprocs.CheckedChanged += new System.EventHandler(this.RadioCheckedChanged);
            // 
            // panelTables
            // 
            this.panelTables.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panelTables.Controls.Add(this.cmbTables);
            this.panelTables.Controls.Add(this.label2);
            this.panelTables.Location = new System.Drawing.Point(13, 150);
            this.panelTables.Name = "panelTables";
            this.panelTables.Size = new System.Drawing.Size(463, 39);
            this.panelTables.TabIndex = 12;
            // 
            // cmbTables
            // 
            this.cmbTables.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbTables.FormattingEnabled = true;
            this.cmbTables.Location = new System.Drawing.Point(71, 9);
            this.cmbTables.Name = "cmbTables";
            this.cmbTables.Size = new System.Drawing.Size(353, 21);
            this.cmbTables.TabIndex = 14;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(34, 13);
            this.label2.TabIndex = 13;
            this.label2.Text = "Table";
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.BackgroundWorker1RunWorkerCompleted);
            // 
            // radData
            // 
            this.radData.AutoSize = true;
            this.radData.Location = new System.Drawing.Point(438, 59);
            this.radData.Name = "radData";
            this.radData.Size = new System.Drawing.Size(48, 17);
            this.radData.TabIndex = 17;
            this.radData.Text = "Data";
            this.radData.UseVisualStyleBackColor = true;
            this.radData.CheckedChanged += new System.EventHandler(this.RadioCheckedChanged);
            // 
            // panelCodeGen
            // 
            this.panelCodeGen.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panelCodeGen.Controls.Add(this.labProjectType);
            this.panelCodeGen.Controls.Add(this.cmbProjectType);
            this.panelCodeGen.Controls.Add(this.chkReadSprocs);
            this.panelCodeGen.Location = new System.Drawing.Point(13, 123);
            this.panelCodeGen.Name = "panelCodeGen";
            this.panelCodeGen.Size = new System.Drawing.Size(463, 60);
            this.panelCodeGen.TabIndex = 18;
            this.panelCodeGen.Visible = false;
            // 
            // labProjectType
            // 
            this.labProjectType.AutoSize = true;
            this.labProjectType.Location = new System.Drawing.Point(0, 31);
            this.labProjectType.Name = "labProjectType";
            this.labProjectType.Size = new System.Drawing.Size(67, 13);
            this.labProjectType.TabIndex = 14;
            this.labProjectType.Text = "Project Type";
            // 
            // cmbProjectType
            // 
            this.cmbProjectType.FormattingEnabled = true;
            this.cmbProjectType.Location = new System.Drawing.Point(71, 28);
            this.cmbProjectType.Name = "cmbProjectType";
            this.cmbProjectType.Size = new System.Drawing.Size(121, 21);
            this.cmbProjectType.TabIndex = 13;
            // 
            // chkReadSprocs
            // 
            this.chkReadSprocs.AutoSize = true;
            this.chkReadSprocs.Checked = global::DatabaseSchemaViewer.Properties.Settings.Default.CodeGenReadProcedures;
            this.chkReadSprocs.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::DatabaseSchemaViewer.Properties.Settings.Default, "CodeGenReadProcedures", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.chkReadSprocs.Location = new System.Drawing.Point(71, 8);
            this.chkReadSprocs.Name = "chkReadSprocs";
            this.chkReadSprocs.Size = new System.Drawing.Size(181, 17);
            this.chkReadSprocs.TabIndex = 12;
            this.chkReadSprocs.Text = "Read Stored Procedures Results";
            this.chkReadSprocs.UseVisualStyleBackColor = true;
            this.chkReadSprocs.Visible = false;
            // 
            // txtNamespace
            // 
            this.txtNamespace.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtNamespace.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::DatabaseSchemaViewer.Properties.Settings.Default, "CodeGenNamespace", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.txtNamespace.Location = new System.Drawing.Point(83, 97);
            this.txtNamespace.Name = "txtNamespace";
            this.txtNamespace.Size = new System.Drawing.Size(354, 20);
            this.txtNamespace.TabIndex = 8;
            this.txtNamespace.Text = global::DatabaseSchemaViewer.Properties.Settings.Default.CodeGenNamespace;
            this.txtNamespace.Validating += new System.ComponentModel.CancelEventHandler(this.NamespaceValidating);
            // 
            // txtFilePath
            // 
            this.txtFilePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFilePath.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::DatabaseSchemaViewer.Properties.Settings.Default, "CodeGenFilePath", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.errorProvider1.SetError(this.txtFilePath, "Invalid path");
            this.txtFilePath.Location = new System.Drawing.Point(83, 32);
            this.txtFilePath.Name = "txtFilePath";
            this.txtFilePath.Size = new System.Drawing.Size(354, 20);
            this.txtFilePath.TabIndex = 2;
            this.txtFilePath.Text = global::DatabaseSchemaViewer.Properties.Settings.Default.CodeGenFilePath;
            this.txtFilePath.Validating += new System.ComponentModel.CancelEventHandler(this.FilePathValidating);
            // 
            // CodeGenForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(498, 266);
            this.Controls.Add(this.panelCodeGen);
            this.Controls.Add(this.radData);
            this.Controls.Add(this.panelTables);
            this.Controls.Add(this.radSprocs);
            this.Controls.Add(this.radDdl);
            this.Controls.Add(this.radCSharp);
            this.Controls.Add(this.cmbDialect);
            this.Controls.Add(this.txtNamespace);
            this.Controls.Add(this.labDialect);
            this.Controls.Add(this.labNamespace);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.btnFolderPicker);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.labExplanation);
            this.Controls.Add(this.txtFilePath);
            this.Controls.Add(this.label1);
            this.Name = "CodeGenForm";
            this.Text = "Code Generation";
            this.Load += new System.EventHandler(this.CodeGenFormLoad);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CodeGenFormFormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.panelTables.ResumeLayout(false);
            this.panelTables.PerformLayout();
            this.panelCodeGen.ResumeLayout(false);
            this.panelCodeGen.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtFilePath;
        private System.Windows.Forms.Label labExplanation;
        private System.Windows.Forms.ErrorProvider errorProvider1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.Button btnFolderPicker;
        private System.Windows.Forms.ComboBox cmbDialect;
        private System.Windows.Forms.TextBox txtNamespace;
        private System.Windows.Forms.Label labDialect;
        private System.Windows.Forms.Label labNamespace;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.RadioButton radSprocs;
        private System.Windows.Forms.RadioButton radDdl;
        private System.Windows.Forms.RadioButton radCSharp;
        private System.Windows.Forms.Panel panelTables;
        private System.Windows.Forms.ComboBox cmbTables;
        private System.Windows.Forms.Label label2;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.RadioButton radData;
        private System.Windows.Forms.Panel panelCodeGen;
        private System.Windows.Forms.CheckBox chkReadSprocs;
        private System.Windows.Forms.Label labProjectType;
        private System.Windows.Forms.ComboBox cmbProjectType;
    }
}