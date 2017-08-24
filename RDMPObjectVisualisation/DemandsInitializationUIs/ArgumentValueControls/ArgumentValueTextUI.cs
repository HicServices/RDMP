using System;
using System.Data;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using ReusableUIComponents;

namespace RDMPObjectVisualisation.DemandsInitializationUIs.ArgumentValueControls
{
    /// <summary>
    /// Allows you to specify the value of an IArugment (the database persistence value of a [DemandsInitialization] decorated Property on a MEF class e.g. a Pipeline components public property that the user can set)
    /// 
    /// This Control is for setting Properties that can be represented as textual strings (this includes parsed types like int, Regex etc).
    /// </summary>
    [TechnicalUI]
    public partial class ArgumentValueTextUI : UserControl, IArgumentValueUI
    {
        private Argument _argument;
        private DemandsInitialization _demand;
        private bool _bLoading = true;

        public ArgumentValueTextUI()
        {
            InitializeComponent();
        }

        public void SetUp(Argument argument, DemandsInitialization demand, DataTable previewIfAny)
        {
            _bLoading = true;
            _argument = argument;
            _demand = demand;
            tbText.Text = argument.Value;

            BombIfMandatoryAndEmpty();
            _bLoading = false;
        }

        private void tbText_TextChanged(object sender, System.EventArgs e)
        {
            if(_bLoading)
                return;

            ragSmiley1.Reset();

            _argument.Value = tbText.Text;
            _argument.SaveToDatabase();

            try
            {
                var val = _argument.GetValueAsSystemType();
                tbText.Text = val != null?val.ToString():"";

                BombIfMandatoryAndEmpty();

            }
            catch (Exception exception)
            {
                ragSmiley1.Fatal(exception);
            }

        }

        private void BombIfMandatoryAndEmpty()
        {
            if (_demand.Mandatory && string.IsNullOrWhiteSpace(tbText.Text))
                ragSmiley1.Fatal(new Exception("Property is Mandatory which means it you have to Type an appropriate input in"));
        }
    }
}
