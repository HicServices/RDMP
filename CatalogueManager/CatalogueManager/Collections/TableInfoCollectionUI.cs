using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Nodes;
using CatalogueLibrary.Providers;
using CatalogueLibrary.Repositories;
using CatalogueManager.Collections.Providers;
using CatalogueManager.CommandExecution;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands.UIFactory;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.Icons.IconProvision.StateBasedIconProviders;
using CatalogueManager.ItemActivation;
using CatalogueManager.LocationsMenu;
using CatalogueManager.Menus;
using CatalogueManager.Menus.MenuItems;
using CatalogueManager.Refreshing;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using MapsDirectlyToDatabaseTable;
using RDMPObjectVisualisation.Copying;
using ReusableUIComponents.TreeHelper;

namespace CatalogueManager.Collections
{
    /// <summary>
    /// Lists all the tables that are in your data repository that the RDMP knows about.  Because it is likely that you have lots of tables including many temporary tables and legacy
    /// tables you would rather just forget about RDMP only tracks the tables you point it at.  This is done through TableInfo objects (which each have a collection of ColumnInfos).
    /// These are mostly discovered and synchronised by the RDMP (e.g. through ImportSQLTable).
    /// 
    /// The control shows the RDMP's table reference (TableInfo), this is a pointer to the specific database/tables/credentials used to access the data in the dataset at query
    /// time (e.g. when doing a project extraction).
    /// 
    /// TableInfos are not just cached schema data, they are also the launch point for configuring Lookup relationships (See ConfigureLookups), Join logic (See ConfigureJoins),
    /// Anonymisation etc.  
    /// 
    /// Right clicking on a TableInfo lets you:
    /// 
    /// DeleteTableInfo - This will delete the RDMP record of the table, this will result in MISSING columnInfo relationships if there is a Catalogue (dataset) which draws on this table.
    ///   This is done so that you don't lose descriptive data and transforms / filters etc just because you deleted the reference to the underlying table).
    /// 
    /// Synchronize - Checks for differences between the RDMP's knowledge of the table schema and the actual table schema in your live data repository.  This will handle when you add 
    /// new columns to your data repository.  Synchronization automatically happens at certain points e.g. before data is loaded.
    ///  
    /// View Extract - Lets you view the data in the underlying table in your repository
    /// 
    /// Create Shadow _Archive Table - Mirrors the functionality of Data Load Engine (See SHADOW ARCHIVE TABLES in UserManual.docx) but without having to build a full load on the table
    /// 
    /// Configure Primary Key Collision Resolution - Launches ConfigurePrimaryKeyCollisionResolution
    /// 
    /// Configure Discarded Columns - Launches ConfigurePreLoadDiscardedColumns
    /// 
    /// Configure ANOTables - Launches ConfigureANOForTableInfo
    /// 
    /// Configure Credentials - Launches ConfigureCredentialsForTableInfos
    /// 
    /// View Dependencies - Lists in graphical format all the RDMP objects (database records) that relate to this TableInfo all the way up to datasets (Catalogues), Loads etc in a huge
    /// network diagram.
    /// 
    ///  Right Clicking a ColumnInfo also lets you View Extract, Delete and View Dependencies but also lets you launch the ConfigureLookups and ConfigureJoins dialogs
    /// </summary>
    public partial class TableInfoCollectionUI : RDMPCollectionUI, ILifetimeSubscriber
    {
        private IActivateItems _activator;
        
        public TableInfoCollectionUI()
        {
            InitializeComponent();
            
            tlvTableInfos.KeyUp += olvTableInfos_KeyUp;

            tlvTableInfos.ItemActivate += tlvTableInfos_ItemActivate;
            olvColumn2.AspectGetter = tlvTableInfos_DataTypeAspectGetter;
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
            var credentials = o as DataAccessCredentials;
            var externalDatabaseServer = o as ExternalDatabaseServer;
            var tableInfo = o as TableInfo;
            var preLoadDiscardedColumn = o as PreLoadDiscardedColumn;
            

            if (credentials != null)
                _activator.ActivateDataAccessCredentials(this,credentials);

            if (o is DecryptionPrivateKeyNode)
                _activator.ShowWindow(new PasswordEncryptionKeyLocationUI(), true);

            if (externalDatabaseServer != null)
                _activator.ActivateExternalDatabaseServer(this, externalDatabaseServer);

            if (tableInfo != null)
                _activator.ActivateTableInfo(this, tableInfo);

            if (preLoadDiscardedColumn != null)
                _activator.ActivatePreLoadDiscardedColumn(this, preLoadDiscardedColumn);

        }
        
