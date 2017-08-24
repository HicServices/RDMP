using CatalogueLibrary.Repositories;

namespace CatalogueLibrary.Data.DataLoad
{
    /// <summary>
    /// Powers the creation of instances of T
    /// </summary>
    public interface ICustomUI<T> : ICustomUI where T:ICustomUIDrivenClass
    {
        void SetUnderlyingObjectTo(T value, System.Data.DataTable previewIfAvailable);
        T GetFinalStateOfUnderlyingObject();
    }

    public interface ICustomUI
    {
        ICatalogueRepository CatalogueRepository { get; set; }

        void SetGenericUnderlyingObjectTo(ICustomUIDrivenClass value, System.Data.DataTable previewIfAvailable);
        ICustomUIDrivenClass GetGenericFinalStateOfUnderlyingObject();
    }
}
