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
        private Argument _argument;
        private DemandsInitializationAttribute _demand;

        public ArgumentValueSqlUI()
        {
            InitializeComponent();
        }

        public void SetUp(Argument argument, RequiredPropertyInfo requirement, DataTable previewIfAny)
        {
            _argument = argument;
            _demand = requirement.Demand;

            BombIfMandatoryAndEmpty();
        }

        private void btnSetSQL_Click(object sender, System.EventArgs e)
        {
            
            if (_argument != null)
            {
                SetSQLDialog dialog = new SetSQLDialog(_argument.Value, new RDMPCommandFactory());
                DialogResult d = dialog.ShowDialog();

                if (d == DialogResult.OK)
                {
                    ragSmiley1.Reset();

                    _argument.SetValue(dialog.Result);
                    _argument.SaveToDatabase();

                    BombIfMandatoryAndEmpty();
                }
            }
        }

        private void BombIfMandatoryAndEmpty()
        {
            if (_demand.Mandatory && string.IsNullOrWhiteSpace(_argument.Value))
                ragSmiley1.Fatal(
                    new Exception("Property is Mandatory which means it you have to Type an appropriate input in"));
        }
    }
}
