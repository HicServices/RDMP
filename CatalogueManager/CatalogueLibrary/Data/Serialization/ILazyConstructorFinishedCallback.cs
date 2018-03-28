namespace CatalogueLibrary.Data.Serialization
{
    /// <summary>
    /// For use with classes who intend to be constructed by <see cref="LazyConstructorsJsonConverter"/>.  If you implement this interface then the callback notify you after your 
    /// constructor has been called and properties populated. 
    /// </summary>
    public interface ILazyConstructorFinishedCallback
    {
        /// <summary>
        /// Called after a LazyConstructorsJsonConverter finishes constructing and populating your instance
        /// </summary>
        void LazyConstructorFinished();
    }
}