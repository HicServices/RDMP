using System;
using System.Collections.Generic;
using System.Linq;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace CatalogueLibrary.Data.DataLoad
{
    public interface ILoadMetadata : IMapsDirectlyToDatabaseTable,ILoadProgressHost
    {
        string LocationOfFlatFiles { get; set; }
        string Name { get; }
        
        IEnumerable<ICatalogue> GetAllCatalogues();

        void SaveToDatabase();

        DiscoveredServer GetDistinctLoggingDatabaseSettings(bool isTest);
        string GetDistinctLoggingTask();

        DiscoveredServer GetDistinctLiveDatabaseServer();
        string GetDistinctDatabaseName();

        ILoadProgress[] LoadProgresses { get; }
        IOrderedEnumerable<ProcessTask> ProcessTasks { get; }
        LoadPeriodically LoadPeriodically { get; }
        IEnumerable<ProcessTask> GetAllProcessTasks(bool includeDisabled);
    }
}