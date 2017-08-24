using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CatalogueLibrary.Data.DataLoad;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace DataLoadEngine.LoadExecution.Components.Arguments
{
    public class  StageArgs : IStageArgs
    {
        //Optional
        public DiscoveredDatabase DbInfo { get; set; }
        public string RootDir { get; set; }
        
        //Mandatory
        public bool TestLoad { get; set; }
        public LoadStage LoadStage { get; set; }

        public StageArgs(LoadStage loadStage, bool isTestLoad)
        {
            LoadStage = loadStage;
            TestLoad = isTestLoad;
        }

        public Dictionary<string, object> ToDictionary()
        {
            var props = GetType().GetProperties();
            return props.ToDictionary(propertyInfo => propertyInfo.Name, propertyInfo => propertyInfo.GetValue(this, null));
        }

    }
}