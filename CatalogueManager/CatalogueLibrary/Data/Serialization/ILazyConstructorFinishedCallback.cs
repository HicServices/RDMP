namespace CatalogueLibrary.Data.Serialization
{
    public interface ILazyConstructorFinishedCallback
    {
        /// <summary>
        /// Called after a LazyConstructorsJsonConverter finishes constructing and populating your instance
        /// </summary>
        void LazyConstructorFinished();
    }
}