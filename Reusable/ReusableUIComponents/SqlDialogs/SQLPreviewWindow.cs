using System;
using System.ComponentModel;
using System.Windows.Forms;
using ReusableUIComponents.ScintillaHelper;
using ScintillaNET;

namespace ReusableUIComponents.SqlDialogs
{
    /// <summary>
    /// Shows you some SQL the system is about to execute with a description of what it is trying to achieve.  You can choose either 'Ok' to execute the SQL and carry on with the rest
    /// of the ongoing procedure or Cancel (the SQL will not run and the procedure will be abandoned).
    /// </summary>
    public partial class SQLPreviewWindow : Form
    {
        private ScintillaNET.Scintilla QueryEditor;

        public SQLPreviewWindow(string title, string msg, string sql)
        {
            InitializeComponent();

            lblMessage.Text = msg;
            this.Text = title;

            bool designMode = (LicenseManager.UsageMode == LicenseUsageMode.Designtime);

            if (designMode) //dont add the QueryEditor if we are in design time (visual studio) because it breaks
                return;

            QueryEditor = new ScintillaTextEditorFactory().Create();
            QueryEditor.Text = sql;

            QueryEditor.ReadOnly = true;

            panel1.Controls.Add(QueryEditor);
            btnOk.Select();
        }

        public bool YesToAll { get; set; }

        private void btnOk_Click(object sender, EventArgs e)
        {
            YesToAll = sender == btnOkToAll;

            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult=DialogResult.Cancel;
            this.Close();
        }

        public static DialogResult Show(string title,string message, string sql)
        {
            SQLPreviewWindow dialog = new SQLPreviewWindow(title,message, sql);
            dialog.ShowDialog();

            return dialog.DialogResult;
        }
    }
}
