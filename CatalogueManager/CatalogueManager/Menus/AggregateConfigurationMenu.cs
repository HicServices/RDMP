using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Repositories;
using CatalogueManager.Collections;
using CatalogueManager.Collections.Providers;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.DataViewing.Collections;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using MapsDirectlyToDatabaseTableUI;
using RDMPStartup;
using ReusableLibraryCode;
using ReusableUIComponents.ChecksUI;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    public class AggregateConfigurationMenu :RDMPContextMenuStrip
    {
        private readonly AggregateConfiguration _aggregate;

        public AggregateConfigurationMenu(RDMPContextMenuStripArgs args, AggregateConfiguration aggregate): base(args, aggregate)
        {
            _aggregate = aggregate;
            
            Items.Add("View Sample", GetImage(RDMPConcept.SQL,OverlayKind.Execute), ViewDatasetSample);

            //only allow them to execute graph if it is normal aggregate graph
            if (!aggregate.IsCohortIdentificationAggregate)
                Add(new ExecuteCommandExecuteAggregateGraph(_activator, aggregate));

            Items.Add("View Checks", CatalogueIcons.Warning, (s, e) => new PopupChecksUI("Checking " + aggregate, false).Check(aggregate));

            var addFilterContainer = new ToolStripMenuItem("Add Filter Container", GetImage(RDMPConcept.FilterContainer, OverlayKind.Add), (s, e) => AddFilterContainer());

            //if it doesn't have a root container or a hijacked container shortcut
            addFilterContainer.Enabled = aggregate.RootFilterContainer_ID == null && aggregate.OverrideFiltersByUsingParentAggregateConfigurationInstead_ID == null;
            Items.Add(addFilterContainer);

            var addShortcutFilterContainer = new ToolStripMenuItem("Create Shortcut to Another AggregateConfigurations Filter Container",
                GetImage(aggregate, OverlayKind.Shortcut), (s, e) => ChooseHijacker());

            
            //if it doesn't have a root container or a hijacked container shortcut
            addShortcutFilterContainer.Enabled = aggregate.RootFilterContainer_ID == null && aggregate.OverrideFiltersByUsingParentAggregateConfigurationInstead_ID == null;

            Items.Add(addShortcutFilterContainer);

            var clearShortcutFilterContainer = new ToolStripMenuItem("Clear Shortcut", GetImage(aggregate, OverlayKind.Shortcut), (s, e) => ClearShortcut());
            clearShortcutFilterContainer.Enabled = aggregate.OverrideFiltersByUsingParentAggregateConfigurationInstead_ID != null;
            Items.Add(clearShortcutFilterContainer);
        }

        private void ViewDatasetSample(object sender,EventArgs e)
        {
            var cic = _aggregate.GetCohortIdentificationConfigurationIfAny();
            
            var collection = new ViewAggregateExtractUICollection(_aggregate);

            //if it has a cic with a query cache AND it uses joinables.  Since this is a TOP 100 select * from dataset the cache on CHI is useless only patient index tables used by this query are useful if cached
            if (cic != null && cic.QueryCachingServer_ID != null && _aggregate.PatientIndexJoinablesUsed.Any())
            {
                switch (MessageBox.Show("Use Query Cache when building query?", "Use Configured Cache", MessageBoxButtons.YesNoCancel))
                {
                    case DialogResult.Cancel:
                        return;
                    case DialogResult.Yes:
                        collection.UseQueryCache = true;
                        break;
                    case DialogResult.No:
                        collection.UseQueryCache = false;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            _activator.ViewDataSample(collection);
        }

        private void ClearShortcut()
        {
            _aggregate.OverrideFiltersByUsingParentAggregateConfigurationInstead_ID = null;
            _aggregate.SaveToDatabase();
            Publish(_aggregate);
        }

        private void ChooseHijacker()
        {
            var others
             =
                //get all configurations
             _aggregate.Repository.GetAllObjects<AggregateConfiguration>().Where(a =>
                 //which are not themselves already shortcuts!
                 a.OverrideFiltersByUsingParentAggregateConfigurationInstead_ID == null
                 &&
                     //and which have a filter set!
                 a.RootFilterContainer_ID != null)
                //and are not ourself!
                 .Except(new[] { _aggregate }).ToArray();

            SelectIMapsDirectlyToDatabaseTableDialog dialog = new SelectIMapsDirectlyToDatabaseTableDialog(others, true, false);

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (dialog.Selected == null)
                    _aggregate.OverrideFiltersByUsingParentAggregateConfigurationInstead_ID = null;
                else
                    _aggregate.OverrideFiltersByUsingParentAggregateConfigurationInstead_ID =
                        ((AggregateConfiguration) dialog.Selected).ID;

                _aggregate.SaveToDatabase();
                Publish(_aggregate);
            }
        
        }

        private void AddFilterContainer()
        {
            var newContainer = new AggregateFilterContainer(RepositoryLocator.CatalogueRepository, FilterContainerOperation.AND);
            _aggregate.RootFilterContainer_ID = newContainer.ID;
            _aggregate.SaveToDatabase();
            Publish(_aggregate);
        }
    }
}
