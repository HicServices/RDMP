using CatalogueLibrary.Repositories;

namespace CatalogueLibrary.Data.DataLoad
{
    /// <summary>
    /// Powers the creation of instances of T in an ICustomUI
    /// </summary>
    public interface ICustomUI<T> : ICustomUI where T:ICustomUIDrivenClass
    {
        /// <summary>
        /// Loads the current value into the user interface
        /// </summary>
        /// <param name="value"></param>
        /// <param name="previewIfAvailable"></param>
        void SetUnderlyingObjectTo(T value, System.Data.DataTable previewIfAvailable);
    }

    /// <summary>
    /// Interface that lets you create UIs for populating <see cref="IArgument"/> values for Properties which are too complicated to do with basic Types.  See <see cref="ICustomUIDrivenClass"/>.  If
    /// If at all possible you should avoid the overhead of this system and instead use [DemandsNestedInitialization] and subclasses if you have a particluarly complex concept
    /// defined in your plugin component.
    /// </summary>
    public interface ICustomUI
    {
        /// <summary>
        /// Use this to fetch objects from the RDMP platform databases e.g. <see cref="Catalogue"/>, <see cref="TableInfo"/> etc
        /// </summary>
        ICatalogueRepository CatalogueRepository { get; set; }

        /// <summary>
        /// When implementing this just cast value to T and call the overload in ICustomUI&lt;T&gt;
        /// </summary>
        /// <param name="value"></param>
        /// <param name="previewIfAvailable"></param>
        void SetGenericUnderlyingObjectTo(ICustomUIDrivenClass value, System.Data.DataTable previewIfAvailable);

        /// <summary>
        /// Fetches the final state of the object being show in the user interface (e.g. after closing the form)
        /// </summary>
        /// <returns></returns>
        ICustomUIDrivenClass GetFinalStateOfUnderlyingObject();
    }
}
