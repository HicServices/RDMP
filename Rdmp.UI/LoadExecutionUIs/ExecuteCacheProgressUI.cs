// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.TestsAndSetup.ServicePropogation;


namespace Rdmp.UI.LoadExecutionUIs;

/// <summary>
/// Allows you to execute a Caching pipeline for a series of days.  For example this might download files from a web service by date and store them in a cache directory
/// for later loading.  Caching is independent of data loading and only required if you have a long running fetch process which is time based and not suitable for
/// execution as part of the load (due to the length of time it takes or the volatility of the load or just because you want to decouple the two processes).
/// </summary>
public partial class ExecuteCacheProgressUI : CachingEngineUI_Design
{
    private ICacheProgress _cacheProgress;

    public ExecuteCacheProgressUI()
    {
        InitializeComponent();
        AssociatedCollection = RDMPCollection.DataLoad;
        checkAndExecuteUI1.CommandGetter += CommandGetter;
    }

    private RDMPCommandLineOptions CommandGetter(CommandLineActivity commandLineActivity) =>
        new CacheOptions
        {
            CacheProgress = _cacheProgress.ID.ToString(),
            Command = commandLineActivity,
            RetryMode = cbFailures.Checked
        };

    public override void SetDatabaseObject(IActivateItems activator, CacheProgress databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);

        _cacheProgress = databaseObject;

        CommonFunctionality.AddToMenu(new ExecuteCommandEditCacheProgress(activator, databaseObject), "Edit");
        CommonFunctionality.AddToMenu(new ExecuteCommandShowCacheFetchFailures(activator, databaseObject),
            "View Cache Failures");

        var failures = _cacheProgress.CacheFetchFailures.Any(f => f.ResolvedOn == null);
        cbFailures.Enabled = failures;

        checkAndExecuteUI1.SetItemActivator(activator);
    }

    public override void ConsultAboutClosing(object sender, FormClosingEventArgs e)
    {
        base.ConsultAboutClosing(sender, e);
        checkAndExecuteUI1.ConsultAboutClosing(sender, e);
    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<CachingEngineUI_Design, UserControl>))]
public abstract class CachingEngineUI_Design : RDMPSingleDatabaseObjectControl<CacheProgress>;