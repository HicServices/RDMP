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
    /// <summary>
    /// Identifies the database target of a given DLE LoadStage (e.g. AdjustRaw would contain a DiscoveredDatabase pointed at the RAW database). Also includes
    /// the location of the load directory
    /// </summary>
    public class  StageArgs : IStageArgs
    {
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