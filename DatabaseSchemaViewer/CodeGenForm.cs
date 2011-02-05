using System;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using DatabaseSchemaReader.CodeGen;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaViewer
{
    public partial class CodeGenForm : Form
    {
        private readonly DatabaseSchema _databaseSchema;

        public CodeGenForm(DatabaseSchema databaseSchema)
        {
            _databaseSchema = databaseSchema;
            InitializeComponent();

            if (txtFilePath.Text == string.Empty)
                txtFilePath.Text = Environment.CurrentDirectory;
            errorProvider1.SetError(txtFilePath, string.Empty);
        }

        private void btnClasses_Click(object sender, EventArgs e)
        {
            if (!ValidateChildren()) return;

            StartWaiting();

            var directory = new DirectoryInfo(txtFilePath.Text.Trim());
            var ns = txtNamespace.Text.Trim();

            var cw = new CodeWriter(_databaseSchema);
            try
            {
                cw.Execute(directory, ns);
            }
            catch (IOException exception)
            {
                MessageBox.Show(
                    @"An IO error occurred while opening the file.\n" + exception.Message,
                    @"Exception",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1);
            }
            catch (UnauthorizedAccessException exception)
            {
                MessageBox.Show(
                    @"The caller does not have the required permission or path is readonly.\n" + exception.Message,
                    @"Exception",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error,
                    MessageBoxDefaultButton.Button1);
            }
            StopWaiting();
        }

        private void StopWaiting()
        {
            btnClasses.Visible = true;
            progressBar1.Visible = false;
            Cursor = Cursors.Default;
        }

        private void StartWaiting()
        {
            btnClasses.Visible = false;
            progressBar1.Visible = true;
            Cursor = Cursors.WaitCursor;
            Refresh();
        }

        private void txtFilePath_Validating(object sender, CancelEventArgs e)
        {
            var path = txtFilePath.Text.Trim();
            if (string.IsNullOrEmpty(path))
            {
                e.Cancel = true;
                errorProvider1.SetError(txtFilePath, "Must be entered");
                return;
            }
            if (!Directory.Exists(path))
            {
                e.Cancel = true;
                errorProvider1.SetError(txtFilePath, "Directory does not exist");
                return;
            }
            errorProvider1.SetError(txtFilePath, string.Empty);
        }

        private void txtNamespace_Validating(object sender, CancelEventArgs e)
        {
            var ns = txtNamespace.Text.Trim();
            //a simplistic regex just to exclude weird punctuation/spaces etc
            if (Regex.IsMatch(ns, @"^([\w]+)([\w\.])$"))
            {
                errorProvider1.SetError(txtNamespace, string.Empty);
            }
            else
            {
                e.Cancel = true;
                errorProvider1.SetError(txtNamespace, "Invalid namespace");
            }
        }

        private void btnFolderPicker_Click(object sender, EventArgs e)
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

        private void CodeGenForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.Save();
        }
    }
}
