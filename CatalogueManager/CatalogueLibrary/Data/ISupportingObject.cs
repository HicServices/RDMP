namespace CatalogueLibrary.Data
{
    /// <summary>
    /// An object that helps understanding a <see cref="Catalogue"/>
    /// </summary>
    public interface ISupportingObject
    {
        int Catalogue_ID { get; set; }
        bool Extractable { get; set; }
        bool IsGlobal { get; set; }
    }
}