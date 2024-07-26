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
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Providers.Nodes;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;

namespace Rdmp.UI.Collections;

/// <summary>
/// This collection Shows all your data load configurations.  Each load configuration (LoadMetadata) is associated with 1 or more Catalogues. But each Catalogue can only have one load configuration
/// 
/// <para>Loads are made up of a collection Process Tasks  (See PluginProcessTaskUI, SqlProcessTaskUI and ExeProcessTaskUI). which are run in sequence at pre defined states of the data load
/// (RAW => STAGING => LIVE).</para>
/// 
/// <para>Within the tree collection you can configure each stage in a data load (LoadMetadata).  A LoadMetadata is a recipe for how to load one or more datasets.  It should have a name and
/// description which accurately describes what it does (e.g. 'Load GP/Practice data' - 'Downloads PracticeGP.zip from FTP server, unzips and loads.  Also includes duplication resolution logic for
/// dealing with null vs 0 exact record duplication').</para>
/// 
/// <para>A data load takes place across 3 stages (RAW, STAGING, LIVE - see UserManual.md).  Each stage can have 0 or more tasks associated with it (See PluginProcessTaskUI).  The minimum requirement
/// for a data load is to have an Attacher (class which populates RAW) e.g. AnySeparatorFileAttacher for comma separated files.  This supposes that your project folder loading directory
/// already has the files you are trying to load (See <see cref="ILoadDirectory"/>).  If you want to build an elegant automated solution then you may choose to use a GetFiles process such as
/// FTPDownloader to fetch new files directly off a data providers server.  After this you may need to write some bespoke SQL/Python scripts etc to deal with unclean/unloadable data or
/// just to iron out idiosyncrasies in the data.</para>
///  
/// <para>Each module will have 0 or more arguments, each of which (when selected) will give you a description of what it expects and an appropriate control for you to choose an option. For
/// example the argument SendLoadNotRequiredIfFileNotFound on FTPDownloader explains that when ticked 'If true the entire data load process immediately stops with exit code LoadNotRequired,
/// if false then the load proceeds as normal'.  This means that you can end cleanly if there are no files to download or proceed anyway on the assumption that one of the other modules will
/// produce the files that the load needs.</para>
///
/// <para> There are many plugins that come as standard in the RDMP distribution such as the DelimitedFlatFileAttacher which lets you load files where cells are delimited by a specific character
/// (e.g. commas).  Clicking 'Description' will display the plugins instructions on how/what stage in which to use it.</para>
/// 
/// <para>DataProvider tasks should mostly be used in GetFiles stage and are intended to be concerned with creating files in the ForLoading directory</para>
/// 
/// <para>Attacher tasks can only be used in 'Mounting' and are concerned with taking loading records into RAW tables</para>
/// 
/// <para>Mutilator tasks operate in the Adjust stages (usually AdjustRaw or AdjustStaging - mutilating LIVE would be a very bad idea).  These can do any task on a table(s) e.g. resolve duplication</para>
///  
/// <para>The above guidelines are deliberately vague because plugins can do almost anything.  For example you could have
/// a DataProvider which emailed you every time the data load began or a Mutilator which read data and sent it to a remote web service
/// (or anything!).  Always read the descriptions of plugins to make sure they do what you want. </para>
/// 
/// <para>In addition to the plugins you can define SQL or EXE tasks that run during the load (See SqlProcessTaskUI and ExeProcessTaskUI). </para>
/// </summary>
public partial class LoadMetadataCollectionUI : RDMPCollectionUI, ILifetimeSubscriber
{
    private bool _isFirstTime = true;

    public LoadMetadata SelectedLoadMetadata => tlvLoadMetadata.SelectedObject as LoadMetadata;

    public LoadMetadataCollectionUI()
    {
        InitializeComponent();
        tlvLoadMetadata.RowHeight = 19;
        olvValue.AspectGetter = s => (s as IArgument)?.Value;
    }

    public override void SetItemActivator(IActivateItems activator)
    {
        base.SetItemActivator(activator);
        Activator.RefreshBus.EstablishLifetimeSubscription(this);

        CommonTreeFunctionality.SetUp(
            RDMPCollection.DataLoad,
            tlvLoadMetadata,
            activator,
            olvName,
            olvName);

        CommonTreeFunctionality.WhitespaceRightClickMenuCommandsGetter = a => new IAtomicCommand[]
            { new ExecuteCommandCreateNewLoadMetadata(a) };

        tlvLoadMetadata.AddObject(Activator.CoreChildProvider.AllPermissionWindowsNode);
        tlvLoadMetadata.AddObject(Activator.CoreChildProvider.LoadMetadataRootFolder);

        BuildCommandList();

        if (_isFirstTime)
        {
            CommonTreeFunctionality.SetupColumnTracking(olvName, new Guid("f84e8217-6b3c-4eb4-a314-fbd95b51c422"));
            CommonTreeFunctionality.SetupColumnTracking(olvValue, new Guid("facab93a-6950-4815-9f5f-5f076277adb5"));

            tlvLoadMetadata.Expand(Activator.CoreChildProvider.LoadMetadataRootFolder);
            _isFirstTime = false;
        }
    }


    public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
    {
        if (e.Object is LoadMetadata)
            tlvLoadMetadata.RefreshObject(Activator.CoreChildProvider.LoadMetadataRootFolder);

        if (e.Object is PermissionWindow)
            tlvLoadMetadata.RefreshObject(tlvLoadMetadata.Objects.OfType<AllPermissionWindowsNode>());

        if (e.Object is CacheProgress)
            tlvLoadMetadata.RefreshObject(tlvLoadMetadata.Objects.OfType<AllPermissionWindowsNode>());

        BuildCommandList();
    }

    public static bool IsRootObject(object root) =>
        // The root LoadMetadata FolderNode is a root element in this tree
        root is FolderNode<LoadMetadata> f ? f.Name == FolderHelper.Root : root is AllPermissionWindowsNode;

    public void BuildCommandList()
    {
        CommonFunctionality.ClearToolStrip();
        CommonFunctionality.Add(new ExecuteCommandCreateNewLoadMetadata(Activator), "New");
        var _refresh = new ToolStripMenuItem
        {
            Visible = true,
            Image = FamFamFamIcons.arrow_refresh.ImageToBitmap(),
            Alignment = ToolStripItemAlignment.Right,
            ToolTipText = "Refresh Object"
        };
        _refresh.Click += delegate (object sender, EventArgs e) {
            var lmd = Activator.CoreChildProvider.AllLoadMetadatas.First();
            if (lmd is not null)
            {
                var cmd = new ExecuteCommandRefreshObject(Activator, lmd);
                cmd.Execute();
            }
        };
        CommonFunctionality.Add(_refresh);
    }
}