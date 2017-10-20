using System;
using System.ComponentModel;
using System.Windows.Forms;
using ReusableLibraryCode;
using ReusableUIComponents.CommandExecution;
using ReusableUIComponents.ScintillaHelper;
using ScintillaNET;

namespace ReusableUIComponents.SqlDialogs
{
    /// <summary>
    /// Allows the user to view and edit some SQL they have written.  Basically the same as ShowSQL but this window expects you to have populated some meaningful SQL that the
    /// caller will store/use somehow.
    /// </summary>
    public partial class SetSQLDialog : Form
    {   
        public Scintilla QueryEditor;
        private bool _designMode;

        public string Result
        {
            get { return QueryEditor.Text; }
        }

        public SetSQLDialog(string originalSQL, ICommandFactory commandFactory)
        {
            InitializeComponent();
            
            _designMode = (LicenseManager.UsageMode == LicenseUsageMode.Designtime);

            if (_designMode) //dont add the QueryEditor if we are in design time (visual studio) because it breaks
                return;

            QueryEditor = new ScintillaTextEditorFactory().Create(commandFactory);
            QueryEditor.Text = originalSQL;
            
            panel1.Controls.Add(QueryEditor);
        
        }

        private void button1_Click(object sender, EventArgs e)
        {

            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
