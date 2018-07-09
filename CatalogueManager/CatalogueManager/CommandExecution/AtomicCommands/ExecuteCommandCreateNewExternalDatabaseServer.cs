using System;
using System.Drawing;
using System.Reflection;
using CatalogueLibrary.Data;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using MapsDirectlyToDatabaseTableUI;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCreateNewExternalDatabaseServer : BasicUICommandExecution,IAtomicCommand
    {
        private readonly Assembly _databaseAssembly;
        private readonly ServerDefaults.PermissableDefaults _defaultToSet;

        public ExternalDatabaseServer ServerCreatedIfAny { get; private set; }
        
        public ExecuteCommandCreateNewExternalDatabaseServer(IActivateItems activator, Assembly databaseAssembly, ServerDefaults.PermissableDefaults defaultToSet) : base(activator)
        {
            _databaseAssembly = databaseAssembly;
            _defaultToSet = defaultToSet;

            //do we already have a default server for this?
            var defaults = new ServerDefaults(Activator.RepositoryLocator.CatalogueRepository);
            var existingDefault = defaults.GetDefaultFor(_defaultToSet);
            
            if(existingDefault != null)
                SetImpossible("There is already an existing " + _defaultToSet + " database");
        }

        public override string GetCommandName()
        {
            return OverrideCommandName?? base.GetCommandName();
        }

        public override void Execute()
        {
            base.Execute();
            
            //user wants to create a new server e.g. a new Logging server
            if (_databaseAssembly == null)
                ServerCreatedIfAny = new ExternalDatabaseServer(Activator.RepositoryLocator.CatalogueRepository,"New ExternalDatabaseServer " + Guid.NewGuid());
            else
            {
                //create the new server
                ServerCreatedIfAny = CreatePlatformDatabase.CreateNewExternalServer(
                    Activator.RepositoryLocator.CatalogueRepository,
                    _defaultToSet,
                    _databaseAssembly);   
            }

            //user cancelled creating a server
            if (ServerCreatedIfAny == null)
                return;
            
            Publish(ServerCreatedIfAny);
            Activate(ServerCreatedIfAny);
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.ExternalDatabaseServer, OverlayKind.Add);
        }
    }
}