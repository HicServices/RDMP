// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.FilterImporting;
using Rdmp.Core.Curation.FilterImporting.Construction;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using IContainer = Rdmp.Core.Curation.Data.IContainer;

namespace Rdmp.UI.Wizard;

/// <summary>
/// Part of CreateNewCohortIdentificationConfigurationUI.  Allows you to view and edit the parameters (if any) of a Filter you have added (or was Mandatory) on a dataset.  For example if
/// you have a Filter 'Drug Prescribed' on the dataset 'Prescribing' typing "'Paracetamol'" into the parameter will likely restrict the cohort to matching only those patients who have ever
/// been prescribed Paracetamol.
/// 
/// <para>If the control is Readonly (disabled / greyed out) then it is probably a Mandatory filter on your dataset and you will not be able to remove it.</para>
/// 
/// <para>This UI is a simplified version of ExtractionFilterUI</para>
/// </summary>
public partial class SimpleFilterUI : UserControl
{
    private readonly IActivateItems _activator;
    private readonly ExtractionFilter _filter;

    public event Action RequestDeletion;

    private int rowHeight = 30;

    public IFilter Filter => _filter;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool Mandatory
    {
        get => _mandatory;
        set
        {
            _mandatory = value;

            if (value)
            {
                btnDelete.Enabled = false;
                lblFilterName.Enabled = false;
            }
            else
            {
                btnDelete.Enabled = true;
                lblFilterName.Enabled = true;
            }
        }
    }

    private List<SimpleParameterUI> parameterUis = new();
    private bool _mandatory;

    public SimpleFilterUI(IActivateItems activator, ExtractionFilter filter)
    {
        _activator = activator;
        _filter = filter;
        InitializeComponent();

        lblFilterName.Text = filter.Name;
        pbFlter.Image = activator.CoreIconProvider.GetImage(RDMPConcept.Filter).ImageToBitmap();

        var parameters = filter.ExtractionFilterParameters.ToArray();

        SetupKnownGoodValues();

        for (var i = 0; i < parameters.Length; i++)
        {
            var currentRowPanel = new Panel
            {
                Bounds = new Rectangle(0, 0, tableLayoutPanel1.Width, rowHeight),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Margin = Padding.Empty
            };

            var p = new SimpleParameterUI(activator, parameters[i]);
            currentRowPanel.Controls.Add(p);
            p.tbValue.TextChanged += (s, e) =>
            {
                //we are here because user is selecting a value from the dropdown not because he is editing the text field manually
                if (_settingAKnownGoodValue)
                    return;

                //user is manually editing a Parameters so it no longer matches a Known value
                ddKnownGoodValues.SelectedItem = "";
            };
            parameterUis.Add(p);

            tableLayoutPanel1.Controls.Add(currentRowPanel, 0, i + 1);
            tableLayoutPanel1.CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset;
        }

        Height = 50 + parameters.Length * rowHeight;
    }

    private void SetupKnownGoodValues()
    {
        var knownGoodValues = _activator.RepositoryLocator.CatalogueRepository
            .GetAllObjectsWithParent<ExtractionFilterParameterSet>(_filter);

        if (knownGoodValues.Any())
        {
            pbKnownValueSets.Visible = true;
            ddKnownGoodValues.Visible = true;

            var l = new List<object> { "" };
            l.AddRange(knownGoodValues);

            ddKnownGoodValues.DataSource = l;
            pbKnownValueSets.Image = _activator.CoreIconProvider.GetImage(RDMPConcept.ExtractionFilterParameterSet)
                .ImageToBitmap();

            pbKnownValueSets.Left = lblFilterName.Right;
            ddKnownGoodValues.Left = pbKnownValueSets.Right;
        }
        else
        {
            pbKnownValueSets.Visible = false;
            ddKnownGoodValues.Visible = false;
        }
    }

    private void btnDelete_Click(object sender, EventArgs e)
    {
        RequestDeletion?.Invoke();
    }

    private void lblFilterName_Click(object sender, EventArgs e)
    {
    }

    private bool _settingAKnownGoodValue;

    private void ddKnownGoodValues_SelectedIndexChanged(object sender, EventArgs e)
    {
        var set = ddKnownGoodValues.SelectedItem as ExtractionFilterParameterSet;

        _settingAKnownGoodValue = true;
        foreach (var p in parameterUis)
            p.SetValueTo(set);
        _settingAKnownGoodValue = false;
    }

    public IFilter CreateFilter(IFilterFactory factory, IContainer filterContainer, IFilter[] alreadyExisting)
    {
        var importer = new FilterImporter(factory, null);
        var newFilter = importer.ImportFilter(filterContainer, _filter, alreadyExisting);

        foreach (var parameterUi in parameterUis)
            parameterUi.HandleSettingParameters(newFilter);

        //if there are known good values
        if (ddKnownGoodValues.SelectedItem != null && ddKnownGoodValues.SelectedItem as string != string.Empty)
            newFilter.Name += $"_{ddKnownGoodValues.SelectedItem}";


        newFilter.FilterContainer_ID = filterContainer.ID;
        newFilter.SaveToDatabase();

        return newFilter;
    }
}