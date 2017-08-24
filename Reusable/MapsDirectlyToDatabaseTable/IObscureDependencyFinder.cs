using System;

namespace MapsDirectlyToDatabaseTable
{
    public interface IObscureDependencyFinder
    {
        void ThrowIfDeleteDisallowed(IMapsDirectlyToDatabaseTable oTableWrapperObject);

        /// <summary>
        /// oTableWrapperObject has just been deleted, you must now deal with the fact that oTableWrapperObject no longer exists (e.g. tidy up orphans etc).  Do not attempt to Save or Delete
        /// or really do anything much with oTableWrapperObject because it no longer exists in the database.
        /// </summary>
        /// <param name="oTableWrapperObject"></param>
        void HandleCascadeDeletesForDeletedObject(IMapsDirectlyToDatabaseTable oTableWrapperObject);
    }
}