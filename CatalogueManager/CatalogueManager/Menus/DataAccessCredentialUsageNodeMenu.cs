using System;
using System.Windows.Forms;
using CatalogueLibrary.Nodes;
using CatalogueLibrary.Repositories;
using CatalogueManager.Collections;
using CatalogueManager.Collections.Providers;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
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
    class DataAccessCredentialUsageNodeMenu : RDMPContextMenuStrip
    {
        public DataAccessCredentialUsageNodeMenu(RDMPContextMenuStripArgs args, DataAccessCredentialUsageNode node): base(args, node)
        {
            var setUsage = new ToolStripMenuItem("Set Context");

            var existingUsages = _activator.RepositoryLocator.CatalogueRepository.TableInfoToCredentialsLinker.GetCredentialsIfExistsFor(node.TableInfo);

            foreach (DataAccessContext context in Enum.GetValues(typeof(DataAccessContext)))
                Add(new ExecuteCommandSetDataAccessContextForCredentials(_activator, node, context, existingUsages), Keys.None, setUsage);
            
            Items.Add(setUsage);

            Add(new ExecuteCommandViewDependencies(node.Credentials,new CatalogueObjectVisualisation(_activator.CoreIconProvider)));
        }
    }
}