using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace DatabaseSchemaViewer
{
    public partial class ScriptForm : Form
    {
        private readonly string _script;

        public ScriptForm(string script)
        {
            _script = script;
            InitializeComponent();
        }

        private void ScriptFormLoad(object sender, EventArgs e)
        {
            StatusLabel.Text = string.Format(CultureInfo.InvariantCulture, "{0} characters", _script.Length);
            textBox1.Text = _script;
            if (string.IsNullOrEmpty(_script))
            {
                textBox1.Text = @"-- Nothing";
            }
        }

        private void SaveButtonClick(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Title = @"Save script";
                dialog.InitialDirectory = Properties.Settings.Default.ScriptDirectory;
                dialog.DefaultExt = ".sql";
                dialog.FileName = "script.sql";
                var result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    try
                    {
                        File.WriteAllText(dialog.FileName, _script);
                        Properties.Settings.Default.ScriptDirectory = Path.GetDirectoryName(dialog.FileName);
                        StatusLabel.Text = @"Saved";
                    }
                    catch (SystemException exception)
                    {
                        StatusLabel.Text = exception.Message;
                    }
                }
            }
        }

        private void CopyButtonClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_script)) return;
            Clipboard.SetText(_script);
            StatusLabel.Text = @"Copied to clipboard";
        }
    }
}
