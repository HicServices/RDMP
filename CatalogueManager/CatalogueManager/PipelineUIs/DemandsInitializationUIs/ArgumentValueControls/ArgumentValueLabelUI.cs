using System.Windows.Forms;
using ReusableUIComponents;

namespace CatalogueManager.PipelineUIs.DemandsInitializationUIs.ArgumentValueControls
{
    /// <summary>
    /// Normally IArgumentValueUIs allows you to specify the value of an IArugment (the database persistence value of a [DemandsInitialization] decorated Property on a MEF class e.g. a Pipeline components public property that the user can set)
    /// 
    /// <para>But in the case of this control the Type is not user editable but will be populated (hopefully) by the RDMP automatically e.g. CatalogueRepository.  In this case this control
    /// will display to the user some information about why he cannot specify a value for the IArgument.</para>
    /// </summary>
    [TechnicalUI]
    public partial class ArgumentValueLabelUI : UserControl, IArgumentValueUI
    {
        public ArgumentValueLabelUI(string readonlyText)
        {
            InitializeComponent();

            lbl.Text = readonlyText;
        }

        public void SetUp(ArgumentValueUIArgs args)
        {
            
        }
    }
}
