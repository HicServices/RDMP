using System;
using System.Windows.Forms;
using ReusableUIComponents;
using ReusableUIComponents.ScintillaHelper;
using ScintillaNET;

namespace CatalogueManager.PluginManagement.CodeGeneration
{
    /// <summary>
    /// TECHNICAL: Allows you as a C# programmer to generate RDMP code automatically to help you build plugins and particularly plugin ITableRepository databases more efficiently.
    /// </summary>
    public partial class GenerateClassCodeFromTable : UserControl
    {
        private Scintilla _codeEditor;

        public GenerateClassCodeFromTable()
        {
            InitializeComponent();

            var factory = new ScintillaTextEditorFactory();
            _codeEditor = factory.Create(null, "csharp");
            _codeEditor.ReadOnly = false;
            panel1.Controls.Add(_codeEditor);
        }

        private void btnGenerateCode_Click(object sender, EventArgs e)
        {

            try
            {
                var db = serverDatabaseTableSelector1.GetDiscoveredDatabase();
                var table = db.ExpectTable(serverDatabaseTableSelector1.Table);
                var generator = new MapsDirectlyToDatabaseTableClassCodeGenerator(table);
                _codeEditor.Text = generator.GetCode();

            }
            catch (Exception exception)
            {
                ExceptionViewer.Show(exception);
            }
        }
    }
}
