// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using BrightIdeasSoftware;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Logging;
using Rdmp.UI.Collections;
using Rdmp.UI.DashboardTabs.Construction;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

namespace Rdmp.UI.Overview;

/// <summary>
/// Displays a graph showing how many of your data loads passed the last time they were run and which data loads are currently failing.  If you have
/// not configured any data loads yet then this control will be blank.
/// </summary>
public partial class DataLoadsGraph : RDMPUserControl, IDashboardableControl
{
    private DataLoadsGraphObjectCollection _collection;

    public DataLoadsGraph()
    {
        InitializeComponent();

        SetupOlvDelegates();

        olvDataLoads.AlwaysGroupByColumn = olvStatus;
        olvDataLoads.UseCellFormatEvents = true;
        olvDataLoads.FormatCell += olvDataLoads_FormatCell;

        olvViewLog.AspectGetter += s => "View Log";

        RDMPCollectionCommonFunctionality.SetupColumnTracking(olvDataLoads, olvName,
            new Guid("4a651e11-62f5-4d8f-8fe5-4db488ee7f3a"));
        RDMPCollectionCommonFunctionality.SetupColumnTracking(olvDataLoads, olvLastRun,
            new Guid("1aadf2e8-798d-4e85-8abc-7f45edb839b7"));
        RDMPCollectionCommonFunctionality.SetupColumnTracking(olvDataLoads, olvCategory,
            new Guid("406173bc-44a0-40b7-8bd1-d01a214c277d"));
        RDMPCollectionCommonFunctionality.SetupColumnTracking(olvDataLoads, olvStatus,
            new Guid("8c5cbcd2-9f06-4e24-9521-c3be7ea22eca"));
        RDMPCollectionCommonFunctionality.SetupColumnTracking(olvDataLoads, olvViewLog,
            new Guid("e9c04da0-0e91-442e-90a4-119a1b67ea06"));
    }

    private void SetupOlvDelegates()
    {
        olvName.ImageGetter = delegate
        {
            return Activator.CoreIconProvider.GetImage(RDMPConcept.LoadMetadata).ImageToBitmap();
        };

        olvDataLoads.ButtonClick += delegate (object sender, CellClickEventArgs e)
        {
            var loadSummary = (DataLoadsGraphResult)e.Model;
            var metadata =
                Activator.RepositoryLocator.CatalogueRepository.GetAllObjects<LoadMetadata>()
                    .SingleOrDefault(m => m.ID == loadSummary.ID);

            if (metadata != null)
                new ExecuteCommandViewLogs(Activator, metadata).Execute();
        };

        olvDataLoads.DoubleClick += delegate (object sender, EventArgs args)
        {
            var loadSummary = (DataLoadsGraphResult)olvDataLoads.GetItem(olvDataLoads.SelectedIndex).RowObject;
            if (loadSummary != null)
            {
                var metadata =
                    Activator.RepositoryLocator.CatalogueRepository.GetAllObjects<LoadMetadata>()
                        .SingleOrDefault(m => m.ID == loadSummary.ID);

                if (metadata != null)
                    Activator.RequestItemEmphasis(sender, new EmphasiseRequest(metadata));
            }
        };
    }

    private void olvDataLoads_FormatCell(object sender, FormatCellEventArgs e)
    {
        if (e.ColumnIndex == olvStatus.Index)
            e.SubItem.ForeColor = ((DataLoadsGraphResult)e.Model).Status != DataLoadsGraphResultStatus.Succeeding
                ? Color.Red
                : Color.Black;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        RefreshChartAsync();
    }

