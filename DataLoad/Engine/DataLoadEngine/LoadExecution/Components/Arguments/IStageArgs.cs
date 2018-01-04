using System.Collections.Generic;
using CatalogueLibrary;
using CatalogueLibrary.Data.DataLoad;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace DataLoadEngine.LoadExecution.Components.Arguments
{
    /// <summary>
    /// See StageArgs
    /// </summary>
    public interface IStageArgs
    {
        DiscoveredDatabase DbInfo { get;}
        Dictionary<string, object> ToDictionary();

        IHICProjectDirectory RootDir { get; }
        LoadStage LoadStage { get;}
    }
}