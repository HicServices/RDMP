// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.


namespace Rdmp.Core.Icons.IconProvision;

/// <summary>
///     Describes all the objects that exist in RDMP.  Serves as an index into <see cref="CatalogueIcons" />
/// </summary>
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
    CatalogueLookupsNode,

    DashboardLayout,
    DashboardControl,

    FilterContainer,
    Filter,
    ExtractionFilterParameterSet,
    ParametersNode,
    ExtendedProperty,

    AggregateTopX,
    AggregateContinuousDateAxis,
    AggregatesNode,
    AggregateGraph,

    AllOrphanAggregateConfigurationsNode,
    AllTemplateAggregateConfigurationsNode,
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
    LoadDirectoryNode,
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
    GovernanceDocument,

    AllProjectCohortIdentificationConfigurationsNode,
    AllFreeCohortIdentificationConfigurationsNode,
    CohortAggregateContainer,

    AllPluginsNode,
    ProcessTask,


    DQEGraphAnnotation,
    Evaluation,
    CumulativeExtractionResults,
    ProjectCohortIdentificationConfigurationAssociation,
    SelectedDataSetsForcedJoin,
    SupplementalExtractionResults,
    ReleaseLog,
    LoadModuleAssembly,
    TicketingSystemConfiguration,
    WindowLayout,
    SpontaneouslyInventedColumn,
    ProcessTaskArgument,
    CacheFetchFailure,
    AllDashboardsNode,
    ExtractionProgress,
    Attacher,
    DataProvider,
    MutilateDataTables,
    Commit,
    Memento,
    TableInfoDatabaseNode,
    Dataset,
    LoadMetadataCatalogueLinkage
}