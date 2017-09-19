using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using ReusableUIComponents;

namespace RDMPObjectVisualisation.DemandsInitializationUIs.ArgumentValueControls
{
    /// <summary>
    /// Normally IArgumentValueUIs allows you to specify the value of an IArugment (the database persistence value of a [DemandsInitialization] decorated Property on a MEF class e.g. a Pipeline components public property that the user can set)
    /// 
    /// But in the case of this control the Type is not user editable but will be populated (hopefully) by the RDMP automatically e.g. CatalogueRepository.  In this case this control
    /// will display to the user some information about why he cannot specify a value for the IArgument.
    /// </summary>
    [TechnicalUI]
    public partial class ArgumentValueLabelUI : UserControl, IArgumentValueUI
    {
        public ArgumentValueLabelUI(string readonlyText)
        {
            InitializeComponent();

            lbl.Text = readonlyText;
        }

        public void SetUp(Argument argument, RequiredPropertyInfo requirement, DataTable previewIfAny)
        {
            
        }
    }
}
