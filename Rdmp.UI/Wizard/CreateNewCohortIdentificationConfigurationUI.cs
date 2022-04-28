// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Windows.Forms;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

namespace Rdmp.UI.Wizard
{
    /// <summary>
    /// Provides streamlined/simplified access to the cohort creation functionality of RDMP.  The UI lets you pick from existing datasets and existing filters created by your Data Manager for
    /// interacting with those datasets including specifying values for arguments e.g. TestCode = 'CRE'.  
    /// 
    /// <para>Initially you are limited to 2 inclusion criteria and 2 exclsuion criteria (datasets).  Upon completing the wizard you will be taken to the execution screen of the Cohort Identification
    /// Configuration created.  There you can test/refine your configuration as well as add more datasets and deeper nesting of set operations as required.</para>
    /// 
    /// </summary>
    public partial class CreateNewCohortIdentificationConfigurationUI : RDMPForm
    {
        public CohortIdentificationConfiguration CohortIdentificationCriteriaCreatedIfAny { get;private set; }

        public CreateNewCohortIdentificationConfigurationUI(IActivateItems activator):base(activator)
        {
            InitializeComponent();

            if(VisualStudioDesignMode)
                return;

            inclusionCriteria1.SetupFor(Activator);

            setOperationInclude.SetupFor(Activator, true);
            setOperationExclude.SetupFor(Activator, false);
        }



        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            /*var mid = lblInclusionCriteria.Right + ((lblExclusionCriteria.Left - lblInclusionCriteria.Right) * 0.888f);

            var top = Math.Max(lblExclusionCriteria.Top, lblInclusionCriteria.Top);

            e.Graphics.DrawLine(Pens.Black, mid, top, mid, top + 200);*/
        }

        private void CheckBoxChanged(object sender, EventArgs e)
        {
            SimpleCohortSetUI target = null;

            if (sender == cbExclusion1)
                target = exclusionCriteria1;
            if (sender == cbExclusion2)
                target = exclusionCriteria2;
            if (sender == cbInclusion2)
                target = inclusionCriteria2;

            if(target == null)
                throw new ArgumentException("sender");

            if(((CheckBox)sender).Checked)
            {
                target.SetupFor(Activator);
                target.Enabled = true;
            }
            else
            {
                target.Clear();
                target.Enabled = false;
            }

            setOperationInclude.Enabled = cbInclusion2.Checked;
            setOperationExclude.Enabled = cbExclusion1.Checked && cbExclusion2.Checked;

        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbName.Text))
            {
                MessageBox.Show("Enter a name for your Cohort Identification Criteria");
                return;
            }
            
            if(!Activator.YesNo("Are you sure you are happy with your configuration, this wizard will close after creating?","Confirm"))
                return;

            var cic = new CohortIdentificationConfiguration(Activator.RepositoryLocator.CatalogueRepository, tbName.Text);
            
            cic.CreateRootContainerIfNotExists();
            var root = cic.RootCohortAggregateContainer;
            root.Operation = SetOperation.EXCEPT;
            root.Name = "EXCEPT";
            root.SaveToDatabase();
            
            var includeContainer = setOperationInclude.CreateCohortAggregateContainer(root);
            

            inclusionCriteria1.CreateCohortSet(cic,includeContainer, 1);
            
            if (cbInclusion2.Checked)
                inclusionCriteria2.CreateCohortSet(cic, includeContainer, 2);

            if(cbExclusion1.Checked || cbExclusion2.Checked)
            {
                var excludeContainer = setOperationExclude.CreateCohortAggregateContainer(root);

                if (cbExclusion1.Checked)
                    exclusionCriteria1.CreateCohortSet(cic, excludeContainer, 1);

                if (cbExclusion2.Checked)
                    exclusionCriteria2.CreateCohortSet(cic, excludeContainer, 2);
            }

            Activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(cic));

            CohortIdentificationCriteriaCreatedIfAny = cic;
            DialogResult = DialogResult.OK;

            Close();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
