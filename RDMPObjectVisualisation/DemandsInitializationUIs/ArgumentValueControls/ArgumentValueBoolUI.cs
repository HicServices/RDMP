using System.Data;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using ReusableUIComponents;

namespace RDMPObjectVisualisation.DemandsInitializationUIs.ArgumentValueControls
{
    /// <summary>
    /// Allows you to specify the value of an IArugment (the database persistence value of a [DemandsInitialization] decorated Property on a MEF class e.g. a Pipeline components public property that the user can set)
    /// 
    /// This Control is for setting Properties that are of Type bool (true/false)
    /// </summary>
    [TechnicalUI]
    public partial class ArgumentValueBoolUI : UserControl, IArgumentValueUI
    {
        private Argument _argument;

        public ArgumentValueBoolUI()
        {
            InitializeComponent();
        }

        public void SetUp(Argument argument, DemandsInitialization demand, DataTable previewIfAny)
        {
            _argument = argument;
            cbValue.Checked = (bool) argument.GetValueAsSystemType();
        }

        private void cbValue_CheckedChanged(object sender, System.EventArgs e)
        {
            _argument.SetValue(cbValue.Checked);
            _argument.SaveToDatabase();
        }
    }
}
