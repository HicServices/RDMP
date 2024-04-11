// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.FilterImporting.Construction;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.UI.ItemActivation;

namespace Rdmp.UI.Wizard;

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

    private List<SimpleFilterUI> _filterUIs = new();

    public SimpleCohortSetUI()
    {
        InitializeComponent();

        _linkImage = FamFamFamIcons.link.ImageToBitmap();
        _unlinkImage = FamFamFamIcons.link_break.ImageToBitmap();

        cbxColumns.PropertySelector = collection => collection.Cast<ExtractionInformation>().Select(i => i.ToString());

        btnLockExtractionIdentifier.Image = _linkImage;
        ddAndOr.DataSource = Enum.GetValues(typeof(FilterContainerOperation));

        btnDelete.Visible = false;
    }

    public void SetupFor(IActivateItems activator)
    {
        _activator = activator;
        cbxCatalogues.SetUp(activator.CoreChildProvider.AllCatalogues.Value);
        pbCatalogue.Image = activator.CoreIconProvider.GetImage(RDMPConcept.Catalogue).ImageToBitmap();
        pbExtractionIdentifier.Image =
            activator.CoreIconProvider.GetImage(RDMPConcept.ExtractionInformation).ImageToBitmap();
        pbFilters.Image = activator.CoreIconProvider.GetImage(RDMPConcept.Filter).ImageToBitmap();
        cbxCatalogues.SetItemActivator(activator);
    }

    private Catalogue _lastCatalogue;

    public ICatalogue Catalogue => cbxCatalogues.SelectedItem as Catalogue;

    private void cbxCatalogues_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (cbxCatalogues.SelectedItem is not Catalogue cata)
            return;

        //if the Catalogue changes clear the old filters because they apply to the last dataset
        if (_lastCatalogue != null)
            if (!Equals(_lastCatalogue, cata)) //catalogue has changed
            {
                //if there are no user specified ones clear the mandatory filters (if there are any)
                if (_filterUIs.All(f => f.Mandatory))
                {
                    ClearFilters();
                }
                else //there are some user specified ones
                if (
                    //confirm with user before we erase these
                    _activator.YesNo(
                        "Changing the dataset will clear the Filters you have configured, is this what you want?",
                        "Change Dataset"))
                {
                    ClearFilters();
                }
                else
                {
                    cbxCatalogues.SelectedItem = _lastCatalogue;
                    return;
                }
            }

        //add any mandatory filters that are not yet part of the configuration
        foreach (var mandatoryFilter in cata.GetAllMandatoryFilters())
            if (!_filterUIs.Any(f => f.Filter.Equals(mandatoryFilter)))
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
        {
            UnlockIdentifier(null);
        }
        else if (identifiers.Length == 1)
        {
            LockIdentifier(identifiers[0]); //there is only one IsExtractionIdentifier so lock it in automatically
        }
        else
        {
            _knownIdentifiersMode = true;

            //if there are multiple columns marked with IsExtractionIdentifier then we show all of them but no others
            //e.g. SMR02 where there could be Mother CHI, Baby CHI and Father CHI do not unmark extraction identifier just because they changed it
            cbxColumns.DataSource = identifiers;
            UnlockIdentifier(null);
        }

        var allFilters = allColumns.SelectMany(c => c.ExtractionFilters).ToArray();
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
        if (!extractionInformation.IsExtractionIdentifier)
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
        if (ddAvailableFilters.SelectedItem is not ExtractionFilter f)
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
        cbxCatalogues.SelectedItem = null;
    }

    public void CreateCohortSet(CohortAggregateContainer targetContainer)
    {
        if (cbxCatalogues.SelectedItem is not Catalogue cata)
            return;

        var cataCommand = new CatalogueCombineable(cata)
        {
            //use this one
            ResolveMultipleExtractionIdentifiers = (s, e) => cbxColumns.SelectedItem as ExtractionInformation
        };

        var cmd = new ExecuteCommandAddCatalogueToCohortIdentificationSetContainer(_activator, cataCommand,
            targetContainer)
        {
            SkipMandatoryFilterCreation = true
        };
        cmd.Execute();

        var aggregate = cmd.AggregateCreatedIfAny;

        var filterOp = (FilterContainerOperation)ddAndOr.SelectedItem;

        IContainer filterContainer;
        if (aggregate.RootFilterContainer_ID != null)
        {
            //this is the case if there are mandatory filters in the dataset
            filterContainer = aggregate.RootFilterContainer;
            filterContainer.Operation = filterOp;
            filterContainer.SaveToDatabase();
        }
        else if (_filterUIs.Count > 0)
        {
            filterContainer = new AggregateFilterContainer(_activator.RepositoryLocator.CatalogueRepository, filterOp);
            aggregate.RevertToDatabaseState();
            aggregate.RootFilterContainer_ID = filterContainer.ID;
            aggregate.SaveToDatabase();

            var filtersAddedSoFar = new List<IFilter>();
            foreach (var ui in _filterUIs)
            {
                var f = ui.CreateFilter(new AggregateFilterFactory(_activator.RepositoryLocator.CatalogueRepository),
                    filterContainer, filtersAddedSoFar.ToArray());
                filtersAddedSoFar.Add(f);
            }
        }
    }

    private void ddAndOr_SelectedIndexChanged(object sender, EventArgs e)
    {
    }
}