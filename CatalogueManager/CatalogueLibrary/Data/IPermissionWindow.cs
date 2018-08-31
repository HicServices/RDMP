using System;
using System.Collections.Generic;
using CatalogueLibrary.Data.Cache;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// See PermissionWindow
    /// </summary>
    public interface IPermissionWindow : INamed
    {
        /// <summary>
        /// Human readable description of the period of time described e.g. 'weekday nights only because we dont want to hit local server backup periods'
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Obsolete
        /// </summary>
        [Obsolete("NotUsed")]
        bool RequiresSynchronousAccess { get; set; }

        /// <summary>
        /// All caching activities which are restricted to running in this time window
        /// </summary>
        IEnumerable<ICacheProgress> CacheProgresses { get; }

        /// <summary>
        /// The time windows that the activity is allowed in
        /// </summary>
        List<PermissionWindowPeriod> PermissionWindowPeriods { get; }

        /// <summary>
        /// Returns true if the current time is within one of the <see cref="PermissionWindowPeriods"/>
        /// </summary>
        /// <returns></returns>
        bool WithinPermissionWindow();

        /// <inheritdoc cref="WithinPermissionWindow()"/>
        bool WithinPermissionWindow(DateTime dateTimeUTC);

        /// <summary>
        /// Sets the time of day that activities are permitted in
        /// </summary>
        /// <param name="windowPeriods"></param>
        void SetPermissionWindowPeriods(List<PermissionWindowPeriod> windowPeriods);
    }
}