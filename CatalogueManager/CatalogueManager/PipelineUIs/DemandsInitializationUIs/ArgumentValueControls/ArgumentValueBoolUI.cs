using System.Data;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using ReusableUIComponents;

namespace CatalogueManager.PipelineUIs.DemandsInitializationUIs.ArgumentValueControls
{
    /// <summary>
    /// Allows you to specify the value of an IArugment (the database persistence value of a [DemandsInitialization] decorated Property on a MEF class e.g. a Pipeline components public property that the user can set)
    /// 
    /// <para>This Control is for setting Properties that are of Type bool (true/false)</para>
    /// </summary>
    [TechnicalUI]
    public partial class ArgumentValueBoolUI : UserControl, IArgumentValueUI
    {
        private Argument _argument;
        private bool _bLoading = true;

        public ArgumentValueBoolUI()
        {
            InitializeComponent();
        }

        public void SetUp(Argument argument, RequiredPropertyInfo requirement, DataTable previewIfAny)
        {
            _bLoading = true;
            _argument = argument;
            cbValue.Checked = (bool) argument.GetValueAsSystemType();
            _bLoading = false;
        }

        private void cbValue_CheckedChanged(object sender, System.EventArgs e)
        {
            if(_bLoading)
                return;

            _argument.SetValue(cbValue.Checked);
            _argument.SaveToDatabase();
        }
    }
}
