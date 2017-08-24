using System.Collections.Generic;
using CatalogueLibrary.Data.DataLoad;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace DataLoadEngine.LoadExecution.Components.Arguments
{
    public interface IStageArgs
    {
        DiscoveredDatabase DbInfo { get; set; }
        Dictionary<string, object> ToDictionary();

        string RootDir { get; set; }
        LoadStage LoadStage { get; set; }
    }
}