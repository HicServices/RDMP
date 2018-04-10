namespace CatalogueLibrary.Data
{
    /// <summary>
    /// Determines how accessible a given ExtractionInformation should be.
    /// </summary>
    public enum ExtractionCategory
    {
        /// <summary>
        /// This column is always available for extraction
        /// </summary>
        Core,

        /// <summary>
        /// This column is available but might not always be wanted e.g. lookup descriptions where there is already a lookup code
        /// </summary>
        Supplemental,

        /// <summary>
        /// This column is only available to researchers who have additional approvals over and above those required for a basic data extract
        /// </summary>
        SpecialApprovalRequired,

        /// <summary>
        /// This column is for internal use only and shouldn't be released to researchers during data extraction
        /// </summary>
        Internal,

        /// <summary>
        /// This column used to be supplied to researchers but should no longer be provided
        /// </summary>
        Deprecated,

        /// <summary>
        /// Value can only be used for fetching ExtractionInformations.  This means that all will be returned.  You cannot set a column to have an ExtractionCategory of Any
        /// </summary>
        Any

    }
}