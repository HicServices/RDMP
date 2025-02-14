// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Windows.Forms;
using Rdmp.Core;
using Rdmp.Core.Caching.Pipeline;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.PipelineUIs.Pipelines.PluginPipelineUsers;
using Rdmp.UI.Rules;
using Rdmp.UI.SimpleControls;
using Rdmp.UI.TestsAndSetup.ServicePropogation;


namespace Rdmp.UI.DataLoadUIs.LoadMetadataUIs.LoadProgressAndCacheUIs;

/// <summary>
/// Caching is method by which long term transfer tasks take place in the RDMP.  These are usually of files and are expected to run all the time (up to 24/7).  Cached data must always be
/// temporal (i.e. a given set of files must correspond to a specific time) such that cache requests for a specific date/time do not vary in real time.  The exact implementation of any
/// caching task is done through a Pipeline.  Since caching is super bespoke, it is anticipated that you will have written your own caching data classes for use in your pipeline.
/// 
/// <para>Clicking 'Configure Caching Pipeline' will let you setup what happens during the caching activity.</para>
/// 
/// <para>Changing the 'Lag Period' to a positive number will indicate a period of time in which to NOT cache data (e.g. if you set it to 30 days then caching will always be suspended when it
/// has cached up to 1 month ago).</para>
/// 
/// <para>Setting a 'Permission Window' will create a restriction on the times of day in which caching can take place (e.g. between midnight and 4am only).</para>
/// </summary>
public partial class CacheProgressUI : CacheProgressUI_Design, ISaveableUI
{
    private CacheProgress _cacheProgress;

    public CacheProgressUI()
    {
        InitializeComponent();

        _bLoading = true;

        ddCacheLagDurationType.DataSource = Enum.GetValues(typeof(CacheLagPeriod.PeriodType));
        ddCacheLagDelayDurationType.DataSource = Enum.GetValues(typeof(CacheLagPeriod.PeriodType));

        AssociatedCollection = RDMPCollection.DataLoad;

        _bLoading = false;
    }

    private bool _bLoading;
    private Control _pipelineSelectionUI;

    public override void SetDatabaseObject(IActivateItems activator, CacheProgress databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);

        CommonFunctionality.AddHelp(tbCacheProgress, "ICacheProgress.CacheFillProgress");
        CommonFunctionality.AddHelp(ddCacheLagDurationType, "ICacheProgress.CacheLagPeriod");
        CommonFunctionality.AddHelp(ddCacheLagDelayDurationType, "ICacheProgress.CacheLagPeriodLoadDelay");
        CommonFunctionality.AddHelp(tbChunkPeriod, "ICacheProgress.ChunkPeriod");
        CommonFunctionality.AddHelp(pPipeline, "ICacheProgress.Pipeline_ID");

        _cacheProgress = databaseObject;

        CommonFunctionality.AddToMenu(new ExecuteCommandExecuteCacheProgress(activator, databaseObject),
            "Go To Execute");

        ragSmiley1.Reset();

