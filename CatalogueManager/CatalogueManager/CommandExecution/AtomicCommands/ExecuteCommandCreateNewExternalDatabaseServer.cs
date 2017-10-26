using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using MapsDirectlyToDatabaseTableUI;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;
using ReusableUIComponents.CommandExecution.AtomicCommands;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCreateNewExternalDatabaseServer : BasicCommandExecution,IAtomicCommand
    {
        private readonly IActivateItems _activator;
        private readonly Assembly _databaseAssembly;
        private readonly ServerDefaults.PermissableDefaults _defaultToSet;

        public ExternalDatabaseServer ServerCreatedIfAny { get; private set; }
        
        public string OverrideCommandName { get; set; }

        public ExecuteCommandCreateNewExternalDatabaseServer(IActivateItems activator, Assembly databaseAssembly, ServerDefaults.PermissableDefaults defaultToSet)
        {
            _activator = activator;
            _databaseAssembly = databaseAssembly;
            _defaultToSet = defaultToSet;
        }

        public override string GetCommandName()
        {
            return OverrideCommandName?? base.GetCommandName();
        }

        public override void Execute()
        {
            base.Execute();
            
            //user wants to create a new server e.g. a new Logging server

            //do we already have a default server for this?
            var defaults = new ServerDefaults(_activator.RepositoryLocator.CatalogueRepository);
            var existingDefault = defaults.GetDefaultFor(_defaultToSet);

            //create the new server
            ServerCreatedIfAny = CreatePlatformDatabase.CreateNewExternalServer(
                _activator.RepositoryLocator.CatalogueRepository,

                //if we already have an existing default of this type then don't set the default yet
                existingDefault != null ? ServerDefaults.PermissableDefaults.None : _defaultToSet,
                _databaseAssembly);

            //user cancelled creating a server
            if (ServerCreatedIfAny == null)
                return;

            if (existingDefault != null)
            {
                if (MessageBox.Show(
                    "Would you like the new default '" + _defaultToSet +
                    "' server to the newly created server? The current default is '" + existingDefault + "'",
                    "Overwrite Default", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    defaults.SetDefault(_defaultToSet, ServerCreatedIfAny);
            }

            _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(ServerCreatedIfAny));
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.ExternalDatabaseServer, OverlayKind.Add);
        }
    }
}