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
        AllAutomationServerSlotsNode,
        AutomationServiceSlot,
        AutomateablePipeline,
        AutomationServiceException,
        AllRDMPRemotesNode,
        RemoteRDMP,
        Favourite,

        LoadMetadata,
        CacheProgress,
        LoadProgress,
        LoadPeriodically,
        Plugin,

        ExternalDatabaseServer,

        Catalogue,
        ProjectCatalogue,
        CatalogueItemsNode,
        CatalogueItem,
        CatalogueItemIssue,
        ExtractionInformation,

        TableInfo,
        ColumnInfo,
        ANOColumnInfo,
        PreLoadDiscardedColumn,

        AllDataAccessCredentialsNode,
        DataAccessCredentials,
        
        AllANOTablesNode,
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

        AggregateTopX,
        AggregateContinuousDateAxis,
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
        
        AllCohortsNode,
        ProjectsNode,
        ProjectCohortIdentificationConfigurationAssociationsNode,
        ProjectSavedCohortsNode,
        ExtractableDataSetsNode,
        ExtractionDirectoryNode,
        
        CohortIdentificationConfiguration,

        AggregateDimension,
        Lookup,
        LookupCompositeJoinInfo,
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
        Logging,

        GetFilesStage,
        LoadBubbleMounting,
        LoadBubble,
        LoadFinalDatabase,

        AllExternalServersNode,
        DecryptionPrivateKeyNode,
        PreLoadDiscardedColumnsNode,
        ExtractionConfigurationsNode,

        PermissionWindow,
        Pipeline,
        PipelineComponent,
        PipelineComponentArgument,


        ObjectExport,
        ObjectImport,

        AllObjectSharingNode,
        AllObjectImportsNode,
        AllObjectExportsNode,
        AllConnectionStringKeywordsNode,

        ExtractableColumn,
        ProjectCohortsNode,
        FrozenExtractionConfigurationsNode,
        ProjectCataloguesNode,

        AllLoadMetadatasNode,
        AllPermissionWindowsNode,


        Waiting,
        WaitingForDatabase,
        Writing,
        Warning,
        Diff,
        FileMissing,

        ConnectionStringKeyword,
        AllStandardRegexesNode,

        AllPipelinesNode,
        OtherPipelinesNode,
        StandardPipelineUseCaseNode,

        AllGovernanceNode,
        GovernancePeriod,
        GovernanceDocument
    }
}