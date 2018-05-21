using System;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.DataLoad;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// See LoadProgress
    /// </summary>
    public interface ILoadProgress :ISaveable, ILockable,IMapsDirectlyToDatabaseTable
    {
        string Name { get; set; }
        DateTime? OriginDate { get; set; }
        DateTime? DataLoadProgress { get; set; }
        int LoadMetadata_ID { get; set; }

        bool AllowAutomation { get; }

        TimeSpan GetLoadPeriodicity();
        void SetLoadPeriodicity(TimeSpan loadPeriod);
        
        ILoadMetadata LoadMetadata { get; }
        ICacheProgress CacheProgress { get; }

        //Dictionary<DateTime, FileInfo> GetDataLoadWorkload(IHICProjectDirectory root, int batchSize);

        bool IsDisabled { get; set; }
        
        int DefaultNumberOfDaysToLoadEachTime { get; }
    }
}