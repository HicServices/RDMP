using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CatalogueLibrary;
using CatalogueLibrary.Data.DataLoad;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace DataLoadEngine.LoadExecution.Components.Arguments
{
    public class  StageArgs : IStageArgs
    {
        //Optional
        public DiscoveredDatabase DbInfo { get; private set; }
        public IHICProjectDirectory RootDir { get; private set; }
        
        //Mandatory
        public LoadStage LoadStage { get; private set; }

        public StageArgs(LoadStage loadStage,DiscoveredDatabase database, IHICProjectDirectory projectDirectory)
        {
            LoadStage = loadStage;
            DbInfo = database;
            RootDir = projectDirectory;
        }

        public Dictionary<string, object> ToDictionary()
        {
            var props = GetType().GetProperties();
            return props.ToDictionary(propertyInfo => propertyInfo.Name, propertyInfo => propertyInfo.GetValue(this, null));
        }
    }
}