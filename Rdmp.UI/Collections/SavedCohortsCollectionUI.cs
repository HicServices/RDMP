// Copyright (c) The University of Dundee 2018-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core;
using Rdmp.Core.CommandExecution.AtomicCommands.CohortCreationCommands;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Providers;
using Rdmp.Core.Providers.Nodes;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;

namespace Rdmp.UI.Collections;

/// <summary>
/// RDMP Collection which shows all the Cohorts that have been committed to RDMP across all Projects / Cohort Sources.
/// </summary>
public partial class SavedCohortsCollectionUI : RDMPCollectionUI, ILifetimeSubscriber
{
    public SavedCohortsCollectionUI()
    {
        InitializeComponent();

        olvProjectNumber.AspectGetter = AspectGetter_ProjectNumber;
        olvVersion.AspectGetter = AspectGetter_Version;
        olvVersion.IsEditable = false;
        olvProjectNumber.IsEditable = false;
    }

    private object AspectGetter_Version(object rowObject) =>
        rowObject is ExtractableCohort c ? c.ExternalVersion : null;

    private object AspectGetter_ProjectNumber(object rowObject) =>
        rowObject is ExtractableCohort c ? c.ExternalProjectNumber : null;

    public override void SetItemActivator(IActivateItems activator)
    {
        base.SetItemActivator(activator);

        CommonTreeFunctionality.SetUp(RDMPCollection.SavedCohorts, tlvSavedCohorts, Activator, olvName, olvName, tbFilter);

        tlvSavedCohorts.AddObject(((DataExportChildProvider)Activator.CoreChildProvider).RootCohortsNode);

        SetupToolStrip();

        Activator.RefreshBus.EstablishLifetimeSubscription(this);

        CommonTreeFunctionality.SetupColumnTracking(olvName, new Guid("6857032b-4b28-4f92-8b38-f532f11c7a44"));
        CommonTreeFunctionality.SetupColumnTracking(olvVersion, new Guid("637fcb62-8395-4b36-a5ce-76ed3194b4e0"));
        CommonTreeFunctionality.SetupColumnTracking(olvProjectNumber, new Guid("8378f8cf-b08d-4656-a16e-760eed71fe3a"));
    }

    public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
    {
        SetupToolStrip();
    }

    private void SetupToolStrip()
    {
        CommonFunctionality.ClearToolStrip();
        CommonFunctionality.Add(new ExecuteCommandCreateNewCohortFromFile(Activator, null), GlobalStrings.FromFile,
            null, "New...");
        CommonFunctionality.Add(
            new ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration(Activator, null),
            "From Query", null, "New...");
        var _refresh = new ToolStripMenuItem
        {
            Visible = true,
            Image = FamFamFamIcons.arrow_refresh.ImageToBitmap(),
            Alignment = ToolStripItemAlignment.Right,
            ToolTipText = "Refresh Object"
        };
        _refresh.Click += delegate (object sender, EventArgs e) {
            var cohort = ((DataExportChildProvider)Activator.CoreChildProvider).CohortSources.First();
            if (cohort is not null)
            {
                var cmd = new ExecuteCommandRefreshObject(Activator, cohort);
                cmd.Execute();
            }
        };
        CommonFunctionality.Add(_refresh);
    }

    public static bool IsRootObject(object root) => root is AllCohortsNode;
}