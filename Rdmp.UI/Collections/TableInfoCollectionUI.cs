// Copyright (c) The University of Dundee 2018-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.Providers;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.Providers.Nodes.PipelineNodes;
using Rdmp.Core.Providers.Nodes.SharingNodes;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.LocationsMenu;
using Rdmp.UI.Refreshing;

namespace Rdmp.UI.Collections;

/// <summary>
/// Lists all the tables that are in your data repository that the RDMP knows about.  Because it is likely that you have lots of tables including many temporary tables and legacy
/// tables you would rather just forget about RDMP only tracks the tables you point it at.  This is done through TableInfo objects (which each have a collection of ColumnInfos).
/// These are mostly discovered and synchronised by the RDMP (e.g. through ImportSQLTable).
/// 
/// <para>The control shows the RDMP's table reference (TableInfo), this is a pointer to the specific database/tables/credentials used to access the data in the dataset at query
/// time (e.g. when doing a project extraction).</para>
/// 
/// <para>TableInfos are not just cached schema data, they are also the launch point for configuring Lookup relationships (See LookupConfiguration), Join logic (See JoinConfiguration),
/// Anonymisation etc.  </para>
/// </summary>
public partial class TableInfoCollectionUI : RDMPCollectionUI, ILifetimeSubscriber
{
    private bool _isFirstTime = true;

    public TableInfoCollectionUI()
    {
        InitializeComponent();

        tlvTableInfos.KeyUp += olvTableInfos_KeyUp;

        tlvTableInfos.ItemActivate += tlvTableInfos_ItemActivate;
        olvDataType.AspectGetter = tlvTableInfos_DataTypeAspectGetter;
        olvValue.AspectGetter = static s => (s as IArgument)?.Value;
    }


    private object tlvTableInfos_DataTypeAspectGetter(object rowobject) =>
        rowobject switch
        {
            ColumnInfo c => c.Data_type,
            PreLoadDiscardedColumn p => p.Data_type,
            PipelineComponentArgument a => a.Type,
            _ => null
        };

    private void tlvTableInfos_ItemActivate(object sender, EventArgs e)
    {
        var o = tlvTableInfos.SelectedObject;

        if (o is DecryptionPrivateKeyNode)
        {
            var c = new PasswordEncryptionKeyLocationUI();
            c.SetItemActivator(Activator);
            Activator.ShowWindow(c, true);
        }
    }

    public void SelectTableInfo(TableInfo toSelect)
    {
        tlvTableInfos.SelectObject(toSelect);
    }


    private void olvTableInfos_KeyUp(object sender, KeyEventArgs e)
    {
        // Handle ctrl-C when an object is selected
        if (e.KeyCode != Keys.C || !e.Control || tlvTableInfos.SelectedObject == null) return;

        Clipboard.SetText(tlvTableInfos.SelectedObject?.ToString());
        e.Handled = true;
    }

    public override void SetItemActivator(IActivateItems activator)
    {
        base.SetItemActivator(activator);
        CommonTreeFunctionality.SetUp(
            RDMPCollection.Tables,
            tlvTableInfos,
            activator,
            olvColumn1,
            olvColumn1
        );

        if (_isFirstTime)
        {
            CommonTreeFunctionality.SetupColumnTracking(olvDataType, new Guid("c743eab7-1c07-41dd-bb10-68b25a437056"));
            CommonTreeFunctionality.SetupColumnTracking(olvValue, new Guid("157fde35-d084-42f6-97d1-13a00ba4d0c1"));
            CommonTreeFunctionality.SetupColumnTracking(olvColumn1, new Guid("3743e6dd-4166-4f71-b42f-c80ccda1446d"));
            _isFirstTime = false;
        }


        CommonTreeFunctionality.WhitespaceRightClickMenuCommandsGetter = static a => new IAtomicCommand[]
        {
            new ExecuteCommandImportTableInfo(a, null, false),
            new ExecuteCommandBulkImportTableInfos(a)
        };

        Activator.RefreshBus.EstablishLifetimeSubscription(this, typeof(DataAccessCredentials).ToString());
        Activator.RefreshBus.EstablishLifetimeSubscription(this, typeof(Catalogue).ToString());
        Activator.RefreshBus.EstablishLifetimeSubscription(this, typeof(TableInfo).ToString());


        tlvTableInfos.AddObject(Activator.CoreChildProvider.AllDashboardsNode);
        tlvTableInfos.AddObject(Activator.CoreChildProvider.AllRDMPRemotesNode);
        tlvTableInfos.AddObject(Activator.CoreChildProvider.AllObjectSharingNode);
        tlvTableInfos.AddObject(Activator.CoreChildProvider.AllPipelinesNode);
        tlvTableInfos.AddObject(Activator.CoreChildProvider.AllExternalServersNode);
        tlvTableInfos.AddObject(Activator.CoreChildProvider.AllDataAccessCredentialsNode);
        tlvTableInfos.AddObject(Activator.CoreChildProvider.AllANOTablesNode);
        tlvTableInfos.AddObject(Activator.CoreChildProvider.AllServersNode);
        tlvTableInfos.AddObject(Activator.CoreChildProvider.AllConnectionStringKeywordsNode);
        tlvTableInfos.AddObject(Activator.CoreChildProvider.AllStandardRegexesNode);
        tlvTableInfos.AddObject(Activator.CoreChildProvider.AllPluginsNode);
    }

    public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
    {
        switch (e.Object)
        {
            case DataAccessCredentials:
                tlvTableInfos.RefreshObject(tlvTableInfos.Objects.OfType<AllDataAccessCredentialsNode>());
                break;
            case Catalogue or TableInfo:
                tlvTableInfos.RefreshObject(tlvTableInfos.Objects.OfType<AllServersNode>());
                break;
        }

        if (tlvTableInfos.IndexOf(Activator.CoreChildProvider.AllPipelinesNode) != -1)
            tlvTableInfos.RefreshObject(Activator.CoreChildProvider.AllPipelinesNode);
    }

    public static bool IsRootObject(object root) => root is AllRDMPRemotesNode or AllObjectSharingNode
        or AllPipelinesNode or AllExternalServersNode or AllDataAccessCredentialsNode or AllANOTablesNode
        or AllServersNode or AllConnectionStringKeywordsNode or AllStandardRegexesNode or AllDashboardsNode;
}