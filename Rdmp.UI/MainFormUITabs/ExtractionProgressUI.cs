// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataQualityEngine;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Reports;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Rules;
using Rdmp.UI.SimpleControls;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

namespace Rdmp.UI.MainFormUITabs;

public partial class ExtractionProgressUI : ExtractionProgressUI_Design, ISaveableUI
{
    public ExtractionProgress ExtractionProgress => (ExtractionProgress)DatabaseObject;
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IDetermineDatasetTimespan TimespanCalculator { get; set; } = new DatasetTimespanCalculator();

    private Tuple<DateTime?, DateTime?> dqeResult;

    public ExtractionProgressUI()
    {
        InitializeComponent();
        var tt = new ToolTip();
        tt.SetToolTip(btnFromDQE, "Populate Start and End dates according to the latest DQE results");
        ddRetry.DataSource = Enum.GetValues(typeof(RetryStrategy));
    }

    protected override void SetBindings(BinderWithErrorProviderFactory rules, ExtractionProgress databaseObject)
    {
        base.SetBindings(rules, databaseObject);

        Bind(tbID, "Text", "ID", d => d.ID);
        Bind(tbDaysPerBatch, "Text", "NumberOfDaysPerBatch", d => d.NumberOfDaysPerBatch);
    }

    public override void SetDatabaseObject(IActivateItems activator, ExtractionProgress databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);

        var result =
            TimespanCalculator.GetMachineReadableTimespanIfKnownOf(
                databaseObject.ExtractionInformation.CatalogueItem.Catalogue, false, out var date);

        btnFromDQE.Image = activator.CoreIconProvider.GetImage(RDMPConcept.DQE, OverlayKind.Import).ImageToBitmap();

        if (date != null)
        {
            lblEvaluationDate.Text = $"(DQE Run on {date})";
            btnFromDQE.Enabled = true;
            dqeResult = result;
        }
        else
        {
            lblEvaluationDate.Text = "(DQE has not been run)";
            btnFromDQE.Enabled = false;
        }


        tbStartDate.Text = databaseObject.StartDate?.ToString("yyyy-MM-dd") ?? "";
        tbEndDate.Text = databaseObject.EndDate == null ? "" : databaseObject.EndDate.Value.ToString("yyyy-MM-dd");
        tbProgress.Text = databaseObject.ProgressDate == null
            ? ""
            : databaseObject.ProgressDate.Value.ToString("yyyy-MM-dd");

        ddRetry.SelectedItem = databaseObject.Retry;

        var cata = databaseObject.SelectedDataSets.GetCatalogue();
        ddColumn.DataSource = cata.GetAllExtractionInformation();


        try
        {
            ddColumn.SelectedItem = databaseObject.ExtractionInformation;
            ragSmiley1.Reset();
            ExtractionProgress.ValidateSelectedColumn(ragSmiley1, databaseObject.ExtractionInformation);
        }
        catch (Exception)
        {
            // could be that the user has deleted this ExtractionInformation
            ddColumn.SelectedItem = null;
        }
    }


    private void tbDate_TextChanged(object sender, EventArgs e)
    {
        if (sender == tbStartDate) SetDate(tbStartDate, v => ExtractionProgress.StartDate = v);

        if (sender == tbEndDate) SetDate(tbEndDate, v => ExtractionProgress.EndDate = v);

        if (sender == tbProgress) SetDate(tbProgress, v => ExtractionProgress.ProgressDate = v);
    }

    private void ddColumn_SelectionChangeCommitted(object sender, EventArgs e)
    {
        ragSmiley1.Reset();
        if (ddColumn.SelectedItem is ExtractionInformation ei)
        {
            ExtractionProgress.ValidateSelectedColumn(ragSmiley1, ei);
            ExtractionProgress.ExtractionInformation_ID = ei.ID;
        }
    }

    private void btnPickColumn_Click(object sender, EventArgs e)
    {
        var col = (ExtractionInformation)Activator.SelectOne("Column",
            ddColumn.Items.Cast<object>().OfType<ExtractionInformation>().ToArray());

        if (col != null)
        {
            ddColumn.SelectedItem = col;
            ragSmiley1.Reset();
            ExtractionProgress.ValidateSelectedColumn(ragSmiley1, col);
            ExtractionProgress.ExtractionInformation_ID = col.ID;
        }
    }

    private void btnFromDQE_Click(object sender, EventArgs e)
    {
        tbStartDate.Text = dqeResult.Item1?.ToString("yyyy-MM-dd");
        tbEndDate.Text = dqeResult.Item2?.AddDays(1).ToString("yyyy-MM-dd");
    }

    private void btnResetProgress_Click(object sender, EventArgs e)
    {
        tbProgress.Text = "";
    }

    private void ddRetry_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (ExtractionProgress == null) return;
        ExtractionProgress.Retry = (RetryStrategy)ddRetry.SelectedItem;
    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ExtractionProgressUI_Design, UserControl>))]
public abstract class ExtractionProgressUI_Design : RDMPSingleDatabaseObjectControl<ExtractionProgress>
{
}