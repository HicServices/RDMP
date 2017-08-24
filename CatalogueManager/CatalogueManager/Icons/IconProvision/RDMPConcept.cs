namespace CatalogueManager.Icons.IconProvision
{
    public enum RDMPConcept 
    {
        Database,
        SQL,
        ReOrder,

        DQE,
        TimeCoverageField,
        Clipboard,
        
        //catalogue database objects
        AutomationServiceSlot,
        Favourite,

        LoadMetadata,
        CacheProgress,
        LoadProgress,
        LoadPeriodically,
        Plugin,

        ExternalDatabaseServer,

        Catalogue,
        CatalogueItemsNode,
        CatalogueItem,
        CatalogueItemIssue,
        ExtractionInformation,

        TableInfo,
        ColumnInfo,
        ANOColumnInfo,
        PreLoadDiscardedColumn,

        DataAccessCredentialsNode,
        DataAccessCredentials,
        
        ANOTablesNode,
        ANOTable,

        AllServersNode,
        TableInfoServerNode,

        CatalogueFolder,
        DocumentationNode,

        DashboardLayout,
        DashboardControl,
        
        FilterContainer,
        Filter,
        ExtractionFilterParameterSet,
        ParametersNode,

        AggregatesNode,
        AggregateGraph,

        CohortSetsNode,
        CohortAggregate,

        JoinableCollectionNode,
        PatientIndexTable,

        SupportingSQLTable,
        SupportingDocument,

        //data export database objects
        ExtractableDataSet,
        ExtractionConfiguration,
        Project,
        ExtractableDataSetPackage,
        ExternalCohortTable,
        ExtractableCohort,
        
        StandardRegex,
        
        CohortsNode,
        ProjectsNode,
        ExtractableDataSetsNode,
        ExtractionFolderNode,
        CustomDataTableNode,
        
        CohortIdentificationConfiguration,

        AggregateDimension,
        Lookup,
        JoinInfo,

        //to release a completed project extract
        Release,
        EmptyProject,
        NoIconAvailable,
        File,
        Help,

        //Load metadata subcomponents
        HICProjectDirectoryNode,
        AllProcessTasksUsedByLoadMetadataNode,
        AllCataloguesUsedByLoadMetadataNode,
        LoadMetadataScheduleNode,

        GetFilesStage,
        LoadBubbleMounting,
        LoadBubble,
        LoadFinalDatabase
    }
}