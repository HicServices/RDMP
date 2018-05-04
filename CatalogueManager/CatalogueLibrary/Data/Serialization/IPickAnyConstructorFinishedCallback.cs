namespace CatalogueLibrary.Data.Serialization
{
    /// <summary>
    /// For use with classes who intend to be constructed by <see cref="PickAnyConstructorJsonConverter"/>.  If you implement this interface then the callback notify you after your 
    /// constructor has been called and properties populated. 
    /// </summary>
    public interface IPickAnyConstructorFinishedCallback
    {
        /// <summary>
        /// Called after a <see cref="PickAnyConstructorJsonConverter"/> finishes constructing and populating your instance
        /// </summary>
        void AfterConstruction();
    }
}