        try
        {
            PopulateCacheProgressPanel(_cacheProgress);
        }
        catch (Exception e)
        {
            ragSmiley1.Fatal(e);
        }
    }

    protected override void SetBindings(BinderWithErrorProviderFactory rules, CacheProgress databaseObject)
    {
        base.SetBindings(rules, databaseObject);

        Bind(tbID, "Text", "ID", c => c.ID);
        Bind(tbName, "Text", "Name", c => c.Name);
    }

    private void PopulateCacheProgressPanel(ICacheProgress cacheProgress)
    {
        _bLoading = true;

        SetCacheProgressTextBox();

        var cacheLagPeriod = cacheProgress.GetCacheLagPeriod();
        if (cacheLagPeriod != null)
        {
            udCacheLagDuration.Value = cacheLagPeriod.Duration;
            ddCacheLagDurationType.SelectedItem = cacheLagPeriod.Type;
        }

        tbChunkPeriod.Text = cacheProgress.ChunkPeriod.ToString();

        UpdateCacheLagPeriodControl();

        SetupPipelineUI();

        _bLoading = false;
    }

    private void SetupPipelineUI()
    {
        if (_pipelineSelectionUI == null)
        {
            var user = new PipelineUser(_cacheProgress);
            var useCase = CachingPipelineUseCase.DesignTime();

            var selectionFactory =
                new PipelineSelectionUIFactory(Activator.RepositoryLocator.CatalogueRepository, user, useCase);
            _pipelineSelectionUI =
                (Control)selectionFactory.Create(Activator, "Cache Pipeline", DockStyle.Fill, pPipeline);
        }
    }

    private void SetCacheProgressTextBox()
    {
        tbCacheProgress.Text = _cacheProgress.CacheFillProgress.HasValue
            ? _cacheProgress.CacheFillProgress.ToString()
            : "Caching not started";
    }

    private void UpdateCacheLagPeriodControl()
    {
        var cacheLagPeriod = _cacheProgress.GetCacheLagPeriod();

        if (cacheLagPeriod == null)
        {
            udCacheLagDuration.Value = 0;
            ddCacheLagDurationType.SelectedItem = CacheLagPeriod.PeriodType.Day;
        }
        else
        {
            udCacheLagDuration.Value = cacheLagPeriod.Duration;
            ddCacheLagDurationType.SelectedItem = cacheLagPeriod.Type;
        }

        var cacheLoadDelayPeriod = _cacheProgress.GetCacheLagPeriodLoadDelay();
        udCacheLagDelayPeriodDuration.Value = cacheLoadDelayPeriod.Duration;
        ddCacheLagDelayDurationType.SelectedItem = cacheLoadDelayPeriod.Type;
    }

    private void udCacheLagDuration_ValueChanged(object sender, EventArgs e)
    {
        if (_bLoading)
            return;

        _cacheProgress.SetCacheLagPeriod(CreateNewCacheLagPeriod(udCacheLagDuration.Value,
            ddCacheLagDurationType.SelectedItem));
    }

    private void ddCacheLagDurationType_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_bLoading)
            return;

        _cacheProgress.SetCacheLagPeriod(CreateNewCacheLagPeriod(udCacheLagDuration.Value,
            ddCacheLagDurationType.SelectedItem));
    }

    private void ddCacheLagDelayDurationType_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_bLoading)
            return;

        _cacheProgress.SetCacheLagPeriodLoadDelay(CreateNewCacheLagPeriod(udCacheLagDelayPeriodDuration.Value,
            ddCacheLagDelayDurationType.SelectedItem));
    }

    private void udCacheLagDelayPeriodDuration_ValueChanged(object sender, EventArgs e)
    {
        if (_bLoading)
            return;

        _cacheProgress.SetCacheLagPeriodLoadDelay(CreateNewCacheLagPeriod(udCacheLagDelayPeriodDuration.Value,
            ddCacheLagDelayDurationType.SelectedItem));
    }

    // Returns null if there is no duration, otherwise picks up the current state of both Duration and Type UI elements
    private static CacheLagPeriod CreateNewCacheLagPeriod(decimal value, object selectedItem)
    {
        var duration = Convert.ToInt32(value);
        CacheLagPeriod cacheLagPeriod = null;

        if (selectedItem != null)
            cacheLagPeriod = new CacheLagPeriod(duration, (CacheLagPeriod.PeriodType)selectedItem);

        return cacheLagPeriod;
    }

    private void btnEdit_Click(object sender, EventArgs e)
    {
        tbCacheProgress.ReadOnly = false;
    }

    private void tbCacheProgress_TextChanged(object sender, EventArgs e)
    {
        RDMPControlCommonFunctionality.DoActionAndRedIfThrows(tbCacheProgress, () =>
        {
            if (string.IsNullOrWhiteSpace(tbCacheProgress.Text))
            {
                _cacheProgress.CacheFillProgress = null;
            }
            else
            {
                var dt = DateTime.Parse(tbCacheProgress.Text);
                _cacheProgress.CacheFillProgress = dt;
            }
        });
    }

    private void tbChunkPeriod_TextChanged(object sender, EventArgs e)
    {
        RDMPControlCommonFunctionality.DoActionAndRedIfThrows(tbChunkPeriod,
            () => { _cacheProgress.ChunkPeriod = TimeSpan.Parse(tbChunkPeriod.Text); });
    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<CacheProgressUI_Design, UserControl>))]
public abstract class CacheProgressUI_Design : RDMPSingleDatabaseObjectControl<CacheProgress>;