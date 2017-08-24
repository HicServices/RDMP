using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data.Cohort;
using CatalogueManager.ItemActivation;

namespace CohortManager.Wizard
{
    /// <summary>
    /// Part of CreateNewCohortIdentificationConfigurationUI.  Allows you to determine how the datasets in the CohortIdentificationConfiguration are combined to identify patients.  The options
    /// are 'Match All patients regardless of which dataset they appear in' and 'Match only patients who are in both datasets'.  If it is an Exclusion criteria then the result is what is subtracted
    /// from the matching cohorts (Inclusion criteria).  For example you could have Inclusion Criteria 'Anyone currently Resident in Tayside' and exclude 'Anyone who is dead' and 'Anyone who has
    /// ever been prescribed Aspirin', in this case the exclusion criteria would be 'Match All patients regardless of which dataset they appear in' since you want to throw out patients in either
    /// dataset.
    /// </summary>
    public partial class SimpleSetOperation : UserControl
    {
        private IActivateItems _activator;
        private string _unionText ;
        private string _intersectText = "Patients Must Appear In BOTH Datasets To Match ";
        private bool _isInclusionCriteria;

        public SimpleSetOperation()
        {
            InitializeComponent();
        }

        public void SetupFor(IActivateItems activator,bool isInclusionCriteria)
        {
            _activator = activator;
            _isInclusionCriteria = isInclusionCriteria;
            if (isInclusionCriteria)
            {
                _unionText = "Patients Are Included If they Appear In EITHER Dataset";
                _intersectText = "Patients Are Included ONLY If they Appear In BOTH Dataset";
            }
            else
            {
                _unionText = "Patients Are Excluded If they Appear In EITHER Dataset";
                _intersectText = "Patients Are Excluded ONLY If they Appear In BOTH Dataset";
            }

            ddSetOperation.Items.Clear();
            ddSetOperation.Items.Add(_intersectText);
            ddSetOperation.Items.Add(_unionText);
            ddSetOperation.SelectedIndex = 0;
        }

        private void ddSetOperation_SelectedIndexChanged(object sender, EventArgs e)
        {

            var op = GetSetOperation();
            pbSetOperation.Image = _activator.CoreIconProvider.GetImage(op);

        }

        private SetOperation GetSetOperation()
        {
            if((string) ddSetOperation.SelectedItem == _intersectText)
                return SetOperation.INTERSECT;

            return SetOperation.UNION;
        }

        public CohortAggregateContainer CreateCohortAggregateContainer(CohortAggregateContainer rootContainer)
        {
            var operation = GetSetOperation();

            if (rootContainer.Operation != SetOperation.EXCEPT)
                throw new ArgumentException("rootContainer");

            var container = new CohortAggregateContainer(_activator.RepositoryLocator.CatalogueRepository, operation);

            if (_isInclusionCriteria)
            {
                container.Order = 1;
                container.Name = operation + " - Inclusion Criteria";
            }
            else
            {
                container.Order = 2;
                container.Name = operation + " - Exclusion Criteria";    
            }

            container.SaveToDatabase();
            rootContainer.AddChild(container);

            return container;
        }
    }
}
