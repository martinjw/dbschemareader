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
            this.label2 = new System.Windows.Forms.Label();
            this.txtNamespace = new System.Windows.Forms.TextBox();
            this.labExplanation = new System.Windows.Forms.Label();
            this.btnClasses = new System.Windows.Forms.Button();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.btnFolderPicker = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.txtFilePath = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "File path";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 66);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Namespace";
            // 
            // txtNamespace
            // 
            this.txtNamespace.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtNamespace.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::DatabaseSchemaViewer.Properties.Settings.Default, "CodeGenNamespace", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.txtNamespace.Location = new System.Drawing.Point(83, 63);
            this.txtNamespace.Name = "txtNamespace";
            this.txtNamespace.Size = new System.Drawing.Size(354, 20);
            this.txtNamespace.TabIndex = 3;
            this.txtNamespace.Text = global::DatabaseSchemaViewer.Properties.Settings.Default.CodeGenNamespace;
            this.txtNamespace.Validating += new System.ComponentModel.CancelEventHandler(this.txtNamespace_Validating);
            // 
            // labExplanation
            // 
            this.labExplanation.AutoSize = true;
            this.labExplanation.Location = new System.Drawing.Point(15, 4);
            this.labExplanation.Name = "labExplanation";
            this.labExplanation.Size = new System.Drawing.Size(124, 13);
            this.labExplanation.TabIndex = 4;
            this.labExplanation.Text = "This will write C# classes";
            // 
            // btnClasses
            // 
            this.btnClasses.Location = new System.Drawing.Point(83, 119);
            this.btnClasses.Name = "btnClasses";
            this.btnClasses.Size = new System.Drawing.Size(75, 23);
            this.btnClasses.TabIndex = 5;
            this.btnClasses.Text = "Generate C# Classes";
            this.btnClasses.UseVisualStyleBackColor = true;
            this.btnClasses.Click += new System.EventHandler(this.btnClasses_Click);
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
            this.statusStrip1.TabIndex = 6;
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
            this.btnFolderPicker.TabIndex = 7;
            this.btnFolderPicker.Text = "...";
            this.btnFolderPicker.UseVisualStyleBackColor = true;
            this.btnFolderPicker.Click += new System.EventHandler(this.btnFolderPicker_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(83, 119);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(354, 23);
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar1.TabIndex = 8;
            this.progressBar1.Visible = false;
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
            this.txtFilePath.TabIndex = 1;
            this.txtFilePath.Text = global::DatabaseSchemaViewer.Properties.Settings.Default.CodeGenFilePath;
            this.txtFilePath.Validating += new System.ComponentModel.CancelEventHandler(this.txtFilePath_Validating);
            // 
            // CodeGenForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(498, 266);
            this.Controls.Add(this.btnFolderPicker);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.btnClasses);
            this.Controls.Add(this.labExplanation);
            this.Controls.Add(this.txtNamespace);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtFilePath);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.progressBar1);
            this.Name = "CodeGenForm";
            this.Text = "CodeGenForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CodeGenForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtFilePath;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtNamespace;
        private System.Windows.Forms.Label labExplanation;
        private System.Windows.Forms.Button btnClasses;
        private System.Windows.Forms.ErrorProvider errorProvider1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.Button btnFolderPicker;
        private System.Windows.Forms.ProgressBar progressBar1;
    }
}