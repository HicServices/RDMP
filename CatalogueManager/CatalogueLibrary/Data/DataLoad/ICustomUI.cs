using CatalogueLibrary.Repositories;

namespace CatalogueLibrary.Data.DataLoad
{
    /// <summary>
    /// Powers the creation of instances of T in an ICustomUI
    /// </summary>
    public interface ICustomUI<T> : ICustomUI where T:ICustomUIDrivenClass
    {
        void SetUnderlyingObjectTo(T value, System.Data.DataTable previewIfAvailable);
        T GetFinalStateOfUnderlyingObject();
    }

    /// <summary>
    /// Interface that lets you create UIs for populating Argument values for Properties which are too complicated to do with basic Types.  See ICustomUIDrivenClass.  If
    /// at all possible you should avoid the overhead of this system and instead use [DemandsNestedInitialization] and subclasses if you have a particluarly complex concept
    /// defined in your plugin component.
    /// </summary>
    public interface ICustomUI
    {
        ICatalogueRepository CatalogueRepository { get; set; }

        void SetGenericUnderlyingObjectTo(ICustomUIDrivenClass value, System.Data.DataTable previewIfAvailable);
        ICustomUIDrivenClass GetGenericFinalStateOfUnderlyingObject();
    }
}
