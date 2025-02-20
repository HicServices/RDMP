// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms.DataVisualization.Charting;
using BrightIdeasSoftware;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataQualityEngine.Reports;
using Rdmp.Core.ReusableLibraryCode.Annotations;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.TestsAndSetup.ServicePropogation;


namespace Rdmp.UI.DataLoadUIs.LoadMetadataUIs.LoadProgressAndCacheUIs.Diagrams;

/// <summary>
/// Allows you to visualise how much data has been loaded for a given LoadProgress based DLE job (LoadMetadata).  The top graph shows row counts over time
/// according to the last DQE run on the dataset (a stack graph with a separate track for each Catalogue in the load - for when you load multiple datasets
/// from the same cached data source).  The bottom graph shows counts of cache fetch failures (periods of dataset time where no data could be fetched from
/// the origin because of data corruption or the data simply not being available for that period) and cache directory file counts (number of files sat in
/// Cache by date awaiting loading by DLE).
/// </summary>
public partial class LoadProgressDiagramUI : RDMPUserControl
{
    private LoadProgressAnnotation _annotations;
    private LoadProgress _loadProgress;
    private LoadProgressSummaryReport _report;
    public event Action LoadProgressChanged;

    private ChartLookAndFeelSetter _chartLookAndFeelSetter = new();


    public LoadProgressDiagramUI()
    {
        InitializeComponent();

        olvLastDQERun.AspectGetter += AspectGetterLastDQERun;
        olvExecute.AspectGetter += rowObject => "Execute";
        olvExecute.IsButton = true;
        olvExecute.ButtonSizing = OLVColumn.ButtonSizingMode.CellBounds;
        olvDQERuns.ButtonClick += olvDQERuns_ButtonClick;
    }

    private void olvDQERuns_ButtonClick(object sender, CellClickEventArgs e)
    {
        var c = (Catalogue)e.Model;
        new ExecuteCommandRunDQEOnCatalogue(Activator).SetTarget(c).Execute();
    }

    [NotNull]
    private object AspectGetterLastDQERun(object rowObject)
    {
        if (rowObject is not Catalogue c) return "Not a catalogue";

        return !_report.CataloguesWithDQERuns.TryGetValue(c, out var value) ? "Never"
            : value.DateOfEvaluation;
    }

    public void SetLoadProgress(LoadProgress lp, IActivateItems activator)
    {
        SetItemActivator(activator);
        _loadProgress = lp;
        RefreshUIFromDatabase();

        DoTransparencyProperly.ThisHoversOver(pathLinkLabel1, cacheState);
    }

    private void RefreshUIFromDatabase()
    {
        ragSmiley1.Reset();

        if (_loadProgress == null)
            return;

        _report = new LoadProgressSummaryReport(_loadProgress);

        try
        {
            _report.Check(ragSmiley1);
        }
        catch (Exception e)
        {
            ragSmiley1.Fatal(e);
        }

        olvDQERuns.ClearObjects();

        if (_report.DQERepositoryExists)
        {
            cataloguesRowCountChart.Visible = true;
            olvDQERuns.AddObjects(_report.CataloguesWithDQERuns.Keys);
            olvDQERuns.AddObjects(_report.CataloguesMissingDQERuns.ToArray());
        }
        else
        {
            ragSmiley1.Fatal(new Exception("You don't have any Default DQE server yet"));
            cataloguesRowCountChart.Visible = false;
            return;
        }

        olvDQERuns.Height = 100 + olvDQERuns.RowHeight * olvDQERuns.GetItemCount();
        olvDQERuns.Top = splitContainer1.Panel1.Height - olvDQERuns.Height;
        btnRefresh.Top = olvDQERuns.Top;
        ragSmiley1.Top = olvDQERuns.Top;
        cataloguesRowCountChart.Height = splitContainer1.Panel1.Height - olvDQERuns.Height;


        if (_report.CataloguesPeriodictiyData == null)
        {
            cataloguesRowCountChart.Visible = false;
            splitContainer1.Panel2Collapsed = true;
            return;
        }

        cataloguesRowCountChart.Visible = true;
        splitContainer1.Panel2Collapsed = _loadProgress.CacheProgress == null;

        cataloguesRowCountChart.Palette = ChartColorPalette.None;
        cataloguesRowCountChart.PaletteCustomColors = new[]
        {
            Color.FromArgb(160, 0, 65),
            Color.FromArgb(246, 110, 60),
            Color.FromArgb(255, 175, 89),
            Color.FromArgb(255, 225, 133),
            Color.FromArgb(255, 255, 188),
            Color.FromArgb(230, 246, 147),
            Color.FromArgb(170, 222, 162),
            Color.FromArgb(98, 195, 165),
            Color.FromArgb(44, 135, 191),
            Color.FromArgb(94, 76, 164)
        };
        try
        {
            //Catalogue periodicity chart
            ChartLookAndFeelSetter.PopulateYearMonthChart(cataloguesRowCountChart, _report.CataloguesPeriodictiyData,
                "Count of records");

            //Annotations
            _annotations = new LoadProgressAnnotation(_loadProgress, _report.CataloguesPeriodictiyData,
                cataloguesRowCountChart);
            cataloguesRowCountChart.Annotations.Add(_annotations.LineAnnotationOrigin);
            cataloguesRowCountChart.Annotations.Add(_annotations.TextAnnotationOrigin);
            cataloguesRowCountChart.Annotations.Add(_annotations.LineAnnotationFillProgress);
            cataloguesRowCountChart.Annotations.Add(_annotations.TextAnnotationFillProgress);

            //Cache annotation (still on the Catalogue periodicity chart)
            if (_annotations.LineAnnotationCacheProgress != null)
            {
                cataloguesRowCountChart.Annotations.Add(_annotations.LineAnnotationCacheProgress);
                cataloguesRowCountChart.Annotations.Add(_annotations.TextAnnotationCacheProgress);
            }

            //Now onto the cache diagram which shows what files are in the cache directory and the failure states of old loads
            if (_report.CachePeriodictiyData == null)
            {
                splitContainer1.Panel2Collapsed = true;
            }
            else
            {
                pathLinkLabel1.Text = _report.ResolvedCachePath.FullName;

                cacheState.Palette = ChartColorPalette.None;
                cacheState.PaletteCustomColors = new[] { Color.Red, Color.Green };
                ChartLookAndFeelSetter.PopulateYearMonthChart(cacheState, _report.CachePeriodictiyData,
                    "Fetch Failure/Success");
                splitContainer1.Panel2Collapsed = false;

                cacheState.Series[0].ChartType = SeriesChartType.Column;
                cacheState.Series[0]["DrawingStyle"] = "Cylinder";

                cacheState.Series[1].ChartType = SeriesChartType.Column;
                cacheState.Series[1]["DrawingStyle"] = "Cylinder";
            }
        }
        catch (Exception e)
        {
            ragSmiley1.Fatal(e);
        }
    }


    private void cataloguesRowCountChart_AnnotationPositionChanged(object sender, EventArgs e)
    {
        _annotations?.OnAnnotationPositionChanged(sender, e);

        LoadProgressChanged();
    }

    private void btnRefresh_Click(object sender, EventArgs e)
    {
        RefreshUIFromDatabase();
    }
}