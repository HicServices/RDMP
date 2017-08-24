using System.ComponentModel;
using System.Windows.Forms;
using ReusableUIComponents;

using ReusableUIComponents.ScintillaHelper;
using ScintillaNET;

namespace CatalogueManager.SimpleDialogs
{
    /// <summary>
    /// Allows you to view a given piece of SQL.  This dialog is used whenever the RDMP wants to show you some SQL and includes syntax highlighting.  It may be readonly or editable
    /// depending on the context in which the dialog was launched.
    /// </summary>
    public partial class ShowSQL : Form
    {

        private ScintillaNET.Scintilla QueryEditor;
        private bool _designMode;


        public ShowSQL(string sql, bool isReadOnly = false)
        {
            InitializeComponent();

            _designMode = (LicenseManager.UsageMode == LicenseUsageMode.Designtime);

            if (_designMode) //dont add the QueryEditor if we are in design time (visual studio) because it breaks
                return;

            QueryEditor = new ScintillaTextEditorFactory().Create();
            QueryEditor.Text = sql;
            QueryEditor.ReadOnly = isReadOnly;

            this.Controls.Add(QueryEditor);

        }
    }
}
