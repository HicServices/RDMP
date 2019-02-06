// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

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
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.FilterImporting.Construction;
using CatalogueLibrary.Providers;
using CatalogueManager.CommandExecution;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Data.DataTables;
using CatalogueManager.Copying.Commands;

namespace CohortManager.Wizard
{
    /// <summary>
    /// Part of CreateNewCohortIdentificationConfigurationUI.  Allows you to modify how a dataset Catalogue is filtered to identify patients.  Selecting prescribing will result in a cohort
    /// set of 'everyone appearing in the prescribing dataset', if you add a filter on drug prescribed to 'Paracetamol' then the set will be 'everyone who has ever been presdcribed Paracetamol'.
    /// </summary>
    public partial class SimpleCohortSetUI : UserControl
    {
        //constant things
        private Bitmap _linkImage;
        private Bitmap _unlinkImage;
        private IActivateItems _activator;

        //dynamic things
        private bool lockIn = true;
        private bool _knownIdentifiersMode;

        private List<SimpleFilterUI> _filterUIs = new List<SimpleFilterUI>();

        public SimpleCohortSetUI()
        {
            InitializeComponent();

            _linkImage = FamFamFamIcons.link;
            _unlinkImage = FamFamFamIcons.link_break;

            cbxCatalogues.PropertySelector = collection => collection.Cast<Catalogue>().Select(c => c.Name);
            cbxColumns.PropertySelector = collection => collection.Cast<ExtractionInformation>().Select(i => i.ToString());

            btnLockExtractionIdentifier.Image = _linkImage;
            ddAndOr.DataSource = Enum.GetValues(typeof (FilterContainerOperation));

            btnDelete.Visible = false;
        }

        public void SetupFor(IActivateItems activator)
        {
            _activator = activator;
            cbxCatalogues.DataSource = activator.CoreChildProvider.AllCatalogues;
            pbCatalogue.Image = activator.CoreIconProvider.GetImage(RDMPConcept.Catalogue);
            pbExtractionIdentifier.Image = activator.CoreIconProvider.GetImage(RDMPConcept.ExtractionInformation);
            pbFilters.Image = activator.CoreIconProvider.GetImage(RDMPConcept.Filter);
        }

