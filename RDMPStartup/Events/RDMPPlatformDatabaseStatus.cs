namespace RDMPStartup.Events
{
    public enum RDMPPlatformDatabaseStatus
    {
        Unreachable,
        Broken,
        RequiresPatching,
        Healthy
    }

    public enum RDMPPlatformType
    {
        Catalogue,  //tier 1
        DataExport,
        Logging,    //tier 2
        DQE,
        ANO,
        IdentifierDump,
        QueryCache,
        
        Plugin  //tier 3
    }
}