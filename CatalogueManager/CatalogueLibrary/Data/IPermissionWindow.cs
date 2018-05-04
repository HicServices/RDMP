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
    public interface IPermissionWindow : IMapsDirectlyToDatabaseTable, ISaveable, IDeleteable,ILockable
    {
        string Name { get; set; }
        string Description { get; set; }
        bool RequiresSynchronousAccess { get; set; }
        IEnumerable<ICacheProgress> CacheProgresses { get; }
        List<PermissionWindowPeriod> PermissionWindowPeriods { get; }

        bool WithinPermissionWindow();
        bool WithinPermissionWindow(DateTime dateTimeUTC);

        void SetPermissionWindowPeriods(List<PermissionWindowPeriod> windowPeriods);
    }
}