    public void RefreshChartAsync()
    {
        if (VisualStudioDesignMode)
            return;

        pbLoading.Visible = true;
        olvDataLoads.ClearObjects();
        ragSmiley1.Reset();
        ragSmiley1.SetVisible(false);

        var t = new Thread(() =>
        {
            try
            {
                var countManualLoadsuccessful = 0;
                var countManualLoadFailure = 0;

                foreach (var metadata in Activator.RepositoryLocator.CatalogueRepository.GetAllObjects<LoadMetadata>())
                    try
                    {
                        LogManager logManager;

                        try
                        {
                            //get the logging connection
                            logManager = new LogManager(metadata.GetDistinctLoggingDatabase());
                        }
                        catch (NotSupportedException e)
                        {
                            //sometimes a load metadata won't have any catalogues so we can't process its log history
                            if (e.Message.Contains("does not have any Catalogues associated with it"))
                                continue;

                            throw;
                        }

                        var archivalDataLoadInfo = logManager
                            .GetArchivalDataLoadInfos(metadata.GetDistinctLoggingTask()).FirstOrDefault();

                        var loadSummary = new DataLoadsGraphResult
                        {
                            ID = metadata.ID,
                            Name = metadata.Name
                        };

                        if (archivalDataLoadInfo == null)
                        {
                            Invoke(new MethodInvoker(() =>
                            {
                                loadSummary.Status = DataLoadsGraphResultStatus.NeverBeenRun;
                                loadSummary.LastRun = "Never";
                                olvDataLoads.AddObject(loadSummary);
                            }));
                            continue; //has never been run (or has had test runs only)
                        }

                        var lastLoadWasError =
                            archivalDataLoadInfo.Errors.Any() || archivalDataLoadInfo.EndTime == null;

                        //while we were fetching data from database the form was closed
                        if (IsDisposed || !IsHandleCreated)
                            return;

                        if (lastLoadWasError)
                            countManualLoadFailure++;
                        else
                            countManualLoadsuccessful++;

                        Invoke(new MethodInvoker(() =>
                        {
                            loadSummary.Status = lastLoadWasError
                                ? DataLoadsGraphResultStatus.Failing
                                : DataLoadsGraphResultStatus.Succeeding;
                            loadSummary.LastRun = archivalDataLoadInfo.EndTime.ToString();

                            olvDataLoads.AddObject(loadSummary);
                        }));
                    }
                    catch (Exception e)
                    {
                        ragSmiley1.Fatal(e);
                        Invoke(new MethodInvoker(() => { pbLoading.Visible = false; }));
                    }


                //if there have been no loads at all ever
                if (countManualLoadsuccessful == 0 && countManualLoadFailure == 0)
                {
                    Invoke(new MethodInvoker(() =>
                    {
                        lblNoDataLoadsFound.Visible = true;
                        chart1.Visible = false;
                        pbLoading.Visible = false;
                    }));

                    return;
                }

                using var dt = new DataTable();
                dt.Columns.Add("Category");
                dt.Columns.Add("NumberOfDataLoadsAtStatus");

                dt.Rows.Add(new object[] { "Manual Successful", countManualLoadsuccessful });
                dt.Rows.Add(new object[] { "Manual Fail", countManualLoadFailure });


                Invoke(new MethodInvoker(() =>
                {
                    chart1.Series[0].XValueMember = "Category";
                    chart1.Series[0].YValueMembers = "NumberOfDataLoadsAtStatus";

                    chart1.DataSource = dt;
                    chart1.DataBind();

                    chart1.Series[0].Points[0].Color = Color.Green;
                    chart1.Series[0].Points[1].Color = Color.Red;

                    var max = new int[]
                    {
                        countManualLoadFailure,
                        countManualLoadsuccessful
                    }.Max();

                    var gridMarkEvery = max == 0 ? 1 : Math.Max(max / 10, 1);


                    chart1.ChartAreas[0].AxisY.Interval = gridMarkEvery;

                    chart1.ChartAreas[0].AxisY.MajorGrid.Interval = gridMarkEvery;
                    chart1.ChartAreas[0].AxisY.MajorTickMark.Interval = gridMarkEvery;
                    chart1.ChartAreas[0].AxisY.MajorTickMark.IntervalOffset = 0;

                    chart1.ChartAreas[0].AxisY.IsMarginVisible = false;


                    chart1.ChartAreas[0].AxisY.MinorGrid.Enabled = false;
                    chart1.ChartAreas[0].AxisY.MinorTickMark.Enabled = false;

                    pbLoading.Visible = false;
                }));
            }
            catch (Exception e)
            {
                ragSmiley1.Fatal(e);
                Invoke(new MethodInvoker(() => { pbLoading.Visible = false; }));
            }
        });
        //t.SetApartmentState(ApartmentState.STA);
        t.Start();
    }

    public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
    {
    }

    public string GetTabName() => Text;

    public string GetTabToolTip() => null;

    public void SetCollection(IActivateItems activator, IPersistableObjectCollection collection)
    {
        SetItemActivator(activator);
        _collection = (DataLoadsGraphObjectCollection)collection;

        if (IsHandleCreated && !IsDisposed)
            RefreshChartAsync();
    }

    public IPersistableObjectCollection GetCollection() => _collection;

    public void NotifyEditModeChange(bool isEditModeOn)
    {
    }

    public IPersistableObjectCollection ConstructEmptyCollection(DashboardControl databaseRecord) =>
        new DataLoadsGraphObjectCollection();
}