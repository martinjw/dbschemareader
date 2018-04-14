namespace DatabaseSchemaViewer
{
    partial class CompareForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CompareForm));
            this.CompareSchema = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.UseSavedSchema = new System.Windows.Forms.CheckBox();
            this.OpenSaved = new System.Windows.Forms.Button();
            this.chkInverse = new System.Windows.Forms.CheckBox();
            this.ConnectionString = new System.Windows.Forms.TextBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.Database = new System.Windows.Forms.GroupBox();
            this.labOriginDatabase = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.label3 = new System.Windows.Forms.Label();
            this.SchemaOwner = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.Database.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // CompareSchema
            // 
            this.CompareSchema.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CompareSchema.Location = new System.Drawing.Point(35, 199);
            this.CompareSchema.Name = "CompareSchema";
            this.CompareSchema.Size = new System.Drawing.Size(785, 80);
            this.CompareSchema.TabIndex = 4;
            this.CompareSchema.Text = "Compare Schema";
            this.CompareSchema.UseVisualStyleBackColor = true;
            this.CompareSchema.Click += new System.EventHandler(this.CompareSchemaClick);
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(35, 285);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(785, 23);
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar1.TabIndex = 4;
            this.progressBar1.Visible = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(23, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(91, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Connection String";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.SchemaOwner);
            this.groupBox1.Controls.Add(this.UseSavedSchema);
            this.groupBox1.Controls.Add(this.OpenSaved);
            this.groupBox1.Controls.Add(this.chkInverse);
            this.groupBox1.Controls.Add(this.ConnectionString);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(12, 60);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(811, 133);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Compare To Database";
            // 
            // UseSavedSchema
            // 
            this.UseSavedSchema.AutoSize = true;
            this.UseSavedSchema.Location = new System.Drawing.Point(224, 25);
            this.UseSavedSchema.Name = "UseSavedSchema";
            this.UseSavedSchema.Size = new System.Drawing.Size(15, 14);
            this.UseSavedSchema.TabIndex = 6;
            this.UseSavedSchema.UseVisualStyleBackColor = true;
            this.UseSavedSchema.Visible = false;
            // 
            // OpenSaved
            // 
            this.OpenSaved.Image = ((System.Drawing.Image)(resources.GetObject("OpenSaved.Image")));
            this.OpenSaved.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.OpenSaved.Location = new System.Drawing.Point(26, 20);
            this.OpenSaved.Name = "OpenSaved";
            this.OpenSaved.Size = new System.Drawing.Size(174, 23);
            this.OpenSaved.TabIndex = 5;
            this.OpenSaved.Text = "Open Saved Schema";
            this.OpenSaved.UseVisualStyleBackColor = true;
            this.OpenSaved.Click += new System.EventHandler(this.OpenSavedClick);
            // 
            // chkInverse
            // 
            this.chkInverse.AutoSize = true;
            this.chkInverse.Location = new System.Drawing.Point(23, 110);
            this.chkInverse.Name = "chkInverse";
            this.chkInverse.Size = new System.Drawing.Size(198, 17);
            this.chkInverse.TabIndex = 4;
            this.chkInverse.Text = "Inverse comparison (Second to First)";
            this.chkInverse.UseVisualStyleBackColor = true;
            // 
            // ConnectionString
            // 
            this.ConnectionString.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ConnectionString.ContextMenuStrip = this.contextMenuStrip1;
            this.ConnectionString.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::DatabaseSchemaViewer.Properties.Settings.Default, "CompareConnectionString", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.ConnectionString.Location = new System.Drawing.Point(23, 61);
            this.ConnectionString.Name = "ConnectionString";
            this.ConnectionString.Size = new System.Drawing.Size(689, 20);
            this.ConnectionString.TabIndex = 3;
            this.ConnectionString.Text = global::DatabaseSchemaViewer.Properties.Settings.Default.CompareConnectionString;
            this.ConnectionString.Validating += new System.ComponentModel.CancelEventHandler(this.ConnectionStringValidating);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // Database
            // 
            this.Database.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Database.Controls.Add(this.labOriginDatabase);
            this.Database.Location = new System.Drawing.Point(12, 13);
            this.Database.Name = "Database";
            this.Database.Size = new System.Drawing.Size(811, 41);
            this.Database.TabIndex = 1;
            this.Database.TabStop = false;
            this.Database.Text = "First Database";
            // 
            // labOriginDatabase
            // 
            this.labOriginDatabase.AutoSize = true;
            this.labOriginDatabase.Location = new System.Drawing.Point(23, 22);
            this.labOriginDatabase.Name = "labOriginDatabase";
            this.labOriginDatabase.Size = new System.Drawing.Size(0, 13);
            this.labOriginDatabase.TabIndex = 0;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 360);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(835, 22);
            this.statusStrip1.TabIndex = 5;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 17);
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BackgroundWorkerDoWork);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.BackgroundWorker1RunWorkerCompleted);
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(23, 84);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(80, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Schema Owner";
            // 
            // SchemaOwner
            // 
            this.SchemaOwner.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::DatabaseSchemaViewer.Properties.Settings.Default, "SchemaOwner", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.SchemaOwner.Location = new System.Drawing.Point(113, 81);
            this.SchemaOwner.Name = "SchemaOwner";
            this.SchemaOwner.Size = new System.Drawing.Size(100, 20);
            this.SchemaOwner.TabIndex = 8;
            this.SchemaOwner.Text = global::DatabaseSchemaViewer.Properties.Settings.Default.SchemaOwner;
            // 
            // CompareForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(835, 382);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.Database);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.CompareSchema);
            this.Controls.Add(this.progressBar1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CompareForm";
            this.Text = "CompareForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CompareForm_FormClosing);
            this.Load += new System.EventHandler(this.CompareFormLoad);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.Database.ResumeLayout(false);
            this.Database.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button CompareSchema;
        private System.Windows.Forms.TextBox ConnectionString;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox Database;
        private System.Windows.Forms.Label labOriginDatabase;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.ErrorProvider errorProvider1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.CheckBox chkInverse;
        private System.Windows.Forms.Button OpenSaved;
        private System.Windows.Forms.CheckBox UseSavedSchema;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox SchemaOwner;
    }
}