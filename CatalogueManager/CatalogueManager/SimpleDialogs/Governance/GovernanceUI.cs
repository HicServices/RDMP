using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data.Governance;
using CatalogueManager.SimpleDialogs.Revertable;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using ReusableUIComponents;

namespace CatalogueManager.SimpleDialogs.Governance
{
    /// <summary>
    /// The RDMP is designed to store sensitive clinical datasets and make them available in research ready (anonymous) form.  This usually requires governance approval from the data
    /// provider.  This control lets you create periods of governance for your datasets (See GovernancePeriodUI).
    /// </summary>
    public partial class GovernanceUI : RDMPForm
    {
        public GovernanceUI()
        {
            InitializeComponent();
            governancePeriodUI1.ChangesSaved += (sender, args) => RefreshFromUIDatabase();
            
            
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            if (VisualStudioDesignMode)
                return;

            RefreshFromUIDatabase();
        }

        public void RefreshFromUIDatabase()
        {
            ddGovernance.Items.Clear();
            ddGovernance.Items.AddRange(RepositoryLocator.CatalogueRepository.GetAllObjects<GovernancePeriod>().ToArray());
        }

        private void btnCreateNewGovernancePeriod_Click(object sender, EventArgs e)
        {
            var gov = new GovernancePeriod(RepositoryLocator.CatalogueRepository);
            RefreshFromUIDatabase();

            //select the newly created one
            ddGovernance.SelectedItem = ddGovernance.Items.Cast<GovernancePeriod>().Single(g => g.ID == gov.ID);
        }

        private void ddGovernance_SelectedIndexChanged(object sender, EventArgs e)
        {
            governancePeriodUI1.GovernancePeriod = ddGovernance.SelectedItem as GovernancePeriod;

            //if they have something selected allow it to be deleted
            btnDelete.Enabled = governancePeriodUI1.GovernancePeriod != null;
        }

        private void GovernanceUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            OfferChanceToSaveDialog.ShowIfRequired(ddGovernance.SelectedItem as GovernancePeriod);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            var gov = ddGovernance.SelectedItem as GovernancePeriod;

            if(gov != null)
                if(MessageBox.Show("Are you sure you want to delete governance period " + gov +"?","Confirm deleting governance period",MessageBoxButtons.YesNo) == DialogResult.Yes)
                {

                    try
                    {
                        gov.DeleteInDatabase();
                    }
                    catch (Exception exception)
                    {
                        ExceptionViewer.Show(exception);
                        return;//delete likely failed so don't bother reloading
                    }

                    //clear the edit pane
                    governancePeriodUI1.GovernancePeriod = null;
                    
                    //refresh the UI too
                    RefreshFromUIDatabase();
                }
        }
    }
}
