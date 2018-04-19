using CatalogueLibrary.Data.Dashboarding;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;

namespace CatalogueManager.TestsAndSetup.ServicePropogation
{
    /// <summary>
    /// Only use if you know what you are doing.  What you are doing is anouncing that you cannot function on a single root database object alone (e.g. Project / ExtractionConfiguration etc).
    /// and that you require a combination of objects and/or custom settings to be persisted/refreshed.  If you can manage with only one object (which you really should be able to) then use
    /// RDMPSingleDatabaseObjectControl instead which is much easier to implement
    /// 
    /// <para>IObjectCollectionControls are controls driven by 0 or more database objects and optional persistence string (stored in an IPersistableObjectCollection).  The lifecycle of the control
    /// is that it is Activated (probably by an IActivateItems control class) with a fully hydrated IPersistableObjectCollection.  This collection should be pretty immutable and will be saved
    /// into the persistence text file when the application is exited (via PersistableObjectCollectionDockContent)</para>
    /// 
    /// </summary>
    public interface IObjectCollectionControl:ILifetimeSubscriber,INamedTab
    {
        /// <summary>
        /// Provides a fully hydrated collection either created by a user action or by deserializing a persistence string in PersistableObjectCollectionDockContent.  Either way the
        /// collection will be fully hydrated.
        /// </summary>
        /// <param name="activator"></param>
        /// <param name="collection"></param>
        void SetCollection(IActivateItems activator, IPersistableObjectCollection collection);

        /// <summary>
        /// Used to serialize the control for later use e.g. on application exit, you must only return your collection, the rest is handled by the IPersistableObjectCollection it'self or
        /// the PersistableObjectCollectionDockContent
        /// </summary>
        /// <returns></returns>
        IPersistableObjectCollection GetCollection();
    }
}
