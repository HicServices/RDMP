using System.Windows.Forms;
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
        private bool _bLoading = true;
        private ArgumentValueUIArgs _args;

        public ArgumentValueBoolUI()
        {
            InitializeComponent();
        }
        
        public void SetUp(ArgumentValueUIArgs args)
        {
            _args = args;
            _bLoading = true;
            cbValue.Checked = (bool)args.InitialValue;
            _bLoading = false;
        }

        private void cbValue_CheckedChanged(object sender, System.EventArgs e)
        {
            if(_bLoading)
                return;

            _args.Setter(cbValue.Checked);
        }
    }
}
