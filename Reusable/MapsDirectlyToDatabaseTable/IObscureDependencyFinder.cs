using System;

namespace MapsDirectlyToDatabaseTable
{
    /// <summary>
    /// Provides cross database/server referential integrity and referential integrity where the dependency requires programming logic (for example preventing
    /// deleting objects that are referenced in Catalogue.ValidatorXML).
    /// 
    /// Also handles CASCADEing between databases/servers as above (when a delete is permitted but it means other objects should suddenly also be deleted).
    /// 
    /// IObscureDependencyFinders are global hooks that are installed into an IRepository (usually a TableRepository) which ensure that the user cannot do 
    /// dangerous deletes / leave orphan objects where simple database level logic cannot be implemented to enforce the rule (e.g. foreign key constraints).
    /// </summary>
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