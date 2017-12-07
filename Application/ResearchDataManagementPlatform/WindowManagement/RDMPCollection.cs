namespace ResearchDataManagementPlatform.WindowManagement
{
    /// <summary>
    /// Enum for specifying which RDMPCollectionUI a given operation relates to.  This allows you to request a collection be shown etc without having to pass typeof(X) 
    /// </summary>
    public enum RDMPCollection
    {
        None=0,
        Tables,
        Catalogue,
        DataExport,
        SavedCohorts,
        Favourites,

        Cohort,
        DataLoad,
        
    }
}