        public void SelectTableInfo(TableInfo toSelect)
        {
            tlvTableInfos.SelectObject(toSelect);
        }


        private void olvTableInfos_CellRightClick(object sender, CellRightClickEventArgs e)
        {
            TableInfo tableInfo = e.Model as TableInfo;
            ColumnInfo columnInfo = e.Model as ColumnInfo;
            var discardCollection = e.Model as PreLoadDiscardedColumnsNode;
            
            if (e.Model is AllExternalServersNode)
                e.MenuStrip = new AllExternalServersNodeMenu(_activator);
            else
            if (tableInfo != null)
                e.MenuStrip = new TableInfoMenu( _activator, tableInfo);
            else if (columnInfo != null)
                e.MenuStrip = new ColumnInfoMenu(_activator, columnInfo);
            else if (e.Model is DataAccessCredentialsNode)
                e.MenuStrip = new DataAccessCredentialsNodeMenu(_activator);
            else if (e.Model is DataAccessCredentialUsageNode)
                e.MenuStrip = new DataAccessCredentialUsageNodeMenu( _activator,(DataAccessCredentialUsageNode)e.Model);
            else if (discardCollection != null)
                e.MenuStrip = new PreLoadDiscardedColumnsCollectionMenu(_activator, discardCollection);
            else
            if(e.Model == null)
            {
                //user right clicked in an empty area
                var factory = new AtomicCommandUIFactory(_activator.CoreIconProvider);
                e.MenuStrip = factory.CreateMenu(new ExecuteCommandCreateNewCatalogueByImportingExistingDataTable(_activator,false));
            }

        }

        private void olvTableInfos_KeyUp(object sender, KeyEventArgs e)
        {
            
            if (e.KeyCode == Keys.Delete)
            {
                var credentialUsage = tlvTableInfos.SelectedObject as DataAccessCredentialUsageNode;

                if(credentialUsage != null)
                    if(MessageBox.Show("Are you sure you want to remove usage rights under Context " + credentialUsage.Context + " for TableInfo " + credentialUsage.TableInfo,"Revoke Usage Permission",MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        RepositoryLocator.CatalogueRepository.TableInfoToCredentialsLinker.BreakLinkBetween(credentialUsage.Credentials, credentialUsage.TableInfo, credentialUsage.Context);
                        _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(credentialUsage.TableInfo));

                    }
            }
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
                tlvTableInfos,
                activator,
                olvColumn1,
                olvColumn1
                );
            
            _activator.RefreshBus.EstablishLifetimeSubscription(this);

            tlvTableInfos.AddObject(_activator.CoreChildProvider.AllAutomationServerSlotsNode);

            tlvTableInfos.AddObject(_activator.CoreChildProvider.AllExternalServersNode);
            tlvTableInfos.AddObject(_activator.CoreChildProvider.DataAccessCredentialsNode);
            tlvTableInfos.AddObject(_activator.CoreChildProvider.ANOTablesNode);
            tlvTableInfos.AddObject(_activator.CoreChildProvider.AllServersNode);
            

        }

        public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            if(e.Object is DataAccessCredentials)
                tlvTableInfos.RefreshObject(tlvTableInfos.Objects.OfType<DataAccessCredentialsNode>());
            
            if(e.Object is Catalogue || e.Object is TableInfo)
                tlvTableInfos.RefreshObject(_activator.CoreChildProvider.AllServersNode);
        }
        
        private bool bExpand = true;
             
        private void btnExpandOrCollapse_Click(object sender, EventArgs e)
        {
            if (bExpand)
                tlvTableInfos.ExpandAll();
            else
                tlvTableInfos.CollapseAll();

            btnExpandOrCollapse.Text = bExpand ? "Collapse" : "Expand";
            bExpand = !bExpand;
        }

        public static bool IsRootObject(object root)
        {
            return root is DataAccessCredentialsNode ||
            root is ANOTablesNode || 
            root is AllServersNode || 
            root is AllExternalServersNode;
        }
    }
}
