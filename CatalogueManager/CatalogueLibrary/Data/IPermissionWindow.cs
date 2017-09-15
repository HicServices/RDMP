using System.Collections.Generic;
using CatalogueLibrary.Data.Cache;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace CatalogueLibrary.Data
{
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