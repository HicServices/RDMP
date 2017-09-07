using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using CatalogueLibrary.Triggers;
using CatalogueManager.Collections.Providers;
using CatalogueManager.CommandExecution;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands.UIFactory;
using CatalogueManager.DataLoadUIs.ANOUIs;
using CatalogueManager.DataLoadUIs.ANOUIs.PreLoadDiscarding;
using CatalogueManager.DataViewing;
using CatalogueManager.DataViewing.Collections;
using CatalogueManager.ExtractionUIs;
using CatalogueManager.ExtractionUIs.FilterUIs.ParameterUIs;
using CatalogueManager.ExtractionUIs.FilterUIs.ParameterUIs.Options;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.MainFormUITabs.SubComponents;
using CatalogueManager.Menus.MenuItems;
using CatalogueManager.ObjectVisualisation;
using CatalogueManager.Refreshing;
using CatalogueManager.SimpleDialogs;
using DataLoadEngine.DataFlowPipeline.Components.Anonymisation;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTableUI;
using RDMPObjectVisualisation.Copying.Commands;
using RDMPStartup;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableUIComponents;
using ReusableUIComponents.ChecksUI;
using ReusableUIComponents.Dependencies;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    class TableInfoMenu : RDMPContextMenuStrip
    {
        private DataAccessCredentials[] _availableCredentials;

        public TableInfoMenu(IActivateItems activator, TableInfo tableInfo) : base( activator, tableInfo)
        {
            var factory = new AtomicCommandUIFactory(activator.CoreIconProvider);

            Items.Add(factory.CreateMenuItem(new ExecuteCommandCreateNewCatalogueByImportingExistingDataTable(activator, false)));

            Items.Add(new ToolStripSeparator());
            Items.Add(new AddLookupMenuItem(activator, "Add new Lookup Table Relationship", null, tableInfo));
            Items.Add(new AddJoinInfoMenuItem(activator, tableInfo));
            Items.Add(new ToolStripSeparator());

            Items.Add("Synchronize TableInfo ", CatalogueIcons.Sync, delegate { TableInfo_Click(tableInfo); });
            Items.Add("Synchronize ANO Configuration ", CatalogueIcons.Sync, delegate { SynchronizeANOConfiguration_Click(tableInfo); });

            Items.Add("Add ColumnInfo ", null, delegate { AddColumnInfo_Click(tableInfo); });

            _availableCredentials = RepositoryLocator.CatalogueRepository.GetAllObjects<DataAccessCredentials>();
            var addPermission = new ToolStripMenuItem("Add Credential Usage Permission", activator.CoreIconProvider.GetImage(RDMPConcept.DataAccessCredentials,OverlayKind.Add),(s, e) => AddCredentialPermission(tableInfo));
            addPermission.Enabled = _availableCredentials.Any();
            Items.Add(addPermission);

            Items.Add(new ToolStripSeparator());
            Items.Add("View Extract", null, (s,e)=> _activator.ViewDataSample(new ViewTableInfoExtractUICollection(tableInfo,ViewType.TOP_100)));

            Items.Add(new ToolStripSeparator());
            Items.Add("Create Shadow _Archive Table (Do not create on highly volatile tables!)", CatalogueIcons.Backup, delegate { CreateBackupTrigger_Click(tableInfo); });

            Items.Add(new ToolStripSeparator());
            Items.Add("Configure Primary Key Collision Resolution ", CatalogueIcons.CollisionResolution, delegate { ConfigurePrimaryKeyCollisionResolution_Click(tableInfo); });

            Items.Add(new ToolStripSeparator());


            AddDiscardedColumnsOptions(activator,tableInfo);

            if (tableInfo != null && tableInfo.IsTableValuedFunction)
                Items.Add("Configure Parameters...", activator.CoreIconProvider.GetImage(RDMPConcept.ParametersNode), delegate { ConfigureTableInfoParameters(tableInfo); });

            AddCommonMenuItems();
        }

        private void AddDiscardedColumnsOptions(IActivateItems activator, TableInfo tableInfo)
        {
            var dumpServerOptions = new ToolStripMenuItem("Set Dump Server");

            var cataRepo = activator.RepositoryLocator.CatalogueRepository;

            var iddServers = cataRepo.GetAllObjects<ExternalDatabaseServer>()
                .Where(
                    s =>
                        s.CreatedByAssembly != null &&
                        s.CreatedByAssembly.Equals(typeof (IdentifierDump.Database.Class1).Assembly.GetName().Name)).ToArray();

            //cannot change server if 
            dumpServerOptions.Enabled = tableInfo.IdentifierDumpServer_ID == null;

            foreach (ExternalDatabaseServer v in iddServers)
            {
                var server = v;
                dumpServerOptions.DropDownItems.Add(
                    new ToolStripMenuItem(server.Name, 
                        null,
                    (s, e) => SetDiscardedColumnTo(tableInfo, server)));
            }

            dumpServerOptions.DropDownItems.Add(new ToolStripSeparator());
            dumpServerOptions.DropDownItems.Add("Create New...",null,(s,e)=>CreateNewIdentifierDumpServer(tableInfo));

            Items.Add(dumpServerOptions);

            //todo get rid of this:
            Items.Add("Configure Discarded Columns ", activator.CoreIconProvider.GetImage(RDMPConcept.PreLoadDiscardedColumn), delegate { ConfigureDiscardedColumns_Click(tableInfo); });
        }

        private void CreateNewIdentifierDumpServer(TableInfo tableInfo)
        {
            var cmd = new ExecuteCommandCreateNewExternalDatabaseServer(_activator, typeof(IdentifierDump.Database.Class1).Assembly, ServerDefaults.PermissableDefaults.IdentifierDumpServer_ID);
            cmd.Execute();

            if(cmd.ServerCreatedIfAny != null)
            {
                tableInfo.IdentifierDumpServer_ID = cmd.ServerCreatedIfAny.ID;
                tableInfo.SaveToDatabase();

                _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(tableInfo));
            }
        }

        private void SetDiscardedColumnTo(TableInfo tableInfo, ExternalDatabaseServer server)
        {
            tableInfo.IdentifierDumpServer_ID = server.ID;
            tableInfo.SaveToDatabase();

            _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(tableInfo));
        }

        private void AddCredentialPermission(TableInfo tableInfo)
        {
            var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(_availableCredentials, false, false);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var cmd = new DataAccessCredentialsCommand((DataAccessCredentials)dialog.Selected);
                var execute = new ExecuteCommandUseCredentialsToAccessTableInfoData(_activator, cmd, tableInfo);

                if(execute.IsImpossible)
                {
                    MessageBox.Show(execute.ReasonCommandImpossible);
                    return;
                }

                execute.Execute();
            }
        }

        private void ConfigurePrimaryKeyCollisionResolution_Click(TableInfo tableInfo)
        {
            var dialog = new ConfigurePrimaryKeyCollisionResolution(tableInfo);
            dialog.RepositoryLocator = RepositoryLocator;
            dialog.ShowDialog(this);
        }

        private void ConfigureDiscardedColumns_Click(TableInfo tableInfo)
        {
            var dialog = new ConfigurePreLoadDiscardedColumns(tableInfo);
            dialog.ShowDialog(this);
        }

        private void SynchronizeANOConfiguration_Click(TableInfo tableInfo)
        {
            //let use check the TableInfo accurately reflects the underlying database first
            if (MessageBox.Show("Check that TableInfo is synchronized with underlying database first?", "Check database first?", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    TableInfoSynchronizer synchronizer = new TableInfoSynchronizer(tableInfo);
                    bool isSynchronized = synchronizer.Synchronize(new ThrowImmediatelyCheckNotifier());
                    if (!isSynchronized)
                    {
                        MessageBox.Show("Unable to synchronize with ANO database because TableInfo is not synchronized with underlying database.");
                        return;
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show(
                        "Unable to check synchronization of TableInfo with underlying database (this check must be performed before checking ANO synchronization):" +
                        exception.Message);
                    return;
                }
            }

            var ANOSynchronizer = new ANOTableInfoSynchronizer(tableInfo);

            try
            {
                ANOSynchronizer.Synchronize(new MakeChangePopup(new YesNoYesToAllDialog()));

                MessageBox.Show("ANO synchronization successful");
            }
            catch (ANOConfigurationException e)
            {
                ExceptionViewer.Show(e);
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show("Fatal error while attempting to synchronize (" + exception.Message + ")", exception);
            }

            _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(tableInfo));
        }

        private void TableInfo_Click(TableInfo tableInfo)
        {
            try
            {
                TableInfoSynchronizer syncher = new TableInfoSynchronizer(tableInfo);
                bool wasSynchedsuccessfully = syncher.Synchronize(new MakeChangePopup(new YesNoYesToAllDialog()));

                if (wasSynchedsuccessfully)
                    MessageBox.Show("Synchronization complete, TableInfo is Synchronized with the live database");
                else
                    MessageBox.Show("Synchronization failed");
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }

            _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(tableInfo));
        }

        private void AddColumnInfo_Click(TableInfo tableInfo)
        {
            var newColumnInfo = new ColumnInfo(RepositoryLocator.CatalogueRepository, Guid.NewGuid().ToString(), "fish", tableInfo);
            _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(newColumnInfo));
        }
        
        private void ConfigureTableInfoParameters(TableInfo tableInfo)
        {
            ParameterCollectionUI.ShowAsDialog(new ParameterCollectionUIOptionsFactory().Create(tableInfo));
        }

        private void CreateBackupTrigger_Click(TableInfo tableInfo)
        {
            try
            {
                var checks = new PopupChecksUI("Implementing archive trigger on table " + tableInfo, true);

                TableInfoSynchronizer synchronizer = new TableInfoSynchronizer(tableInfo);
                if (synchronizer.Synchronize(checks))
                {
                    var pks = tableInfo.ColumnInfos.Where(col => col.IsPrimaryKey).ToArray();

                    if (!pks.Any())
                        MessageBox.Show("Your table does not have any primary keys so cannot support an archive trigger");

                    if (MessageBox.Show("Are you sure you want to create a backup triggered _Archive table using the following primary keys?:" + Environment.NewLine + string.Join("" + Environment.NewLine, pks.Select(p => p.GetRuntimeName())), "Confirm Creating Archive?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {

                        var db = DataAccessPortal.GetInstance().ExpectDatabase(tableInfo, DataAccessContext.InternalDataProcessing);

                        TriggerImplementer implementer = new TriggerImplementer(db, tableInfo.GetRuntimeName());
                        implementer.CreateTrigger(pks.Select(col => col.GetRuntimeName()).ToArray(), checks);
                        MessageBox.Show("Success, look for the new table " + tableInfo.GetRuntimeName() + "_Archive which will contain old records whenever there is an update");
                    }
                }
            }
            catch (Exception e)
            {
                ExceptionViewer.Show(e);
            }
        }


        
    }
}
