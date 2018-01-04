using System.Collections.Generic;
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
    public interface IStageArgs
    {
        DiscoveredDatabase DbInfo { get;}
        Dictionary<string, object> ToDictionary();

        IHICProjectDirectory RootDir { get; }
        LoadStage LoadStage { get;}
    }
}