using System;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.DataLoad;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace CatalogueLibrary.Data
{
    public interface ILoadProgress :ISaveable, ILockable
    {
        int ID { get; }
        string Name { get; set; }
        DateTime? OriginDate { get; set; }
        DateTime? DataLoadProgress { get; set; }
        int LoadMetadata_ID { get; set; }

        TimeSpan GetLoadPeriodicity();
        void SetLoadPeriodicity(TimeSpan loadPeriod);
        ICacheProgress GetCacheProgress();

        ILoadMetadata GetLoadMetadata();

        //Dictionary<DateTime, FileInfo> GetDataLoadWorkload(IHICProjectDirectory root, int batchSize);

        bool IsDisabled { get; set; }
        IRepository Repository { get; }

        int DefaultNumberOfDaysToLoadEachTime { get; }
    }
}