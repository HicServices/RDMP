using System;
using System.Reflection;
using System.Windows.Forms;
using CatalogueLibrary.Data.Dashboarding;
using CatalogueLibrary.Repositories;
using CatalogueLibrary.Repositories.Construction;
using CatalogueManager.LoadExecutionUIs;
using Dashboard.Raceway;
using DataExportManager.ProjectUI;
using MapsDirectlyToDatabaseTable;
using RDMPStartup;

namespace ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking.Persistence
{
    /// <summary>
    /// Translates persistence strings into DeserializeInstructions for restoring the RDMP main application window layout after application close/restart.
    /// </summary>
    public class PersistenceDecisionFactory
    {
        public const char Separator = ':';

        public PersistenceDecisionFactory()
        {
            //ensure dashboard UI assembly is loaded
            Assembly.Load(typeof (RacewayRenderAreaUI).Assembly.FullName);
            //ensure data export UI assembly is loaded
            Assembly.Load(typeof(ExtractionConfigurationUI).Assembly.FullName);
            //ensure DLE UI assembly is loaded
            Assembly.Load(typeof(ExecuteLoadMetadataUI).Assembly.FullName);
        }

        public RDMPCollection? ShouldCreateCollection(string persistString)
        {
            if (!persistString.StartsWith(PersistableToolboxDockContent.Prefix))
                return null;

            var tokens = persistString.Split(Separator);

            if (tokens.Length != 2)
                return null;

            RDMPCollection collection;
            Enum.TryParse(tokens[1], true, out collection);
            return collection;
            
        }

        public DeserializeInstruction ShouldCreateSingleObjectControl(string persistString, IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {
            if (!persistString.StartsWith(PersistableSingleDatabaseObjectDockContent.Prefix))
                return null;

            //return Prefix + s + _control.GetType().Name + s + _databaseObject.Repository.GetType() +  s + _databaseObject.GetType().Name + s + _databaseObject.ID;
            var tokens = persistString.Split(Separator);

            if (tokens.Length != 5)
                throw new PersistenceException("Unexpected number of tokens (" + tokens.Length + ") for Persistence of Type " + PersistableSingleDatabaseObjectDockContent.Prefix);
            
            Type controlType = GetTypeByName(tokens[1], typeof(Control), repositoryLocator);
            IMapsDirectlyToDatabaseTable o = repositoryLocator.GetArbitraryDatabaseObject(tokens[2], tokens[3], int.Parse(tokens[4]));
            
            return new DeserializeInstruction(controlType,o);

        }

        public DeserializeInstruction ShouldCreateObjectCollection(string persistString, IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {
            if (!persistString.StartsWith(PersistableObjectCollectionDockContent.Prefix))
                return null;
            
            if(!persistString.Contains(PersistableObjectCollectionDockContent.ExtraText))
                throw new PersistenceException("Persistence string did not contain '" + PersistableObjectCollectionDockContent.ExtraText);

            //Looks something like this  RDMPObjectCollection:MyCoolControlUI:MyControlUIsBundleOfObjects:[CatalogueRepository:AggregateConfiguration:105,CatalogueRepository:AggregateConfiguration:102,CatalogueRepository:AggregateConfiguration:101]###EXTRA_TEXT###I've got a lovely bunch of coconuts
            var tokens = persistString.Split(Separator);
            
            var uiType = GetTypeByName(tokens[1],typeof(Control),repositoryLocator);
            var collectionType = GetTypeByName(tokens[2], typeof (IPersistableObjectCollection), repositoryLocator);

            ObjectConstructor objectConstructor = new ObjectConstructor();
            IPersistableObjectCollection collectionInstance = (IPersistableObjectCollection)objectConstructor.Construct(collectionType);
                
            if(collectionInstance.DatabaseObjects == null)
                throw new PersistenceException("Constructor of Type '" +collectionType + "' did not initialise property DatabaseObjects");

            var allObjectsString = PersistableObjectCollectionDockContent.MatchCollectionInString(persistString);
            var objectStrings = allObjectsString.Split(new []{PersistableObjectCollectionDockContent.CollectionObjectSeparator},StringSplitOptions.RemoveEmptyEntries);

            foreach (string objectString in objectStrings)
            {
                var objectTokens = objectString.Split(Separator);

                if(objectTokens.Length != 3)
                    throw new PersistenceException("Could not figure out what database object to fetch because the list contained an item with an invalid number of tokens (" + objectTokens.Length + " tokens).  The current object string is:"+Environment.NewLine + objectString + " The current persistence string is:"+Environment.NewLine + persistString);

                var dbObj = repositoryLocator.GetArbitraryDatabaseObject(objectTokens[0], objectTokens[1], int.Parse(objectTokens[2]));

                if (dbObj != null)
                    collectionInstance.DatabaseObjects.Add(dbObj);
                else
                    throw new PersistenceException("DatabaseObject '" + objectString +
                                                   "' has been deleted meaning IPersistableObjectCollection " +
                                                   collectionInstance.GetType().Name +
                                                   " could not be properly created/populated");
            }
            
            var extraText = persistString.Substring(persistString.IndexOf(PersistableObjectCollectionDockContent.ExtraText, System.StringComparison.Ordinal) + PersistableObjectCollectionDockContent.ExtraText.Length);
            collectionInstance.LoadExtraText(extraText);

            return new DeserializeInstruction(uiType,collectionInstance);
        }

        private Type GetTypeByName(string s, Type expectedBaseClassType,IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {
            var toReturn = repositoryLocator.CatalogueRepository.MEF.GetTypeByNameFromAnyLoadedAssembly(s);

            if (toReturn == null)
                throw new TypeLoadException("Could not find Type called '" + s + "'");

            if (expectedBaseClassType != null)
                if (!expectedBaseClassType.IsAssignableFrom(toReturn))
                    throw new TypeLoadException("Persistence string included a reference to Type '" + s + "' which we managed to find but it did not match an expected base Type (" + expectedBaseClassType + ")");

            return toReturn;
        }
    
    }
}
