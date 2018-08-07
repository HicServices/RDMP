namespace CatalogueLibrary.Data.Cohort
{
    /// <summary>
    /// Any class which should appear in a specific order
    /// </summary>
    public interface IOrderable
    {
        /// <summary>
        /// Order object should appear in relative to other <see cref="IOrderable"/> objects 
        /// in the same scope
        /// </summary>
        int Order { get; set; }
    }
}