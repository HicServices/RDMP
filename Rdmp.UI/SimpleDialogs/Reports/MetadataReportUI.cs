// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataQualityEngine;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Reports;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

namespace Rdmp.UI.SimpleDialogs.Reports;

/// <summary>
/// This dialog is the preferred way of extracting per dataset documentation for users.  It will generate a report for each (or a single) dataset (Catalogue) including:
/// 
/// <para>- The dataset description</para>
/// 
/// <para>- Descriptions of all extractable columns / extraction transforms</para>
/// 
/// <para>- Counts of the number of records and unique patient identifiers (See ExtractionInformationUI and the IsExtractionIdentifier flag)</para>
/// 
/// <para>- Complete extract of the lookup tables configured for the dataset (See LookupConfiguration)</para>
/// 
/// <para>- Graphs of each IsExtractable aggregate on the dataset (See AggregateGraph)</para>
/// 
/// <para>You can untick any of the above options if desired.  If any aspect times out then you can either fix the underlying problem (maybe you need an index that helps an
/// Aggregate run faster) or just increase the Query Timeout (default is 30s).</para>
/// </summary>
public partial class MetadataReportUI : RDMPForm
{
    private MetadataReport _report;
    private readonly Catalogue[] _catalogues;
    private bool _firstTime = true;

    public MetadataReportUI(IActivateItems activator, ICatalogue[] initialSelection = null) : base(activator)
    {
        InitializeComponent();

        _catalogues = Activator.CoreChildProvider.AllCatalogues.OrderBy(static x => x.Name).ToArray();
        cbxCatalogues.Items.AddRange(_catalogues);

        if (initialSelection != null)
            SetCatalogueSelection(initialSelection);

        aggregateGraph1.Silent = true;

        btnFolder.Image = activator.CoreIconProvider.GetImage(RDMPConcept.CatalogueFolder).ImageToBitmap();
    }

    private void btnGenerateReport_Click(object sender, EventArgs e)
    {
        progressBarsUI1.Clear();

        IEnumerable<Catalogue> toReportOn = _catalogues;

        if (rbAllCatalogues.Checked)
            toReportOn = toReportOn.Where(c => !c.IsInternalDataset && !c.IsDeprecated)
                .ToArray();
        else if (_cataloguesToRun == null || !_catalogues.Any())
            return;
        else
            toReportOn = _cataloguesToRun.Cast<Catalogue>();

        var args = new MetadataReportArgs(toReportOn)
        {
            Timeout = _timeout,
            IncludeRowCounts = cbIncludeRowCounts.Checked,
            IncludeDistinctIdentifierCounts = cbIncludeDistinctIdentifiers.Checked,
            SkipImages = !cbIncludeGraphs.Checked,
            TimespanCalculator = new DatasetTimespanCalculator(),
            IncludeDeprecatedItems = cbIncludeDeprecatedCatalogueItems.Checked,
            IncludeInternalItems = cbIncludeInternalCatalogueItems.Checked,
            IncludeNonExtractableItems = cbIncludeNonExtractable.Checked,
            MaxLookupRows = (int)nMaxLookupRows.Value
        };

        _report = new MetadataReport(Activator.RepositoryLocator.CatalogueRepository, args);

        _report.RequestCatalogueImages += report_RequestCatalogueImages;
        _report.GenerateWordFileAsync(progressBarsUI1, true);

        btnGenerateReport.Enabled = false;
        btnStop.Enabled = true;
    }

    public BitmapWithDescription[] report_RequestCatalogueImages(Catalogue catalogue)
    {
        //cross thread
        if (InvokeRequired)
            return (BitmapWithDescription[])Invoke(new RequestCatalogueImagesHandler(report_RequestCatalogueImages),
                catalogue);

        var toReturn = new List<BitmapWithDescription>();


        aggregateGraph1.Width = (int)_report.PageWidthInPixels;
        aggregateGraph1.Visible = true;
        aggregateGraph1.Left = -(int)_report.PageWidthInPixels; //push the render off screen 


        //only graph extractable aggregates
        foreach (var aggregate in catalogue.AggregateConfigurations.Where(config => config.IsExtractable))
        {
            if (_firstTime)
            {
                aggregateGraph1.SetDatabaseObject(Activator, aggregate);
                _firstTime = false;
            }
            else
            {
                aggregateGraph1.SetAggregate(Activator, aggregate);
            }

            aggregateGraph1.LoadGraphAsync();

            while (aggregateGraph1.Done == false && aggregateGraph1.Crashed == false)
            {
                Thread.Sleep(100);
                Application.DoEvents();
            }

            if (aggregateGraph1.Crashed)
            {
                progressBarsUI1.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error,
                    $"Aggregate with ID {aggregate.ID} crashed", aggregateGraph1.Exception));
                continue;
            }

