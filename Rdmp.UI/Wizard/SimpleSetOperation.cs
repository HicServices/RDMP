// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Windows.Forms;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.Icons;

namespace Rdmp.UI.Wizard
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
        private string _intersectText = "Records Must Appear In BOTH Datasets To Match ";
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
                _unionText = "Records Are Included If they Appear In EITHER Dataset";
                _intersectText = "Records Are Included ONLY If they Appear In BOTH Dataset";
            }
            else
            {
                _unionText = "Records Are Excluded If they Appear In EITHER Dataset";
                _intersectText = "Records Are Excluded ONLY If they Appear In BOTH Dataset";
            }

            ddSetOperation.Items.Clear();
            ddSetOperation.Items.Add(_unionText);
            ddSetOperation.Items.Add(_intersectText);
            ddSetOperation.SelectedIndex = 0;
        }

        private void ddSetOperation_SelectedIndexChanged(object sender, EventArgs e)
        {

            var op = GetSetOperation();
            pbSetOperation.Image = _activator.CoreIconProvider.GetImage(op).ImageToBitmap();

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
                container.Name = "Inclusion Criteria";
            }
            else
            {
                container.Order = 2;
                container.Name = "Exclusion Criteria";    
            }

            container.SaveToDatabase();
            rootContainer.AddChild(container);

            return container;
        }
    }
}
