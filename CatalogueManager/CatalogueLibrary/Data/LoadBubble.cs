namespace CatalogueLibrary.Data
{
    /// <summary>
    /// A data load engine database stage, all tables being loaded go through each of these stages (RAW=>STAGING=>LIVE).  Archive is where redundant old replaced records are moved to on successful data loading
    /// </summary>
    public enum LoadBubble
    {
        Raw,
        Staging,
        Live,
        Archive //The archive TABLE! not a physical disk archive
    }
}