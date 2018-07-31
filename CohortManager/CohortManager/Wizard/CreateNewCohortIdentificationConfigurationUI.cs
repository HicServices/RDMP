using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data.Cohort;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;

namespace CohortManager.Wizard
{
    /// <summary>
    /// Provides streamlined/simplified access to the cohort creation functionality of RDMP.  The UI lets you pick from existing datasets and existing filters created by your Data Manager for
    /// interacting with those datasets including specifying values for arguments e.g. TestCode = 'CRE'.  
    /// 
    /// <para>Initially you are limited to 2 inclusion criteria and 2 exclsuion criteria (datasets).  Upon completing the wizard you will be taken to the execution screen of the Cohort Identification
    /// Configuration created (See CohortCompilerUI).  There you can test/refine your configuration as well as add more datasets and deeper nesting of set operations as required.</para>
    /// 
    /// </summary>
    public partial class CreateNewCohortIdentificationConfigurationUI : Form
    {
        private readonly IActivateItems _activator;
        public CohortIdentificationConfiguration CohortIdentificationCriteriaCreatedIfAny { get;private set; }

        public CreateNewCohortIdentificationConfigurationUI(IActivateItems activator)
        {
            _activator = activator;
            InitializeComponent();

            pbBigImageTopLeft.Image = CatalogueIcons.BigCohort;

            inclusionCriteria1.SetupFor(_activator);

            setOperationInclude.SetupFor(_activator, true);
            setOperationExclude.SetupFor(_activator,false);
        }



        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var mid = lblInclusionCriteria.Right + ((lblExclusionCriteria.Left - lblInclusionCriteria.Right) * 0.888f);

            var top = Math.Max(lblExclusionCriteria.Top, lblInclusionCriteria.Top);

            e.Graphics.DrawLine(Pens.Black, mid, top, mid, top + 200);
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
                target.SetupFor(_activator);
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

            if(MessageBox.Show("Are you sure you are happy with your configuration, this wizard will close after creating?","Confirm",MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;

            var cic = new CohortIdentificationConfiguration(_activator.RepositoryLocator.CatalogueRepository, tbName.Text);
            
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

            _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(cic));

            CohortIdentificationCriteriaCreatedIfAny = cic;
            DialogResult = DialogResult.OK;

            Close();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
