using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Reflection;
using CatalogueLibrary.Data;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.Icons.IconProvision.StateBasedIconProviders;
using CatalogueManager.ItemActivation;
using MapsDirectlyToDatabaseTableUI;
using ReusableLibraryCode;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCreateNewExternalDatabaseServer : BasicUICommandExecution,IAtomicCommand
    {
        private readonly Assembly _databaseAssembly;
        private readonly ServerDefaults.PermissableDefaults _defaultToSet;
        private ExternalDatabaseServerStateBasedIconProvider _databaseIconProvider;
        private IconOverlayProvider _overlayProvider;

        public ExternalDatabaseServer ServerCreatedIfAny { get; private set; }


        [ImportingConstructor]
        public ExecuteCommandCreateNewExternalDatabaseServer(IActivateItems activator) : this(activator,null,ServerDefaults.PermissableDefaults.None)
        {
            
        }

        public ExecuteCommandCreateNewExternalDatabaseServer(IActivateItems activator, Assembly databaseAssembly, ServerDefaults.PermissableDefaults defaultToSet) : base(activator)
        {
            _databaseAssembly = databaseAssembly;
            _defaultToSet = defaultToSet;
            
            _overlayProvider = new IconOverlayProvider();
            _databaseIconProvider = new ExternalDatabaseServerStateBasedIconProvider(_overlayProvider);

            //do we already have a default server for this?
            var existingDefault = Activator.ServerDefaults.GetDefaultFor(_defaultToSet);
            
            if(existingDefault != null)
                SetImpossible("There is already an existing " + _defaultToSet + " database");
        }

        public override string GetCommandName()
        {
            if(_defaultToSet != ServerDefaults.PermissableDefaults.None)
                return string.Format("Create New {0} Server...", UsefulStuff.PascalCaseStringToHumanReadable(_defaultToSet.ToString().Replace("_ID", "").Replace("Live", "").Replace("ANO", "Anonymisation")));

            if (_databaseAssembly != null)
                return string.Format("Create New {0} Server...", _databaseAssembly.GetName().Name);

            return base.GetCommandName();
        }

        public override string GetCommandHelp()
        {
            switch (_defaultToSet)
            {
                case ServerDefaults.PermissableDefaults.LiveLoggingServer_ID:
                    return "Creates a database for auditing all flows of data (data load, extraction etc) including tables for errors, progress tables/record count loaded etc";
                case ServerDefaults.PermissableDefaults.TestLoggingServer_ID:
                    return "Deprecated, do not create TestLoggingServers";
                case ServerDefaults.PermissableDefaults.IdentifierDumpServer_ID:
                    return "Creates a database for storing the values of intercepted columns that are discarded during data load because they contain identifiable data";
                case ServerDefaults.PermissableDefaults.DQE:
                    return "Creates a database for storing the results of data quality engine runs on your datasets over time.";
                case ServerDefaults.PermissableDefaults.WebServiceQueryCachingServer_ID:
                    break;
                case ServerDefaults.PermissableDefaults.RAWDataLoadServer:
                    return "Defines which database server should be used for the RAW data in the RAW=>STAGING=>LIVE model of the data load engine";
                case ServerDefaults.PermissableDefaults.ANOStore:
                    return "Creates a new anonymisation database which contains mappings of identifiable values to anonymous representations";
                case ServerDefaults.PermissableDefaults.CohortIdentificationQueryCachingServer_ID:
                    return "Creates a new Query Cache database which contains the indexed results of executed subqueries in a CohortIdentificationConfiguration";
            }

            return "Defines a new server that can be accessed by RDMP";
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
            if(_databaseAssembly != null)
            {
                var basicIcon = _databaseIconProvider.GetIconForAssembly(_databaseAssembly);
                return _overlayProvider.GetOverlay(basicIcon, OverlayKind.Add);
            }

            return iconProvider.GetImage(RDMPConcept.ExternalDatabaseServer, OverlayKind.Add);
        }
    }
}