        private Catalogue _lastCatalogue = null;
        private void cbxCatalogues_SelectedIndexChanged(object sender, EventArgs e)
        {
            var cata = cbxCatalogues.SelectedItem as Catalogue;

            if (cata == null)
                return;

            //if the Catalogue changes clear the old filters because they apply to the last dataset
            if(_lastCatalogue != null)
                if(!Equals(_lastCatalogue, cata))//catalogue has changed
                {
                    //if there are no user specified ones clear the mandatory filters (if there are any)
                    if (_filterUIs.All(f => f.Mandatory))
                        ClearFilters();
                    else //there are some user specified ones
                        if (
                            //confirm with user before we erase these
                            MessageBox.Show(
                                "Changing the dataset will clear the Filters you have configured, is this what you want?",
                                "Change Dataset", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            ClearFilters();
                        else
                        {
                            cbxCatalogues.SelectedItem = _lastCatalogue;
                            return;
                        }
                }

            //add any mandatory filters that are not yet part of the configuration
            foreach (var mandatoryFilter in cata.GetAllMandatoryFilters())
                if(!_filterUIs.Any(f=>f.Filter.Equals(mandatoryFilter)))
                {
                    var m = AddFilter(mandatoryFilter);
                    m.Mandatory = true; //mandatory ones cannot be changed or gotten rid of
                }

            //set the columns
            var allColumns = cata.GetAllExtractionInformation(ExtractionCategory.Any).ToArray();

            cbxColumns.DataSource = allColumns;

            var identifiers = allColumns.Where(ei => ei.IsExtractionIdentifier).ToArray();

            //if no columns have yet been marked as IsExtractionIdentifier then we show all columns and demand that the user lock one in
            if (identifiers.Length == 0)
                UnlockIdentifier(null);
            else
            if (identifiers.Length == 1)
                LockIdentifier(identifiers[0]);//there is only one IsExtractionIdentifier so lock it in automatically
            else
            {
                _knownIdentifiersMode = true;

                //if there are multiple columns marked with IsExtractionIdentifier then we show all of them but no others
                //e.g. SMR02 where there could be Mother CHI, Baby CHI and Father CHI do not unmark extraction identifier just because they changed it
                cbxColumns.DataSource = identifiers;
                UnlockIdentifier(null);
            }

            var allFilters = _activator.RepositoryLocator.CatalogueRepository.GetAllObjects<ExtractionFilter>("WHERE ExtractionInformation_ID IN (" + string.Join(",", allColumns.Select(c => c.ID.ToString())) + ")");
            ddAvailableFilters.DataSource = allFilters;

            _lastCatalogue = cata;
        }

        private void ClearFilters()
        {
            _filterUIs.Clear();
            tableLayoutPanel1.Controls.Clear();
            ddAndOr.Enabled = false;
        }

        private void LockIdentifier(ExtractionInformation extractionInformation)
        {
            cbxColumns.Enabled = false;
            cbxColumns.SelectedItem = extractionInformation;
            btnLockExtractionIdentifier.Image = _unlinkImage;

            //if it isn't yet marked as an extraction identifier save it as one for next time
            if(!extractionInformation.IsExtractionIdentifier)
            {
                extractionInformation.IsExtractionIdentifier = true;
                extractionInformation.SaveToDatabase();
            }

            lockIn = false;
        }

        private void UnlockIdentifier(ExtractionInformation currentSelectionIfAny)
        {
            cbxColumns.Enabled = true;
            cbxColumns.SelectedItem = null;

            //if there are multiple extraction identifiers e.g. SMR02 where there could be Mother CHI, Baby CHI and Father CHI do not unmark extraction identifier just because they changed it
            if (!_knownIdentifiersMode && currentSelectionIfAny != null)
            {
                currentSelectionIfAny.IsExtractionIdentifier = false;
                currentSelectionIfAny.SaveToDatabase();
            }

            btnLockExtractionIdentifier.Image = _linkImage;
            lockIn = true;
        }

        private void btnLockExtractionIdentifier_Click(object sender, EventArgs e)
        {
            if (lockIn)
                LockIdentifier(cbxColumns.SelectedItem as ExtractionInformation);
            else
                UnlockIdentifier(cbxColumns.SelectedItem as ExtractionInformation);
        }

        private void btnAddFilter_Click(object sender, EventArgs e)
        {

            var f = ddAvailableFilters.SelectedItem as ExtractionFilter;

            if(f == null)
                return;

            AddFilter(f);

        }

        private SimpleFilterUI AddFilter(ExtractionFilter f)
        {
            var filterUI = new SimpleFilterUI(_activator, f);
            filterUI.RequestDeletion += () => RemoveFilter(filterUI);

            filterUI.Width = tableLayoutPanel1.Width;
            filterUI.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

            tableLayoutPanel1.Controls.Add(filterUI, tableLayoutPanel1.RowCount - 1, 0);

            //this array always seems to be 1 element long..
            tableLayoutPanel1.RowStyles[0].SizeType = SizeType.AutoSize;

            _filterUIs.Add(filterUI);

            //if there are 2+ filters then user can specify AND / OR to combine them
            ddAndOr.Enabled = _filterUIs.Count >= 2;

            return filterUI;
        }

        private void RemoveFilter(SimpleFilterUI filterUI)
        {
            _filterUIs.Remove(filterUI);
            tableLayoutPanel1.Controls.Remove(filterUI);
        }

        public void Clear()
        {
            cbxCatalogues.DataSource = null;
        }

        public void CreateCohortSet(CohortIdentificationConfiguration cic, CohortAggregateContainer targetContainer, int order)
        {
            var cata = cbxCatalogues.SelectedItem as Catalogue;
            
            if(cata == null)
                throw new Exception("Catalogue has not been picked!");

            var cataCommand = new CatalogueCommand(cata);
            //use this one
            cataCommand.ResolveMultipleExtractionIdentifiers = (s, e) => cbxColumns.SelectedItem as ExtractionInformation;

            var cmd = new ExecuteCommandAddCatalogueToCohortIdentificationSetContainer(_activator,cataCommand ,targetContainer);
            cmd.SkipMandatoryFilterCreation = true;
            cmd.Execute();

            var aggregate = cmd.AggregateCreatedIfAny;
            
            var filterOp = (FilterContainerOperation) ddAndOr.SelectedItem;

            AggregateFilterContainer filterContainer;
            if (aggregate.RootFilterContainer_ID != null)
            {
                //this is the case if there are mandatory filters in the dataset
                filterContainer = aggregate.RootFilterContainer;
                filterContainer.Operation = filterOp;
                filterContainer.SaveToDatabase();
            }
            else
                filterContainer = new AggregateFilterContainer(_activator.RepositoryLocator.CatalogueRepository, filterOp);

            aggregate.Order = order;
            aggregate.RootFilterContainer_ID = filterContainer.ID;
            aggregate.SaveToDatabase();

            List<IFilter> filtersAddedSoFar = new List<IFilter>();
            foreach (var ui in _filterUIs)
            {
                var f = ui.CreateFilter(new AggregateFilterFactory(_activator.RepositoryLocator.CatalogueRepository), filterContainer, filtersAddedSoFar.ToArray());
                filtersAddedSoFar.Add(f);
            }
        }

        private void ddAndOr_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
