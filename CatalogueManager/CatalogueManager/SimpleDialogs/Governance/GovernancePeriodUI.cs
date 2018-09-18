using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Governance;
using CatalogueManager.ItemActivation;
using CatalogueManager.SimpleControls;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using MapsDirectlyToDatabaseTableUI;
using ReusableUIComponents;

namespace CatalogueManager.SimpleDialogs.Governance
{
    /// <summary>
    /// The RDMP is designed to store sensitive clinical datasets and make them available in research ready (anonymous) form.  This usually requires governance approval from the data
    /// provider.  This control lets you configure a period of governance (GovernancePeriod) which can be open ended (never expires).  You must then choose which datasets (Catalogues)
    /// the governance permission applies to.  Finally you can attach documents that prove the permission (See GovernanceDocumentUI).
    /// 
    /// <para>You should make sure you name and describe the governance period.  The name should correspond to the period.  For example you might have 3 periods 'Fife approvals 2001-2002', 
    /// 'Fife approvals 2002-2003' and 'Fife open ended approvals 2003-Forever'.  </para>
    /// 
    /// <para>If you are doing yearly approvals you can import the dataset list from the last year as the basis of governanced datasets.</para>
    /// 
    /// <para>If a GovernancePeriod expires all datasets (Catalogues) in the period will be assumed to have expired governance and will appear in the Dashboard as expired unless there is a new
    /// GovernancePeriod that is active (See GovernanceSummary).</para>
    /// </summary>
    public partial class GovernancePeriodUI : GovernancePeriodUI_Design,ISaveableUI
    {
        private GovernancePeriod _governancePeriod;
        public event EventHandler ChangesSaved;

        public GovernancePeriod GovernancePeriod
        {
            get { return _governancePeriod; }
            set
            {
                _governancePeriod = value;

                this.Enabled = value != null;

                //clear related catalogues
                lbCatalogues.Items.Clear();
                
                if (value == null)
                {
                    tbName.Text = "";
                    tbDescription.Text = "";
                    ticketingControl1.TicketText = null;
                }
                else
                {
                    tbName.Text = value.Name;
                    tbDescription.Text = value.Description;
                    ticketingControl1.TicketText = value.Ticket;

                    dtpStartDate.Value = value.StartDate;

                    if (value.EndDate == null)
                        rbNeverExpires.Checked = true;
                    else
                    {
                        rbExpiresOn.Checked = true;
                        dtpEndDate.Value = (DateTime) value.EndDate;
                    }
                    
                    //add related catalogues
                    lbCatalogues.Items.AddRange(value.GovernedCatalogues.ToArray());
                }
            }
        }

        public GovernancePeriodUI()
        {
            InitializeComponent();
        }

        public override void SetDatabaseObject(IActivateItems activator, GovernancePeriod databaseObject)
        {
            base.SetDatabaseObject(activator, databaseObject);

            GovernancePeriod = databaseObject;
            objectSaverButton1.SetupFor(databaseObject, activator.RefreshBus);
        }

        private void tbName_TextChanged(object sender, EventArgs e)
        {
            if (_governancePeriod != null)
                _governancePeriod.Name = tbName.Text;

        }

        private void tbDescription_TextChanged(object sender, EventArgs e)
        {
            if (_governancePeriod != null)
                _governancePeriod.Description = tbDescription.Text;
        }

        private void rbNeverExpires_CheckedChanged(object sender, EventArgs e)
        {
            dtpEndDate.Enabled = !rbNeverExpires.Checked;

            if (_governancePeriod != null)
                if (rbNeverExpires.Checked)
                    _governancePeriod.EndDate = null; //user changed to never expiry
                else
                    _governancePeriod.EndDate = dtpEndDate.Value;
        }

        private void dtpStartDate_ValueChanged(object sender, EventArgs e)
        {
            if (_governancePeriod != null)
                _governancePeriod.StartDate = dtpStartDate.Value;
        }

        private void dtpEndDate_ValueChanged(object sender, EventArgs e)
        {
            if(_governancePeriod != null)
                if(rbExpiresOn.Checked)
                    _governancePeriod.EndDate = dtpEndDate.Value;
        }

        private void rbExpiresOn_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void btnAddCatalogue_Click(object sender, EventArgs e)
        {
            
            var alreadyMappedCatalogues = lbCatalogues.Items.Cast<Catalogue>();
            var allCatalogues = RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>();

            var availableToSelect =
                allCatalogues.Where(c => !alreadyMappedCatalogues.Contains(c)).ToArray();

            SelectIMapsDirectlyToDatabaseTableDialog selector = new SelectIMapsDirectlyToDatabaseTableDialog(availableToSelect, false, false);
            selector.AllowMultiSelect = true;

            if (selector.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    foreach(Catalogue c in selector.MultiSelected)
                        AddCatalogue(c);
                }
                catch (Exception ex)
                {
                    ExceptionViewer.Show("Could not add relationship to Catalogue:" + selector.Selected,ex);
                }

                Publish(_governancePeriod);
            }
            
        }

        private void AddCatalogue(Catalogue c)
        {
            _governancePeriod.CreateGovernanceRelationshipTo(c);
            lbCatalogues.Items.Add(c);
        }

        private void lbCatalogues_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                var toDelete = lbCatalogues.SelectedItem as Catalogue;

                if(toDelete != null)
                    if(MessageBox.Show("Are you sure you want to erase the fact that '" + _governancePeriod.Name + "' provides governance over Catalogue '" + toDelete + "'","Confirm Deleting Governance Relationship?",MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        _governancePeriod.DeleteGovernanceRelationshipTo(toDelete);
                        lbCatalogues.Items.Remove(toDelete);
                    }

                Publish(GovernancePeriod);
            }
        }
        
        private void btnImportCatalogues_Click(object sender, EventArgs e)
        {
            GovernancePeriod[] toImportFrom = RepositoryLocator.CatalogueRepository.GetAllObjects<GovernancePeriod>()
                .Where(gov=>gov.ID != GovernancePeriod.ID)
                .ToArray();

            if (!toImportFrom.Any())
            {
                MessageBox.Show("You do not have any other GovernancePeriods in your Catalogue");
                return;
            }
            
            SelectIMapsDirectlyToDatabaseTableDialog dialog = new SelectIMapsDirectlyToDatabaseTableDialog(toImportFrom,false,false);

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Catalogue[] toAdd = ((GovernancePeriod) dialog.Selected).GovernedCatalogues.ToArray();

                //do not add any we already have
                toAdd = toAdd.Except(lbCatalogues.Items.Cast<Catalogue>()).ToArray();

                if (!toAdd.Any())
                    MessageBox.Show("Selected GovernancePeriod '" + dialog.Selected +
                                    "' does not govern any novel Catalogues (Catalogues already in your configuration are not repeat imported)");
                else
                {
                    foreach (var c in toAdd)
                        AddCatalogue(c);

                    Publish(_governancePeriod);
                }
            }
        }

        public ObjectSaverButton GetObjectSaverButton()
        {
            return objectSaverButton1;
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<GovernancePeriodUI_Design, UserControl>))]
    public abstract class GovernancePeriodUI_Design : RDMPSingleDatabaseObjectControl<GovernancePeriod>
    {
    }
}
