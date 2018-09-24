namespace CatalogueLibrary.Data
{
    /// <summary>
    /// A data load engine database stage, all tables being loaded go through each of these stages (RAW=>STAGING=>LIVE).  Archive is where redundant old replaced records are moved to on successful data loading
    /// </summary>
    public enum LoadBubble
    {
        /// <summary>
        /// The temporary unconstrained database created during a data load execution into which identifiable data is loaded and data 
        /// integrity issues (null records, normalisation etc) occurs
        /// </summary>
        Raw,

        /// <summary>
        /// The constrained database into which all records in a data load are written to before applying an UPSERT into the live table of
        /// new records.
        /// </summary>
        Staging,

        /// <summary>
        /// The live database containing your clinical data
        /// </summary>
        Live,

        /// <summary>
        /// The _Archive table in your live database into which historic records are moved when an UPDATE happens during data load
        /// </summary>
        Archive
    }
}