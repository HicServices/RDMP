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
using CatalogueManager.Collections;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;

namespace CohortManager.Wizard
{
    /// <summary>
    /// Part of SimpleFilterUI.  Allows you to specify the value of a given parameter of the filter.  There can be multiple parameters on a given filter (or none).  For example a filter
    /// 'Drug Prescribed' might have a parameter @drugName and another @amountPrescribed.  
    /// </summary>
    public partial class SimpleParameterUI : UserControl
    {
        private readonly IActivateItems _activator;
        private readonly ISqlParameter _parameter;

        public SimpleParameterUI(IActivateItems activator,ISqlParameter parameter)
        {
            _activator = activator;
            _parameter = parameter;
            InitializeComponent();

            lblParameterName.Text = parameter.ParameterName.TrimStart('@');
            pbParameter.Image = _activator.CoreIconProvider.GetImage(RDMPConcept.ParametersNode);

            tbValue.Text = parameter.Value;

            //move the text box to the right of the parameter name but make sure it is minimum visible
            tbValue.Left = Math.Min(Width - tbValue.Width,lblParameterName.Right);
        }

        public void SetValueTo(ExtractionFilterParameterSet set)
        {
            if (set == null)
                return;

            var correctValue = set.GetAllParameters().FirstOrDefault(p=>p.ParameterName.Equals(_parameter.ParameterName));

            if(correctValue == null)
            {
                tbValue.Text = "";
                return;
            }
            
            tbValue.Text = correctValue.Value;
        }

        public void HandleSettingParameters(IFilter filter)
        {
            //rename operations can have happened
            var parameterToSet = filter.GetAllParameters().Single(p => p.ParameterName.StartsWith(_parameter.ParameterName));
            parameterToSet.Value = tbValue.Text;
            parameterToSet.SaveToDatabase();
        }

        private void tbValue_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
