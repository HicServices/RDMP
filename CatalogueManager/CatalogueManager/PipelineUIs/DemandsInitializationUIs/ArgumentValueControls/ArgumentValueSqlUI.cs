using System;
using System.Data;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.Copying;
using ReusableUIComponents;
using ReusableUIComponents.SqlDialogs;

namespace CatalogueManager.PipelineUIs.DemandsInitializationUIs.ArgumentValueControls
{
    /// <summary>
    /// Allows you to specify the value of an IArugment (the database persistence value of a [DemandsInitialization] decorated Property on a MEF class e.g. a Pipeline components public property that the user can set)
    /// 
    /// <para>This Control is for setting Properties that are of Type string but expect SQL code.  Clicking the button will launch an SQL editor with syntax highlighting.</para>
    /// </summary>
    [TechnicalUI]
    public partial class ArgumentValueSqlUI : UserControl, IArgumentValueUI
    {
        private ArgumentValueUIArgs _args;

        public ArgumentValueSqlUI()
        {
            InitializeComponent();
        }

        public void SetUp(ArgumentValueUIArgs args)
        {
            _args = args;
        }

        private void btnSetSQL_Click(object sender, System.EventArgs e)
        {

            SetSQLDialog dialog = new SetSQLDialog((string)_args.InitialValue, new RDMPCommandFactory());
            DialogResult d = dialog.ShowDialog();

            if (d == DialogResult.OK)
                _args.Setter(dialog.Result);
        }
    }
}