            //wait 2 seconds for screen refresh?
            Task.Delay(2000).Wait();

            toReturn.AddRange(aggregateGraph1.GetImages());
        }

        aggregateGraph1.Visible = false;

        return toReturn.ToArray();
    }

    private void btnStop_Click(object sender, EventArgs e)
    {
        _report.Abort();
        aggregateGraph1.AbortLoadGraph();

        btnStop.Enabled = false;
        btnGenerateReport.Enabled = true;
    }

    private void ConfigureMetadataReport_FormClosing(object sender, FormClosingEventArgs e)
    {
        _report?.Abort();

        aggregateGraph1.AbortLoadGraph();
    }

    private void rbAllCatalogues_CheckedChanged(object sender, EventArgs e)
    {
        if (bLoading)
            return;

        cbxCatalogues.Enabled = false;
        btnPick.Enabled = false;
    }

    private void rbSpecificCatalogue_CheckedChanged(object sender, EventArgs e)
    {
        if (bLoading)
            return;

        cbxCatalogues.Enabled = true;
        btnPick.Enabled = true;
    }


    private int _timeout = 30;
    private ICatalogue[] _cataloguesToRun;

    private void tbTimeout_TextChanged(object sender, EventArgs e)
    {
        try
        {
            _timeout = int.Parse(tbTimeout.Text);
            aggregateGraph1.Timeout = _timeout;
            tbTimeout.ForeColor = Color.Black;
        }
        catch (Exception)
        {
            tbTimeout.ForeColor = Color.Red;
        }
    }

    private void cbIncludeDistinctIdentifiers_CheckedChanged(object sender, EventArgs e)
    {
    }

    private void btnPick_Click(object sender, EventArgs e)
    {
        if (Activator.SelectObjects(new DialogArgs
        {
            TaskDescription = "Which Catalogue(s) do you want to generate metadata for?"
        }, cbxCatalogues.Items.OfType<Catalogue>().ToArray(), out var selected))
            SetCatalogueSelection(selected);
    }

    private bool bLoading;

    private void SetCatalogueSelection(ICatalogue[] array)
    {
        bLoading = true;
        if (array.Length > 1)
        {
            rbSpecificCatalogue.Checked = true;
            cbxCatalogues.SelectedItem = null;
            cbxCatalogues.Enabled = false;
            _cataloguesToRun = array;
        }
        else if (array.Length == 1)
        {
            rbSpecificCatalogue.Checked = true;
            cbxCatalogues.SelectedItem = array[0];
            cbxCatalogues.Enabled = true;
            _cataloguesToRun = array;
        }
        else
        {
            rbAllCatalogues.Checked = true;
            cbxCatalogues.SelectedItem = null;
            cbxCatalogues.Enabled = true;
            _cataloguesToRun = array;
        }

        bLoading = false;
    }

    private void cbxCatalogues_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (bLoading)
            return;

        if (cbxCatalogues.SelectedItem != null)
            _cataloguesToRun = new[] { (ICatalogue)cbxCatalogues.SelectedItem };
    }

    private void btnFolder_Click(object sender, EventArgs e)
    {
        var folders = Activator.CoreChildProvider
            .AllCatalogues
            .Select(c => c.Folder)
            .Distinct()
            .ToArray();

        if (Activator.SelectObject(new DialogArgs
        {
            TaskDescription =
                    "Which folder do you want to generate metadata for? All Catalogues in that folder will be included in the metadata report generated"
        }, folders, out var selected))
            SetCatalogueSelection(
                Activator.CoreChildProvider
                    .AllCatalogues
                    .Where(c => c.Folder.Equals(selected))
                    .ToArray());
    }
}