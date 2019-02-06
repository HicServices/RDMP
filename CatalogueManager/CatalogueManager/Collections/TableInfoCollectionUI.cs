// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Nodes;
using CatalogueLibrary.Nodes.PipelineNodes;
using CatalogueLibrary.Nodes.SharingNodes;
using CatalogueLibrary.Providers;
using CatalogueLibrary.Repositories;
using CatalogueManager.Collections.Providers;
using CatalogueManager.CommandExecution;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands.UIFactory;
using CatalogueManager.Icons.IconProvision.StateBasedIconProviders;
using CatalogueManager.ItemActivation;
using CatalogueManager.LocationsMenu;
using CatalogueManager.Menus;
using CatalogueManager.Menus.MenuItems;
using CatalogueManager.Refreshing;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using MapsDirectlyToDatabaseTable;
using CatalogueManager.Copying;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Settings;
using ReusableUIComponents.TreeHelper;

namespace CatalogueManager.Collections
{
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
        private IActivateItems _activator;
        
        public TableInfoCollectionUI()
        {
            InitializeComponent();
            
            tlvTableInfos.KeyUp += olvTableInfos_KeyUp;

            tlvTableInfos.ItemActivate += tlvTableInfos_ItemActivate;
            olvDataType.AspectGetter = tlvTableInfos_DataTypeAspectGetter;
            olvDataType.IsVisible = UserSettings.ShowColumnDataType;
            olvDataType.VisibilityChanged += (s, e) => UserSettings.ShowColumnDataType = ((OLVColumn)s).IsVisible;

        }


        private object tlvTableInfos_DataTypeAspectGetter(object rowobject)
        {
            var c = rowobject as ColumnInfo;
            var p = rowobject as PreLoadDiscardedColumn;

            if (c != null)
                return c.Data_type;

            if (p != null)
                return p.Data_type;

            return null;
        }

        private void tlvTableInfos_ItemActivate(object sender, EventArgs e)
        {
            var o = tlvTableInfos.SelectedObject;
            
            if (o is DecryptionPrivateKeyNode)
                _activator.ShowWindow(new PasswordEncryptionKeyLocationUI(), true);
        }
        
        public void SelectTableInfo(TableInfo toSelect)
        {
            tlvTableInfos.SelectObject(toSelect);
        }


        private void olvTableInfos_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.C && e.Control && tlvTableInfos.SelectedObject != null)
            {
                Clipboard.SetText(tlvTableInfos.SelectedObject.ToString());
                e.Handled = true;
            }
        }

        public override void SetItemActivator(IActivateItems activator)
        {
            _activator = activator;
            CommonFunctionality.SetUp(
                RDMPCollection.Tables, 
                tlvTableInfos,
                activator,
                olvColumn1,
                olvColumn1
                );

            CommonFunctionality.WhitespaceRightClickMenuCommandsGetter = (a)=> new IAtomicCommand[]
            {
                new ExecuteCommandCreateNewCatalogueByImportingExistingDataTable(a, false),
                new ExecuteCommandBulkImportTableInfos(a)
            };
            
            _activator.RefreshBus.EstablishLifetimeSubscription(this);


            tlvTableInfos.AddObject(_activator.CoreChildProvider.AllRDMPRemotesNode);
            tlvTableInfos.AddObject(_activator.CoreChildProvider.AllObjectSharingNode);
            tlvTableInfos.AddObject(_activator.CoreChildProvider.AllPipelinesNode);
            tlvTableInfos.AddObject(_activator.CoreChildProvider.AllExternalServersNode);
            tlvTableInfos.AddObject(_activator.CoreChildProvider.AllDataAccessCredentialsNode);
            tlvTableInfos.AddObject(_activator.CoreChildProvider.AllANOTablesNode);
            tlvTableInfos.AddObject(_activator.CoreChildProvider.AllServersNode);
            tlvTableInfos.AddObject(_activator.CoreChildProvider.AllConnectionStringKeywordsNode);
            tlvTableInfos.AddObject(_activator.CoreChildProvider.AllStandardRegexesNode);

        }

        public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            if(e.Object is DataAccessCredentials)
                tlvTableInfos.RefreshObject(tlvTableInfos.Objects.OfType<AllDataAccessCredentialsNode>());
            
            if(e.Object is Catalogue || e.Object is TableInfo) 
                tlvTableInfos.RefreshObject(tlvTableInfos.Objects.OfType<AllServersNode>());

            if (tlvTableInfos.IndexOf(_activator.CoreChildProvider.AllPipelinesNode) != -1)
                tlvTableInfos.RefreshObject(_activator.CoreChildProvider.AllPipelinesNode);
        }
        
        public static bool IsRootObject(object root)
        {
            return
                root is AllRDMPRemotesNode ||
                root is AllObjectSharingNode ||
                root is AllPipelinesNode ||
                root is AllExternalServersNode ||
                root is AllDataAccessCredentialsNode ||
                root is AllANOTablesNode ||
                root is AllServersNode ||
                root is AllConnectionStringKeywordsNode || 
                root is AllStandardRegexesNode;

        }
    }
}
