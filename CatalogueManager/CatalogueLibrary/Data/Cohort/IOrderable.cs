namespace CatalogueLibrary.Data.Cohort
{
    /// <summary>
    /// Any class which should appear in a specific order
    /// </summary>
    public interface IOrderable
    {
        int Order { get; set; }
    }
}