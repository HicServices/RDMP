using System.Collections.Generic;
using CatalogueLibrary.Data.Cache;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Describes a period of time in which a given act can take place (e.g. only cache data from the MRI imaging web service during the hours of 11pm - 5am so as not to 
    /// disrupt routine hospital use).  Also serves as a Locking point for job control.  Once an IPermissionWindow is in use by a process (e.g. Caching Pipeline) then it
    /// is not available to other processes (e.g. loading or other caching pipelines that share the same IPermissionWindow).
    /// </summary>
    public interface IPermissionWindow : IMapsDirectlyToDatabaseTable, ISaveable, IDeleteable,ILockable
    {
        string Name { get; set; }
        string Description { get; set; }
        bool RequiresSynchronousAccess { get; set; }
        List<PermissionWindowPeriod> PermissionWindowPeriods { get; }

        bool CurrentlyWithinPermissionWindow();
        IEnumerable<ICacheProgress> GetAllCacheProgresses();
        void SetPermissionWindowPeriods(List<PermissionWindowPeriod> windowPeriods);
    }
}