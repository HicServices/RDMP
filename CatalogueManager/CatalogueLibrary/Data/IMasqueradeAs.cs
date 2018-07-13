namespace CatalogueLibrary.Data
{
    /// <summary>
    /// If you are a wrapper masquerading as another class e.g. <see cref="CatalogueLibrary.Nodes.LoadMetadataNodes.CatalogueUsedByLoadMetadataNode"/>
    ///  is a class masquerading as an <see cref="Catalogue"/>
    /// </summary>
    public interface IMasqueradeAs
    {
        object MasqueradingAs();
    }
}