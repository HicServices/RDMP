// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Rdmp.Core;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Rules;
using Rdmp.UI.SimpleControls;
using Rdmp.UI.TestsAndSetup.ServicePropogation;


namespace Rdmp.UI.DataLoadUIs.LoadMetadataUIs.LoadProgressAndCacheUIs;

/// <summary>
/// Let's you configure the settings of a LoadProgress (see LoadProgress) including how many days to ideally load in each data load, what date has currently been loaded up to etc.
/// 
/// <para>Each LoadProgress can be tied to a Cache progress.  If you are using a LoadProgress without a cache then it is up to your load implementation to respect the time period being loaded
/// (e.g. when using RemoteTableAttacher you should make use of the @startDate and @endDate parameters are in your fetch query).  See CacheProgressUI for a description of caching and
/// permission windows.</para>
/// </summary>
public partial class LoadProgressUI : LoadProgressUI_Design, ISaveableUI
{
    private LoadProgress _loadProgress;

    public LoadProgressUI()
    {
        InitializeComponent();
        loadProgressDiagram1.LoadProgressChanged += ReloadUIFromDatabase;
        AssociatedCollection = RDMPCollection.DataLoad;
    }

    private void ReloadUIFromDatabase()
    {
        loadProgressDiagram1.SetLoadProgress(_loadProgress, Activator);
        loadProgressDiagram1.Visible = true;

        tbDataLoadProgress.ReadOnly = true;

        if (_loadProgress.OriginDate != null)
            tbOriginDate.Text = _loadProgress.OriginDate.ToString();

        tbDataLoadProgress.Text =
            _loadProgress.DataLoadProgress != null ? _loadProgress.DataLoadProgress.ToString() : "";
    }

    private void btnEditLoadProgress_Click(object sender, EventArgs e)
    {
        tbDataLoadProgress.ReadOnly = false;
    }

    private void tbDataLoadProgress_TextChanged(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(tbDataLoadProgress.Text))
                _loadProgress.DataLoadProgress = null;
            else
                _loadProgress.DataLoadProgress = DateTime.Parse(tbDataLoadProgress.Text);

            tbDataLoadProgress.ForeColor = Color.Black;
        }
        catch (Exception)
        {
            tbDataLoadProgress.ForeColor = Color.Red;
        }
    }

    private void tbOriginDate_TextChanged(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(tbOriginDate.Text))
                _loadProgress.OriginDate = null;
            else
                _loadProgress.OriginDate = DateTime.Parse(tbOriginDate.Text);

            tbOriginDate.ForeColor = Color.Black;
        }
        catch (Exception)
        {
            tbOriginDate.ForeColor = Color.Red;
        }
    }

    public override void SetDatabaseObject(IActivateItems activator, LoadProgress databaseObject)
    {
        base.SetDatabaseObject(activator, databaseObject);
        _loadProgress = databaseObject;

        loadProgressDiagram1.SetItemActivator(activator);

        ReloadUIFromDatabase();

        CommonFunctionality.AddHelp(nDefaultNumberOfDaysToLoadEachTime,
            "ILoadProgress.DefaultNumberOfDaysToLoadEachTime");

        CommonFunctionality.AddToMenu(new ExecuteCommandActivate(activator, databaseObject.LoadMetadata),
            "Execute Load");
    }


    protected override void SetBindings(BinderWithErrorProviderFactory rules, LoadProgress databaseObject)
    {
        base.SetBindings(rules, databaseObject);

        Bind(tbID, "Text", "ID", l => l.ID);
        Bind(tbName, "Text", "Name", l => l.Name);
        Bind(nDefaultNumberOfDaysToLoadEachTime, "Value", "DefaultNumberOfDaysToLoadEachTime",
            l => l.DefaultNumberOfDaysToLoadEachTime);
    }
}

[TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<LoadProgressUI_Design, UserControl>))]
public abstract class LoadProgressUI_Design : RDMPSingleDatabaseObjectControl<LoadProgress>;