using System;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.DataLoad;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Describes the progress of a long term epic data load operation which cannot be completed in a single Data load bubble (execution of LoadMetadata through the data load engine).
    /// This entity includes start and end dates for what is trying to be loaded as well as how far through that process progess has been made up to.
    /// </summary>
    public interface ILoadProgress :INamed
    {
        /// <summary>
        /// The date the dataset starts at, this is in dataset time e.g. if you have prescribing records held from 2001-01-01 to present then the <see cref="OriginDate"/> is 2001-01-01
        /// </summary>
        DateTime? OriginDate { get; set; }

        /// <summary>
        /// Records how far through the process of loading data into this dataset.  This is updated at the end of a successful data load for a given date range (E.g. the next 10 days
        /// due to be loaded) 
        /// </summary>
        DateTime? DataLoadProgress { get; set; }

        /// <summary>
        /// The data load that this object records progress for.  You can have multiple <see cref="ILoadProgress"/> for a single <see cref="ILoadMetadata"/> (data load) for example you 
        /// might have loaded Tayside data into biochemistry up to 2017-01-01 but for Fife you have only loaded data up to 2015-01-01 so far.
        /// </summary>
        int LoadMetadata_ID { get; set; }

        /// <inheritdoc cref="LoadMetadata_ID"/>
        ILoadMetadata LoadMetadata { get; }

        /// <summary>
        /// If the data load involves iteratively loading dat from a date based cache of fetched data then this will be the <see cref="ICacheProgress"/> which is responsible for fetching
        /// and saving the cached data to disk (this occurs seperately from the data loading).
        /// </summary>
        ICacheProgress CacheProgress { get; }

        
        /// <summary>
        /// Do not use, is not respected
        /// </summary>
        [Obsolete("Not respected")]
        bool IsDisabled { get; set; }
        
        /// <summary>
        /// The number of days to load each time the DLE is run with the <see cref="ILoadProgress"/>
        /// </summary>
        int DefaultNumberOfDaysToLoadEachTime { get; }
    }
}