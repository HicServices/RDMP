// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Governance;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.ItemActivation.Emphasis;
using CatalogueManager.Rules;
using CatalogueManager.SimpleControls;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using MapsDirectlyToDatabaseTableUI;
using ReusableUIComponents;
using ReusableUIComponents.Dialogs;

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

        public GovernancePeriodUI()
        {
            InitializeComponent();
            AssociatedCollection = RDMPCollection.Catalogue;

            olvName.ImageGetter = s => Activator.CoreIconProvider.GetImage(s);
        }

        public override void SetDatabaseObject(IActivateItems activator, GovernancePeriod databaseObject)
        {
            base.SetDatabaseObject(activator, databaseObject);

            _governancePeriod = databaseObject;
            
            //clear related catalogues
            olvCatalogues.ClearObjects();

            ticketingControl1.TicketText = _governancePeriod.Ticket;

            lblExpired.Visible = _governancePeriod.IsExpired();

            dtpStartDate.Value = _governancePeriod.StartDate;

            if (_governancePeriod.EndDate == null)
                rbNeverExpires.Checked = true;
            else
            {
                rbExpiresOn.Checked = true;
                dtpEndDate.Value = (DateTime)_governancePeriod.EndDate;
            }

            //add related catalogues
            olvCatalogues.AddObjects(_governancePeriod.GovernedCatalogues.ToArray());

            CommonFunctionality.AddHelp(olvCatalogues, "GovernancePeriod.GovernedCatalogues");

            CommonFunctionality.AddChecks(_governancePeriod);
            CommonFunctionality.StartChecking();
        }

        protected override void SetBindings(BinderWithErrorProviderFactory rules, GovernancePeriod databaseObject)
        {
            base.SetBindings(rules, databaseObject);

            Bind(tbID, "Text", "ID", g => g.ID);
            Bind(tbName, "Text", "Name", g => g.Name);
            Bind(tbDescription, "Text", "Description", g => g.Description);
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

        private void btnAddCatalogue_Click(object sender, EventArgs e)
        {
            var alreadyMappedCatalogues = olvCatalogues.Objects.Cast<Catalogue>();
            var allCatalogues = Activator.RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>();

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

        private void AddCatalogue(ICatalogue c)
        {
            _governancePeriod.CreateGovernanceRelationshipTo(c);
            olvCatalogues.AddObject(c);
        }

        private void lbCatalogues_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                var toDelete = olvCatalogues.SelectedObject as Catalogue;

                if(toDelete != null)
                    if(MessageBox.Show("Are you sure you want to erase the fact that '" + _governancePeriod.Name + "' provides governance over Catalogue '" + toDelete + "'","Confirm Deleting Governance Relationship?",MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        _governancePeriod.DeleteGovernanceRelationshipTo(toDelete);
                        olvCatalogues.RemoveObject(toDelete);
                    }

                Publish(_governancePeriod);
            }
        }
        
        private void btnImportCatalogues_Click(object sender, EventArgs e)
        {
            GovernancePeriod[] toImportFrom = Activator.RepositoryLocator.CatalogueRepository.GetAllObjects<GovernancePeriod>()
                .Where(gov => gov.ID != _governancePeriod.ID)
                .ToArray();

            if (!toImportFrom.Any())
            {
                MessageBox.Show("You do not have any other GovernancePeriods in your Catalogue");
                return;
            }
            
            SelectIMapsDirectlyToDatabaseTableDialog dialog = new SelectIMapsDirectlyToDatabaseTableDialog(toImportFrom,false,false);

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                ICatalogue[] toAdd = ((GovernancePeriod) dialog.Selected).GovernedCatalogues.ToArray();

                //do not add any we already have
                toAdd = toAdd.Except(olvCatalogues.Objects.Cast<Catalogue>()).ToArray();

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

        private void tbFilter_TextChanged(object sender, EventArgs e)
        {
            olvCatalogues.UseFiltering = true;
            olvCatalogues.ModelFilter = new TextMatchFilter(olvCatalogues,tbFilter.Text);
        }

        private void olvCatalogues_ItemActivate(object sender, EventArgs e)
        {
            var cata = olvCatalogues.SelectedObject as Catalogue;
            if(cata != null)
                Activator.RequestItemEmphasis(this,new EmphasiseRequest(cata));
        }

    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<GovernancePeriodUI_Design, UserControl>))]
    public abstract class GovernancePeriodUI_Design : RDMPSingleDatabaseObjectControl<GovernancePeriod>
    {
    }
}
