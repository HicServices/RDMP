using System.Windows.Forms;
using CatalogueLibrary.Nodes;
using CatalogueLibrary.Repositories;
using CatalogueManager.Collections;
using CatalogueManager.Collections.Providers;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.ObjectVisualisation;
using CatalogueManager.Refreshing;
using RDMPStartup;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;
using ReusableUIComponents.CommandExecution;
using ReusableUIComponents.CommandExecution.AtomicCommands;
using ReusableUIComponents.Dependencies;

namespace CatalogueManager.Menus
{
    public class DataAccessCredentialUsageNodeMenu : RDMPContextMenuStrip
    {
        public DataAccessCredentialUsageNodeMenu(IActivateItems activator, DataAccessCredentialUsageNode node, RDMPCollectionCommonFunctionality collection):base(activator,null, collection)
        {
            var setUsage = new ToolStripMenuItem("Set Context");

            AddMenuItem(setUsage, DataAccessContext.Any, node);
            AddMenuItem(setUsage, DataAccessContext.DataExport, node);
            AddMenuItem(setUsage, DataAccessContext.DataLoad, node);
            AddMenuItem(setUsage, DataAccessContext.InternalDataProcessing, node);
            AddMenuItem(setUsage, DataAccessContext.Logging, node);
            
            Items.Add(setUsage);

            Add(new ExecuteCommandViewDependencies(node.Credentials,new CatalogueObjectVisualisation(activator.CoreIconProvider)));
        }

        private void AddMenuItem(ToolStripMenuItem setUsage, DataAccessContext context, DataAccessCredentialUsageNode node)
        {
            var newItem = new ToolStripMenuItem(context.ToString(), null, (s, e) => SetContext(context, node));
            newItem.Enabled = node.Context != context;
            setUsage.DropDownItems.Add(newItem);
        }

        private void SetContext(DataAccessContext destinationContext, DataAccessCredentialUsageNode node)
        {
            RepositoryLocator.CatalogueRepository.TableInfoToCredentialsLinker.SetContextFor(node,destinationContext);
            Publish(node.TableInfo);

        }
    }
}