// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ClientModel.Primitives;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NPOI.OpenXmlFormats.Dml;
using Org.BouncyCastle.Asn1.X509.Qualified;
using Rdmp.Core.Caching.Pipeline;
using Rdmp.Core.CohortCommitting.Pipeline;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Cohort.Joinables;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Governance;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.Curation.Data.Remoting;
using Rdmp.Core.Curation.DataHelper.RegexRedaction;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.Pipeline;
using Rdmp.Core.DataExport.DataRelease.Pipeline;
using Rdmp.Core.DataLoad.Engine.Pipeline;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.Providers.Nodes.CohortNodes;
using Rdmp.Core.Providers.Nodes.LoadMetadataNodes;
using Rdmp.Core.Providers.Nodes.PipelineNodes;
using Rdmp.Core.Providers.Nodes.SharingNodes;
using Rdmp.Core.Repositories;
using Rdmp.Core.Repositories.Managers;
using Rdmp.Core.Repositories.Managers.HighPerformance;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Comments;
using Rdmp.Core.ReusableLibraryCode.Settings;

namespace Rdmp.Core.Providers;

/// <summary>
/// Performance optimisation class and general super class in charge of recording and discovering all objects in the Catalogue database so they can be displayed in
/// RDMPCollectionUIs etc.  This includes issuing a single database query per Type fetching all objects (e.g. AllProcessTasks, AllLoadMetadatas etc) and then in evaluating
/// and documenting the hierarchy in _childDictionary.  Every object that is not a root level object also has a DescendancyList which records the path of parents to that
/// exact object.  Therefore you can easily identify 1. what the immediate children of any object are, 2. what the full path to any given object is.
/// 
/// <para>The pattern is:
/// 1. Identify a root level object
/// 2. Create a method overload AddChildren that takes the object
/// 3. Create a new HashSet containing all the child objects (regardless of mixed Type)
/// 4. Call AddToDictionaries with a new DescendancyList containing the parent object
/// 5. For each of the objects added that has children of its own repeat the above (Except call DescendancyList.Add instead of creating a new one)</para>
///  
/// </summary>
public class CatalogueChildProvider : ICoreChildProvider
{
    //Load System
    LazyWithReset<LoadMetadata[]> _lazyAllLoadMetadatas = new(() => []);
    public LoadMetadata[] AllLoadMetadatas { get => _lazyAllLoadMetadatas.Value; }

    LazyWithReset<LoadMetadataCatalogueLinkage[]> _lazyAllLoadMetadataCatalogueLinkages = new(() => []);
    public LoadMetadataCatalogueLinkage[] AllLoadMetadataCatalogueLinkages { get => _lazyAllLoadMetadataCatalogueLinkages.Value; }

    LazyWithReset<LoadMetadataCatalogueLinkage[]> _lazyAllLoadMetadataLinkage = new(() => []);
    private LoadMetadataCatalogueLinkage[] AllLoadMetadataLinkage { get => _lazyAllLoadMetadataLinkage.Value; }

    LazyWithReset<ProcessTask[]> _lazyAllProcessTasks = new(() => []);
    public ProcessTask[] AllProcessTasks { get => _lazyAllProcessTasks.Value; }

    LazyWithReset<ProcessTaskArgument[]> _lazyAllProcessTasksArguments = new(() => []);
    public ProcessTaskArgument[] AllProcessTasksArguments { get => _lazyAllProcessTasksArguments.Value; }

    LazyWithReset<LoadProgress[]> _lazyAllLoadProgress = new(() => []);
    public LoadProgress[] AllLoadProgresses { get => _lazyAllLoadProgress.Value; }

    LazyWithReset<CacheProgress[]> _lazyAllCacheProgresses = new(() => []);
    public CacheProgress[] AllCacheProgresses { get => _lazyAllCacheProgresses.Value; }

    LazyWithReset<PermissionWindow[]> _lazyAllPermissionWindows = new(() => []);
    public PermissionWindow[] AllPermissionWindows { get => _lazyAllPermissionWindows.Value; }

    //Catalogue side of things
    LazyWithReset<Catalogue[]> _lazyAllCatalogues = new(() => []);
    public Catalogue[] AllCatalogues { get => _lazyAllCatalogues.Value; }

    LazyWithReset<Curation.Data.Dataset[]> _lazyAllDatasets = new(() => []);
    public Curation.Data.Dataset[] AllDatasets { get => _lazyAllDatasets.Value; }

    LazyWithReset<Dictionary<int, Catalogue>> _lazyAllCataloguesDictionary = new(() => []);
    public Dictionary<int, Catalogue> AllCataloguesDictionary { get => _lazyAllCataloguesDictionary.Value; }

    LazyWithReset<SupportingDocument[]> _lazyAllSupportingDocuments = new(() => []);
    public SupportingDocument[] AllSupportingDocuments { get => _lazyAllSupportingDocuments.Value; }

    LazyWithReset<SupportingSQLTable[]> _lazyAllSupportingSQL = new(() => []);
    public SupportingSQLTable[] AllSupportingSQL { get => _lazyAllSupportingSQL.Value; }

    //tells you the immediate children of a given node.  Do not add to this directly instead add using AddToDictionaries unless you want the Key to be an 'on the sly' no known descendency child
    //private ConcurrentDictionary<object, HashSet<object>> _childDictionary = new();

    ////This is the reverse of _childDictionary in some ways.  _childDictionary tells you the immediate children while
    ////this tells you for a given child object what the navigation tree down to get to it is e.g. ascendancy[child] would return [root,grandParent,parent]
    //private ConcurrentDictionary<object, DescendancyList> _descendancyDictionary = new();


    public IEnumerable<CatalogueItem> AllCatalogueItems { get => AllCatalogueItemsDictionary.Values; }

    LazyWithReset<Dictionary<int, List<CatalogueItem>>> _lazy_catalogueToCatalogueItems = new(() => new Dictionary<int, List<CatalogueItem>>());

    private Dictionary<int, List<CatalogueItem>> _catalogueToCatalogueItems { get => _lazy_catalogueToCatalogueItems.Value; }

    LazyWithReset<Dictionary<int, CatalogueItem>> _lazyAllCatalogueItemsDictionary = new(() => new Dictionary<int, CatalogueItem>());
    public Dictionary<int, CatalogueItem> AllCatalogueItemsDictionary { get => _lazyAllCatalogueItemsDictionary.Value; }

    LazyWithReset<Dictionary<int, ColumnInfo>> _lazy_allColumnInfos = new(() => new Dictionary<int, ColumnInfo>());
    private Dictionary<int, ColumnInfo> _allColumnInfos { get => _lazy_allColumnInfos.Value; }

    LazyWithReset<AggregateConfiguration[]> _lazyAllAggregateConfigurations = new(() => []);
    public AggregateConfiguration[] AllAggregateConfigurations { get => _lazyAllAggregateConfigurations.Value; }

    LazyWithReset<AggregateDimension[]> _lazyAllAggregateDimensions;
    public AggregateDimension[] AllAggregateDimensions { get => _lazyAllAggregateDimensions.Value; }

    LazyWithReset<AggregateContinuousDateAxis[]> _lazyAllAggregateContinuousDataAxis;
    public AggregateContinuousDateAxis[] AllAggregateContinuousDateAxis { get => _lazyAllAggregateContinuousDataAxis.Value; }

    LazyWithReset<AllRDMPRemotesNode> _lazyAllRDMPRemotesNode;
    public AllRDMPRemotesNode AllRDMPRemotesNode { get { return _lazyAllRDMPRemotesNode.Value; } }

    LazyWithReset<RemoteRDMP[]> _lazyAllRemoteRDMPs = new(() => []);
    public RemoteRDMP[] AllRemoteRDMPs { get => _lazyAllRemoteRDMPs.Value; }

    LazyWithReset<AllDashboardsNode> _lazyAllDashboardsNode;
    public AllDashboardsNode AllDashboardsNode { get => _lazyAllDashboardsNode.Value; }

    LazyWithReset<DashboardLayout[]> _lazyAllDashboards;
    public DashboardLayout[] AllDashboards { get => _lazyAllDashboards.Value; }

    LazyWithReset<AllObjectSharingNode> _lazyAllObjectSharingNode;
    public AllObjectSharingNode AllObjectSharingNode { get => _lazyAllObjectSharingNode.Value; }

    LazyWithReset<ObjectImport[]> _lazyAllImports;
    public ObjectImport[] AllImports { get => _lazyAllImports.Value; }

    LazyWithReset<ObjectExport[]> _lazyAllExports;
    public ObjectExport[] AllExports { get => _lazyAllExports.Value; }


    LazyWithReset<AllStandardRegexesNode> _lazyAllStandardRegexesNode;
    public AllStandardRegexesNode AllStandardRegexesNode { get => _lazyAllStandardRegexesNode.Value; }

    LazyWithReset<AllPipelinesNode> _lazyAllPipelinesNode;
    public AllPipelinesNode AllPipelinesNode { get => _lazyAllPipelinesNode.Value; }

    LazyWithReset<OtherPipelinesNode> _lazyOtherPipelineNode;
    public OtherPipelinesNode OtherPipelinesNode { get => _lazyOtherPipelineNode.Value; }

    LazyWithReset<Pipeline[]> _lazyAllPipelines;
    public Pipeline[] AllPipelines { get => _lazyAllPipelines.Value; }

    LazyWithReset<PipelineComponent[]> _lazyAllPipelineComponents;
    public PipelineComponent[] AllPipelineComponents { get => _lazyAllPipelineComponents.Value; }

    LazyWithReset<PipelineComponentArgument[]> _lazyAllPipelineComponentArgument;
    public PipelineComponentArgument[] AllPipelineComponentsArguments { get => _lazyAllPipelineComponentArgument.Value; }

    LazyWithReset<StandardRegex[]> _lazyAllStandardRegex;
    public StandardRegex[] AllStandardRegexes { get => _lazyAllStandardRegex.Value; }

    //TableInfo side of things
    LazyWithReset<AllANOTablesNode> _lazyAllANOTableNodes = new(() => new AllANOTablesNode());
    public AllANOTablesNode AllANOTablesNode { get => _lazyAllANOTableNodes.Value; }

    LazyWithReset<ANOTable[]> _lazyAllANOTables = new(() => []);
    public ANOTable[] AllANOTables { get => _lazyAllANOTables.Value; }

    LazyWithReset<ExternalDatabaseServer[]> _lazyAllExternalServers = new(() => []);
    public ExternalDatabaseServer[] AllExternalServers { get => _lazyAllExternalServers.Value; }

    LazyWithReset<TableInfoServerNode[]> _lazyAllServers;
    public TableInfoServerNode[] AllServers { get => _lazyAllServers.Value; }

    LazyWithReset<TableInfo[]> _lazyAllTableInfos = new(() => []);
    public TableInfo[] AllTableInfos { get => _lazyAllTableInfos.Value; }


    LazyWithReset<AllDataAccessCredentialsNode> _lazyAllDataAccessCredentialsNode = new(() => new AllDataAccessCredentialsNode());
    public AllDataAccessCredentialsNode AllDataAccessCredentialsNode { get => _lazyAllDataAccessCredentialsNode.Value; }

    LazyWithReset<AllExternalServersNode> _lazyAllExternalServersNode;
    public AllExternalServersNode AllExternalServersNode { get => _lazyAllExternalServersNode.Value; }

    LazyWithReset<AllServersNode> _lazyAllServersNode;
    public AllServersNode AllServersNode { get => _lazyAllServersNode.Value; }

    LazyWithReset<DataAccessCredentials[]> _lazyAllDataAccessCredentials = new(() => []);
    public DataAccessCredentials[] AllDataAccessCredentials { get => _lazyAllDataAccessCredentials.Value; }

    LazyWithReset<Dictionary<ITableInfo, List<DataAccessCredentialUsageNode>>> _lazyAllDataAccessCredentialsUsage = new(() => []);
    public Dictionary<ITableInfo, List<DataAccessCredentialUsageNode>> AllDataAccessCredentialUsages { get => _lazyAllDataAccessCredentialsUsage.Value; }

    LazyWithReset<Dictionary<int, List<ColumnInfo>>> _lazyTableInfosToColumnInfos = new(() => new Dictionary<int, List<ColumnInfo>>());
    public Dictionary<int, List<ColumnInfo>> TableInfosToColumnInfos { get => _lazyTableInfosToColumnInfos.Value; }

    LazyWithReset<ColumnInfo[]> _lazyAllColumnInfos = new(() => []);
    public ColumnInfo[] AllColumnInfos { get => _lazyAllColumnInfos.Value; }

    LazyWithReset<PreLoadDiscardedColumn[]> _lazyAllPreLoadDiscardedColumns = new(() => []);
    public PreLoadDiscardedColumn[] AllPreLoadDiscardedColumns { get => _lazyAllPreLoadDiscardedColumns.Value; }

    LazyWithReset<Lookup[]> _lazyAllLookups;
    public Lookup[] AllLookups { get => _lazyAllLookups.Value; }

    LazyWithReset<JoinInfo[]> _lazyAllJoinInfos;
    public JoinInfo[] AllJoinInfos { get => _lazyAllJoinInfos.Value; }

    LazyWithReset<AnyTableSqlParameter[]> _lazyAllAnyTableParameters = new(() => []);
    public AnyTableSqlParameter[] AllAnyTableParameters { get => _lazyAllAnyTableParameters.Value; }

    //Filter / extraction side of things
    public IEnumerable<ExtractionInformation> AllExtractionInformations => AllExtractionInformationsDictionary.Values;

    LazyWithReset<AllPermissionWindowsNode> _lazyAllPermissionWindowsNode = new(() => new AllPermissionWindowsNode());
    public AllPermissionWindowsNode AllPermissionWindowsNode { get => _lazyAllPermissionWindowsNode.Value; }

    LazyWithReset<FolderNode<LoadMetadata>> _lazyLoadMetadataRootFolder;
    public FolderNode<LoadMetadata> LoadMetadataRootFolder { get => _lazyLoadMetadataRootFolder.Value; }

    LazyWithReset<FolderNode<Curation.Data.Dataset>> _lazyDatasetRootFolder;
    public FolderNode<Curation.Data.Dataset> DatasetRootFolder { get => _lazyDatasetRootFolder.Value; }

    LazyWithReset<FolderNode<CohortIdentificationConfiguration>> _lazyCohortidentificationConfigurationRootFolder;
    public FolderNode<CohortIdentificationConfiguration> CohortIdentificationConfigurationRootFolder { get => _lazyCohortidentificationConfigurationRootFolder.Value; }

    LazyWithReset<FolderNode<CohortIdentificationConfiguration>> _lazyCohortIdentificationConfigurationRootFolderWithoutVersionedConfigurations;
    public FolderNode<CohortIdentificationConfiguration> CohortIdentificationConfigurationRootFolderWithoutVersionedConfigurations { get => _lazyCohortIdentificationConfigurationRootFolderWithoutVersionedConfigurations.Value; }

    LazyWithReset<AllConnectionStringKeywordsNode> _lazyAllConnectionStringKeyworksNode = new(() => new AllConnectionStringKeywordsNode());
    public AllConnectionStringKeywordsNode AllConnectionStringKeywordsNode { get => _lazyAllConnectionStringKeyworksNode.Value; }

    LazyWithReset<ConnectionStringKeyword[]> _lazyAllConnectionStringKeywords = new(() => []);
    public ConnectionStringKeyword[] AllConnectionStringKeywords { get => _lazyAllConnectionStringKeywords.Value; }

    LazyWithReset<Dictionary<int, ExtractionInformation>> _lazyAllExtractionInformationsDictionary;
    public Dictionary<int, ExtractionInformation> AllExtractionInformationsDictionary { get => _lazyAllExtractionInformationsDictionary.Value; }

    LazyWithReset<Dictionary<int, ExtractionInformation>> _lazy_extractionInformationsByCatalogueItem = new(() => new Dictionary<int, ExtractionInformation>());
    protected Dictionary<int, ExtractionInformation> _extractionInformationsByCatalogueItem { get => _lazy_extractionInformationsByCatalogueItem.Value; }

    private IFilterManager _aggregateFilterManager;

    //Filters for Aggregates (includes filter containers (AND/OR)
    LazyWithReset<Dictionary<int, AggregateFilterContainer>> _lazyAllAggregateContainersDictionary;
    public Dictionary<int, AggregateFilterContainer> AllAggregateContainersDictionary { get => _lazyAllAggregateContainersDictionary.Value; }

    public AggregateFilterContainer[] AllAggregateContainers => AllAggregateContainersDictionary.Values.ToArray();

    LazyWithReset<AggregateFilter[]> _lazyAllAggregateFilters;
    public AggregateFilter[] AllAggregateFilters { get => _lazyAllAggregateFilters.Value; }
    LazyWithReset<AggregateFilterParameter[]> _lazyAllAggregateFilterParameters;
    public AggregateFilterParameter[] AllAggregateFilterParameters { get => _lazyAllAggregateFilterParameters.Value; }

    //Catalogue master filters (does not include any support for filter containers (AND/OR)
    LazyWithReset<ExtractionFilter[]> _lazyAllCatalogueFilters;
    private ExtractionFilter[] AllCatalogueFilters { get => _lazyAllCatalogueFilters.Value; }
    LazyWithReset<ExtractionFilterParameter[]> _lazyAllCatalogueParameters;
    public ExtractionFilterParameter[] AllCatalogueParameters { get => _lazyAllCatalogueParameters.Value; }

    LazyWithReset<ExtractionFilterParameterSet[]> _lazyAllCatalogueValueSets;
    public ExtractionFilterParameterSet[] AllCatalogueValueSets { get => _lazyAllCatalogueValueSets.Value; }
    LazyWithReset<ExtractionFilterParameterSetValue[]> _lazyAllCatalogueValueSetValues;
    public ExtractionFilterParameterSetValue[] AllCatalogueValueSetValues { get => _lazyAllCatalogueValueSetValues.Value; }

    private ICohortContainerManager _cohortContainerManager;

    LazyWithReset<CohortIdentificationConfiguration[]> _lazyAllCohortIdentificationConfigurations = new(() => []);
    public CohortIdentificationConfiguration[] AllCohortIdentificationConfigurations { get => _lazyAllCohortIdentificationConfigurations.Value; }

    LazyWithReset<CohortAggregateContainer[]> _lazyAllCohortAggregateContainers = new(() => []);
    public CohortAggregateContainer[] AllCohortAggregateContainers { get => _lazyAllCohortAggregateContainers.Value; }

    LazyWithReset<JoinableCohortAggregateConfiguration[]> _lazyAllJoinables;
    public JoinableCohortAggregateConfiguration[] AllJoinables { get => _lazyAllJoinables.Value; }

    LazyWithReset<JoinableCohortAggregateConfigurationUse[]> _lazyAllJoinUses;
    public JoinableCohortAggregateConfigurationUse[] AllJoinUses { get => _lazyAllJoinUses.Value; }

    /// <summary>
    /// Collection of all objects for which there are masqueraders
    /// </summary>
    LazyWithReset<ConcurrentDictionary<object, HashSet<IMasqueradeAs>>> _lazyAllMasquerades = new(() => new ConcurrentDictionary<object, HashSet<IMasqueradeAs>>());
    public ConcurrentDictionary<object, HashSet<IMasqueradeAs>> AllMasqueraders { get => _lazyAllMasquerades.Value; }

    private IChildProvider[] _pluginChildProviders;
    private readonly ICatalogueRepository _catalogueRepository;
    private readonly ICheckNotifier _errorsCheckNotifier;
    private readonly List<IChildProvider> _blockedPlugins = new();


    LazyWithReset<AllGovernanceNode> _lazyAllGovernanceNode;
    public AllGovernanceNode AllGovernanceNode { get => _lazyAllGovernanceNode.Value; }

    LazyWithReset<GovernancePeriod[]> _lazyAllGovernancePeriods;
    public GovernancePeriod[] AllGovernancePeriods { get => _lazyAllGovernancePeriods.Value; }

    LazyWithReset<GovernanceDocument[]> _lazyAllGovernanceDocuments;
    public GovernanceDocument[] AllGovernanceDocuments { get => _lazyAllGovernanceDocuments.Value; }

    LazyWithReset<Dictionary<int, HashSet<int>>> _lazyAllGovernanceCoverage;
    public Dictionary<int, HashSet<int>> GovernanceCoverage { get => _lazyAllGovernanceCoverage.Value; }

    private CommentStore _commentStore;

    LazyWithReset<JoinableCohortAggregateConfigurationUse[]> _lazyAllJoinableCohortAggregateConfigurationUse;
    public JoinableCohortAggregateConfigurationUse[] AllJoinableCohortAggregateConfigurationUse { get => _lazyAllJoinableCohortAggregateConfigurationUse.Value; }

    LazyWithReset<AllPluginsNode> _lazyAllPluginsNode;
    public AllPluginsNode AllPluginsNode { get => _lazyAllPluginsNode.Value; }

    LazyWithReset<HashSet<StandardPipelineUseCaseNode>> _lazyPipelineUseCases = new(() => new HashSet<StandardPipelineUseCaseNode>());

    public HashSet<StandardPipelineUseCaseNode> PipelineUseCases { get => _lazyPipelineUseCases.Value; }

    /// <summary>
    /// Lock for changes to Child provider
    /// </summary>
    protected object WriteLock = new();

    LazyWithReset<AllOrphanAggregateConfigurationsNode> _lazyOrphanAggregateConfigurationsNode = new(() => new AllOrphanAggregateConfigurationsNode());
    public AllOrphanAggregateConfigurationsNode OrphanAggregateConfigurationsNode { get => _lazyOrphanAggregateConfigurationsNode.Value; }

    LazyWithReset<AllTemplateAggregateConfigurationsNode> _lazyTemplateAggregateConfigurationsNode = new(() => new AllTemplateAggregateConfigurationsNode());

    public AllTemplateAggregateConfigurationsNode TemplateAggregateConfigurationsNode { get => _lazyTemplateAggregateConfigurationsNode.Value; }

    LazyWithReset<FolderNode<Catalogue>> _lazyCatalogueRootFolder;
    public FolderNode<Catalogue> CatalogueRootFolder { get => _lazyCatalogueRootFolder.Value; set => _lazyCatalogueRootFolder.Reset(); }

    LazyWithReset<AllDatasetsNode> _lazyAllDatasetsNode = new(() => new AllDatasetsNode());
    public AllDatasetsNode AllDatasetsNode { get => _lazyAllDatasetsNode.Value; }

    LazyWithReset<RegexRedactionConfiguration[]> _lazyAllRegexRedactionConfigurations = new(() => []);
    public RegexRedactionConfiguration[] AllRegexRedactionConfigurations { get => _lazyAllRegexRedactionConfigurations.Value; }

    LazyWithReset<AllRegexRedactionConfigurationsNode> _lazyAllRegexRedactionConfigurationsNode = new(() => new AllRegexRedactionConfigurationsNode());
    public AllRegexRedactionConfigurationsNode AllRegexRedactionConfigurationsNode { get => _lazyAllRegexRedactionConfigurationsNode.Value; }

    LazyWithReset<HashSet<AggregateConfiguration>> _lazyOrphanAggregateConfigurations;
    public HashSet<AggregateConfiguration> OrphanAggregateConfigurations { get => _lazyOrphanAggregateConfigurations.Value; }
    LazyWithReset<AggregateConfiguration[]> _lazyTemplateAggregateConfigurations = new(() => []);
    public AggregateConfiguration[] TemplateAggregateConfigurations { get => _lazyTemplateAggregateConfigurations.Value; }

    LazyWithReset<AllTemplateCohortIdentificationConfigurationsNode> _lazyAllTemplateCohortIdentificationConfigurationsNode = new(() => new AllTemplateCohortIdentificationConfigurationsNode());
    public AllTemplateCohortIdentificationConfigurationsNode AllTemplateCohortIdentificationConfigurationsNode { get => _lazyAllTemplateCohortIdentificationConfigurationsNode.Value; }

    LazyWithReset<CohortIdentificationConfiguration[]> _lazyAllTemplateCohortIdentificationConfigurations = new(() => []);
    public CohortIdentificationConfiguration[] AllTemplateCohortIdentificationConfigurations { get => _lazyAllTemplateCohortIdentificationConfigurations.Value; }

    protected Stopwatch ProgressStopwatch = Stopwatch.StartNew();
    private int _progress;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="pluginChildProviders"></param>
    /// <param name="errorsCheckNotifier">Where to report errors building the hierarchy e.g. when <paramref name="pluginChildProviders"/> crash.  Set to null for <see cref="IgnoreAllErrorsCheckNotifier"/></param>
    /// <param name="previousStateIfKnown">Previous child provider state if you know it otherwise null</param>
    public CatalogueChildProvider(ICatalogueRepository repository, IChildProvider[] pluginChildProviders,
        ICheckNotifier errorsCheckNotifier, CatalogueChildProvider previousStateIfKnown)
    {
        var x = Environment.StackTrace;
        _commentStore = repository.CommentStore;
        _catalogueRepository = repository;
        _catalogueRepository?.EncryptionManager?.ClearAllInjections();

        _errorsCheckNotifier = errorsCheckNotifier ?? IgnoreAllErrorsCheckNotifier.Instance;



        _lazyAllCatalogues = new LazyWithReset<Catalogue[]>(() =>
        {
            var _catalogues = GetAllObjects<Catalogue>(_catalogueRepository);
            return _catalogues;

        });

        _lazyCatalogueRootFolder = new LazyWithReset<FolderNode<Catalogue>>(() =>
        {
            var tree = FolderHelper.BuildFolderTree(AllCatalogues);
            return tree;

        });

        if (UserSettings.DebugPerformance)
            _errorsCheckNotifier.OnCheckPerformed(new CheckEventArgs(
                $"Refresh generated by:{Environment.NewLine}{Environment.StackTrace}", CheckResult.Success));

        _pluginChildProviders = pluginChildProviders ?? Array.Empty<IChildProvider>();

        ReportProgress("Before object fetches");

        _lazyAllAnyTableParameters = new LazyWithReset<AnyTableSqlParameter[]>(() => GetAllObjects<AnyTableSqlParameter>(repository));

        _lazyAllANOTables = new LazyWithReset<ANOTable[]>(() => GetAllObjects<ANOTable>(repository));

        _lazyAllANOTableNodes = new LazyWithReset<AllANOTablesNode>(() =>
        {
            var x = new AllANOTablesNode();
            return x;
        });

        _lazyAllCataloguesDictionary = new LazyWithReset<Dictionary<int, Catalogue>>(() => AllCatalogues.ToDictionaryEx(i => i.ID, o => o));

        _lazyAllDatasets = new LazyWithReset<Curation.Data.Dataset[]>(() => GetAllObjects<Curation.Data.Dataset>(repository));

        _lazyAllLoadMetadatas = new LazyWithReset<LoadMetadata[]>(() =>
        {
            return GetAllObjects<LoadMetadata>(repository);
        });

        _lazyAllLoadMetadataCatalogueLinkages = new LazyWithReset<LoadMetadataCatalogueLinkage[]>(() => GetAllObjects<LoadMetadataCatalogueLinkage>(repository));

        _lazyAllLoadMetadataLinkage = new LazyWithReset<LoadMetadataCatalogueLinkage[]>(() => GetAllObjects<LoadMetadataCatalogueLinkage>(repository));

        _lazyAllProcessTasks = new LazyWithReset<ProcessTask[]>(() => GetAllObjects<ProcessTask>(repository));

        _lazyAllProcessTasksArguments = new LazyWithReset<ProcessTaskArgument[]>(() => GetAllObjects<ProcessTaskArgument>(repository));

        _lazyAllLoadProgress = new LazyWithReset<LoadProgress[]>(() => GetAllObjects<LoadProgress>(repository));

        _lazyAllCacheProgresses = new LazyWithReset<CacheProgress[]>(() => GetAllObjects<CacheProgress>(repository));

        _lazyAllPermissionWindows = new LazyWithReset<PermissionWindow[]>(() => GetAllObjects<PermissionWindow>(repository));
        _lazyAllPermissionWindowsNode = new LazyWithReset<AllPermissionWindowsNode>(() => { var x = new AllPermissionWindowsNode(); return x; });

        _lazyAllRemoteRDMPs = new LazyWithReset<RemoteRDMP[]>(() =>
        {
            return GetAllObjects<RemoteRDMP>(_catalogueRepository);
        });

        _lazyAllExternalServers = new LazyWithReset<ExternalDatabaseServer[]>(() => GetAllObjects<ExternalDatabaseServer>(repository));

        _lazyAllTableInfos = new LazyWithReset<TableInfo[]>(() => GetAllObjects<TableInfo>(repository));
        _lazyAllDataAccessCredentials = new LazyWithReset<DataAccessCredentials[]>(() => GetAllObjects<DataAccessCredentials>(repository));
        _lazyAllDataAccessCredentialsNode = new LazyWithReset<AllDataAccessCredentialsNode>(() =>
        {
            var x = new AllDataAccessCredentialsNode();
            return x;
        });

        _lazyAllConnectionStringKeyworksNode = new LazyWithReset<AllConnectionStringKeywordsNode>(() =>
        {
            var x = new AllConnectionStringKeywordsNode();
            return x;
        });
        _lazyAllConnectionStringKeywords = new LazyWithReset<ConnectionStringKeyword[]>(() => GetAllObjects<ConnectionStringKeyword>(repository));


        ReportProgress("after basic object fetches");

        _lazyAllDataAccessCredentialsUsage = new LazyWithReset<Dictionary<ITableInfo, List<DataAccessCredentialUsageNode>>>(() => repository.TableInfoCredentialsManager.GetAllCredentialUsagesBy(AllDataAccessCredentials,
                AllTableInfos));

        _lazyAllColumnInfos = new LazyWithReset<ColumnInfo[]>(() => GetAllObjects<ColumnInfo>(repository));

        ReportProgress("After credentials");

        _lazyTableInfosToColumnInfos = new LazyWithReset<Dictionary<int, List<ColumnInfo>>>(() => AllColumnInfos.GroupBy(c => c.TableInfo_ID)
                .ToDictionaryEx(gdc => gdc.Key, gdc => gdc.ToList()));

        ReportProgress("After TableInfo to ColumnInfo mapping");

        _lazyAllPreLoadDiscardedColumns = new LazyWithReset<PreLoadDiscardedColumn[]>(() => GetAllObjects<PreLoadDiscardedColumn>(repository));

        _lazyAllSupportingDocuments = new LazyWithReset<SupportingDocument[]>(() => GetAllObjects<SupportingDocument>(repository));

        _lazyAllSupportingSQL = new LazyWithReset<SupportingSQLTable[]>(() => GetAllObjects<SupportingSQLTable>(repository));

        _lazyAllCohortIdentificationConfigurations = new LazyWithReset<CohortIdentificationConfiguration[]>(() => GetAllObjects<CohortIdentificationConfiguration>(repository));

        FetchCatalogueItems();

        ReportProgress("After CatalogueItem injection");

        FetchExtractionInformations();

        ReportProgress("After ExtractionInformation injection");

        BuildAggregateConfigurations();

        BuildCohortCohortAggregateContainers();
        _lazyAllJoinables = new LazyWithReset<JoinableCohortAggregateConfiguration[]>(() => GetAllObjects<JoinableCohortAggregateConfiguration>(repository));
        _lazyAllJoinUses = new LazyWithReset<JoinableCohortAggregateConfigurationUse[]>(() => GetAllObjects<JoinableCohortAggregateConfigurationUse>(repository));
        _lazyAllCatalogueFilters = new LazyWithReset<ExtractionFilter[]>(() => GetAllObjects<ExtractionFilter>(repository));
        _lazyAllCatalogueParameters = new LazyWithReset<ExtractionFilterParameter[]>(() => GetAllObjects<ExtractionFilterParameter>(repository));
        _lazyAllCatalogueValueSets = new LazyWithReset<ExtractionFilterParameterSet[]>(() => GetAllObjects<ExtractionFilterParameterSet>(repository));
        _lazyAllCatalogueValueSetValues = new LazyWithReset<ExtractionFilterParameterSetValue[]>(() => GetAllObjects<ExtractionFilterParameterSetValue>(repository));

        ReportProgress("After Filter and Joinable fetching");

        _lazyAllLookups = new LazyWithReset<Lookup[]>(() =>
            {
                var x = GetAllObjects<Lookup>(repository);

                foreach (var l in x)
                    l.SetKnownColumns(_allColumnInfos[l.PrimaryKey_ID], _allColumnInfos[l.ForeignKey_ID],
                        _allColumnInfos[l.Description_ID]);
                return x;
            });

        _lazyAllJoinInfos = new LazyWithReset<JoinInfo[]>(() =>
        {
            var x = GetAllObjects<JoinInfo>(repository);
            foreach (var j in x)
                j.SetKnownColumns(_allColumnInfos[j.PrimaryKey_ID], _allColumnInfos[j.ForeignKey_ID]);
            return x;
        });

        ReportProgress("After SetKnownColumns");


        _lazyAllExternalServersNode = new LazyWithReset<AllExternalServersNode>(() => { var x = new AllExternalServersNode(); return x; });

        _lazyAllRDMPRemotesNode = new LazyWithReset<AllRDMPRemotesNode>(() =>
        {
            var x = new AllRDMPRemotesNode();
            return x;
        });

        _lazyAllDashboardsNode = new LazyWithReset<AllDashboardsNode>(() => { var x = new AllDashboardsNode(); return x; });
        _lazyAllDashboards = new LazyWithReset<DashboardLayout[]>(() => GetAllObjects<DashboardLayout>(repository));

        _lazyAllObjectSharingNode = new LazyWithReset<AllObjectSharingNode>(() => { var x = new AllObjectSharingNode(); return x; });
        _lazyAllExports = new LazyWithReset<ObjectExport[]>(() =>
        {
            var x = GetAllObjects<ObjectExport>(repository);
            var searchables = new Dictionary<int, HashSet<IMapsDirectlyToDatabaseTable>>();

            //foreach (var o in _descendancyDictionary.Keys.OfType<IMapsDirectlyToDatabaseTable>())
            //{
            //    if (!searchables.ContainsKey(o.ID))
            //        searchables.Add(o.ID, new HashSet<IMapsDirectlyToDatabaseTable>());

            //    searchables[o.ID].Add(o);
            //}

            ReportProgress("After building Searchables");

            foreach (var e in x)
            {
                if (!searchables.TryGetValue(e.ReferencedObjectID, out var searchable))
                    continue;

                var known = searchable
                    .FirstOrDefault(s => e.ReferencedObjectType == s.GetType().FullName);

                if (known != null)
                    e.InjectKnown(known);
            }
            return x;
        });
        _lazyAllImports = new LazyWithReset<ObjectImport[]>(() => GetAllObjects<ObjectImport>(repository));


        ReportProgress("After Object Sharing discovery");

        //Pipelines setup (see also DataExportChildProvider for calls to AddPipelineUseCases)
        //Root node for all pipelines
        _lazyAllPipelinesNode = new LazyWithReset<AllPipelinesNode>(() =>
        {
            var useCases = new Dictionary<string, PipelineUseCase>();
            useCases.Add("File Import", new UploadFileUseCase());
            useCases.Add("Extraction", new ExtractionPipelineUseCase());
            useCases.Add("Release", new ReleaseUseCase());
            useCases.Add("Cohort Creation", new CohortCreationRequest());
            useCases.Add("Caching", new CachingPipelineUseCase());
            useCases.Add("Aggregate Committing", new CreateTableFromAggregateUseCase());

            var x = new AllPipelinesNode();
            var descendancy = new DescendancyList(x);
            var children = new HashSet<object>();
            var unknownPipelines = new HashSet<object>(AllPipelines);

            foreach (var useCase in useCases)
            {
                var node = new StandardPipelineUseCaseNode(useCase.Key, useCase.Value, _commentStore);

                //keep track of all the use cases
                PipelineUseCases.Add(node);

                foreach (var pipeline in GetChildren(node))
                    unknownPipelines.Remove(pipeline);

                children.Add(node);
            }

            children.Add(OtherPipelinesNode);
            OtherPipelinesNode.Pipelines.AddRange(unknownPipelines.Cast<Pipeline>());
            return x;
        });

        //Pipelines not found to be part of any use case after AddPipelineUseCases
        _lazyOtherPipelineNode = new LazyWithReset<OtherPipelinesNode>(() => new OtherPipelinesNode());

        _lazyAllPipelines = new LazyWithReset<Pipeline[]>(() =>
        {
            var x = GetAllObjects<Pipeline>(repository);
            foreach (var p in x)
                p.InjectKnown(AllPipelineComponents.Where(pc => pc.Pipeline_ID == p.ID).ToArray());
            return x;
        });
        _lazyAllPipelineComponents = new LazyWithReset<PipelineComponent[]>(() => GetAllObjects<PipelineComponent>(repository));
        _lazyAllPipelineComponentArgument = new LazyWithReset<PipelineComponentArgument[]>(() => GetAllObjects<PipelineComponentArgument>(repository));


        _lazyAllStandardRegexesNode = new LazyWithReset<AllStandardRegexesNode>(() =>
        {
            var x = new AllStandardRegexesNode();
            return x;
        });
        _lazyAllStandardRegex = new LazyWithReset<StandardRegex[]>(() => GetAllObjects<StandardRegex>(repository));

        ReportProgress("After Pipelines setup");

        //All the things for TableInfoCollectionUI
        BuildServerNodes();

        ReportProgress("BuildServerNodes");

        //add a new CatalogueItemNodes
        InjectCatalogueItems();

        _lazyDatasetRootFolder = new LazyWithReset<FolderNode<Curation.Data.Dataset>>(() =>
        {
            var x = FolderHelper.BuildFolderTree(AllDatasets);
            return x;
        });

        ReportProgress("Build Catalogue Folder Root");

        _lazyLoadMetadataRootFolder = new LazyWithReset<FolderNode<LoadMetadata>>(() =>
        {
            var x = FolderHelper.BuildFolderTree(AllLoadMetadatas.Where(lmd => lmd.RootLoadMetadata_ID is null).ToArray());
            return x;
        });

        _lazyAllTemplateCohortIdentificationConfigurations = new LazyWithReset<CohortIdentificationConfiguration[]>(() => GetAllObjects<CohortIdentificationConfiguration>(repository).Where(cic => cic.IsTemplate).ToArray());
        _lazyCohortidentificationConfigurationRootFolder = new LazyWithReset<FolderNode<CohortIdentificationConfiguration>>(() =>
        {
            var x = FolderHelper.BuildFolderTree(AllCohortIdentificationConfigurations);
            return x;
        });

        _lazyCohortIdentificationConfigurationRootFolderWithoutVersionedConfigurations = new LazyWithReset<FolderNode<CohortIdentificationConfiguration>>(() =>
        {
            var x = FolderHelper.BuildFolderTree(AllCohortIdentificationConfigurations.Where(cic => cic.Version is null).ToArray());
            return x;
        });

        _lazyTemplateAggregateConfigurations = new LazyWithReset<AggregateConfiguration[]>(() => AllAggregateConfigurations
            .Where(ac => new HashSet<int>(
                repository.GetExtendedProperties(ExtendedProperty.IsTemplate)
                    .Where(p => p.ReferencedObjectType.Equals(nameof(AggregateConfiguration)))
                    .Select(r => r.ReferencedObjectID)).Contains(ac.ID)).ToArray());

        _lazyOrphanAggregateConfigurationsNode = new LazyWithReset<AllOrphanAggregateConfigurationsNode>(() =>
        {
            var x = new AllOrphanAggregateConfigurationsNode();
            return x;
        });

        _lazyTemplateAggregateConfigurationsNode = new LazyWithReset<AllTemplateAggregateConfigurationsNode>(() =>
        {
            var x = new AllTemplateAggregateConfigurationsNode();
            var dec = new DescendancyList(x);
            dec.SetBetterRouteExists();
            return x;
        });


        //Some AggregateConfigurations are 'Patient Index Tables', this happens when there is an existing JoinableCohortAggregateConfiguration declared where
        //the AggregateConfiguration_ID is the AggregateConfiguration.ID.  We can inject this knowledge now so to avoid database lookups later (e.g. at icon provision time)

        ReportProgress("After AggregateConfiguration injection");

        _lazyAllGovernanceNode = new LazyWithReset<AllGovernanceNode>(() => { var x = new AllGovernanceNode(); return x; });
        _lazyAllGovernancePeriods = new LazyWithReset<GovernancePeriod[]>(() => GetAllObjects<GovernancePeriod>(repository));
        _lazyAllGovernanceDocuments = new LazyWithReset<GovernanceDocument[]>(() => GetAllObjects<GovernanceDocument>(repository));

        _lazyAllGovernanceCoverage = new LazyWithReset<Dictionary<int, HashSet<int>>>(() => repository.GovernanceManager.GetAllGovernedCataloguesForAllGovernancePeriods());



        ReportProgress("After Governance");

        _lazyAllPluginsNode = new LazyWithReset<AllPluginsNode>(() => { var x = new AllPluginsNode(); return x; });

        ReportProgress("After Plugins");

        _lazyAllRegexRedactionConfigurations = new LazyWithReset<RegexRedactionConfiguration[]>(() => GetAllObjects<RegexRedactionConfiguration>(repository));
        _lazyAllRegexRedactionConfigurationsNode = new LazyWithReset<AllRegexRedactionConfigurationsNode>(() =>
        {
            var x = new AllRegexRedactionConfigurationsNode();
            return x;
        });


        _lazyAllDatasetsNode = new LazyWithReset<AllDatasetsNode>(() => { var x = new AllDatasetsNode(); return x; });

        ReportProgress("After Configurations");

        ReportProgress("After building exports");
    }


    private void FetchCatalogueItems()
    {
        _lazyAllCatalogueItemsDictionary = new LazyWithReset<Dictionary<int, CatalogueItem>>(() =>
        {
            var x = GetAllObjects<CatalogueItem>(_catalogueRepository);
            //Inject known ColumnInfos into CatalogueItems
            Parallel.ForEach(x, ci =>
            {
                if (ci.ColumnInfo_ID != null && _allColumnInfos.TryGetValue(ci.ColumnInfo_ID.Value, out var col))
                    ci.InjectKnown(col);
                else
                    ci.InjectKnown((ColumnInfo)null);
            });
            return x.ToDictionaryEx(i => i.ID, o => o);
        });

        ReportProgress("After CatalogueItem getting");

        _lazy_catalogueToCatalogueItems = new LazyWithReset<Dictionary<int, List<CatalogueItem>>>(() => AllCatalogueItems.GroupBy(c => c.Catalogue_ID).ToDictionaryEx(gdc => gdc.Key, gdc => gdc.ToList()));

        _lazy_allColumnInfos = new LazyWithReset<Dictionary<int, ColumnInfo>>(() => AllColumnInfos.ToDictionaryEx(i => i.ID, o => o));

        ReportProgress("After CatalogueItem Dictionary building");
    }

    private void FetchExtractionInformations()
    {
        _lazyAllExtractionInformationsDictionary = new LazyWithReset<Dictionary<int, ExtractionInformation>>(() =>
        {
            var x = GetAllObjects<ExtractionInformation>(_catalogueRepository);
            //Inject known CatalogueItems into ExtractionInformations
            foreach (var ei in x)
                if (AllCatalogueItemsDictionary.TryGetValue(ei.CatalogueItem_ID, out var ci))
                {
                    ei.InjectKnown(ci.ColumnInfo);
                    ei.InjectKnown(ci);
                }
            return x.ToDictionaryEx(i => i.ID, o => o);
        });
        _lazy_extractionInformationsByCatalogueItem = new LazyWithReset<Dictionary<int, ExtractionInformation>>(() => AllExtractionInformationsDictionary.Values.ToDictionaryEx(k => k.CatalogueItem_ID, v => v));
    }

    private void BuildCohortCohortAggregateContainers()
    {
        _lazyAllCohortAggregateContainers = new LazyWithReset<CohortAggregateContainer[]>(
            () => GetAllObjects<CohortAggregateContainer>(_catalogueRepository)
        );

        //if we have a database repository then we should get answers from the caching version CohortContainerManagerFromChildProvider otherwise
        //just use the one that is configured on the repository
        _cohortContainerManager = _catalogueRepository is CatalogueRepository cataRepo
            ? new CohortContainerManagerFromChildProvider(cataRepo, this)
            : _catalogueRepository.CohortContainerManager;
    }

    private void BuildAggregateConfigurations()
    {
        _lazyAllJoinableCohortAggregateConfigurationUse = new LazyWithReset<JoinableCohortAggregateConfigurationUse[]>(() => GetAllObjects<JoinableCohortAggregateConfigurationUse>(_catalogueRepository));
        _lazyAllAggregateConfigurations = new LazyWithReset<AggregateConfiguration[]>(() =>
        {
            var x = GetAllObjects<AggregateConfiguration>(_catalogueRepository);
            var joinableDictionaryByAggregateConfigurationId = AllJoinables.ToDictionaryEx(j => j.AggregateConfiguration_ID, v => v);


            var y = AllCataloguesDictionary;

            foreach (var ac in x) //if there's a joinable
                ac.InjectKnown( //inject that we know the joinable (and what it is)
                    joinableDictionaryByAggregateConfigurationId.GetValueOrDefault(ac.ID)); //otherwise inject that it is not a joinable (suppresses database checking later)
            foreach (var configuration in x)
            {
                configuration.InjectKnown(AllCataloguesDictionary[configuration.Catalogue_ID]);
                configuration.InjectKnown(AllAggregateDimensions.Where(d => d.AggregateConfiguration_ID == configuration.ID)
                    .ToArray());
            }

            foreach (var d in AllAggregateDimensions)
                d.InjectKnown(AllExtractionInformationsDictionary[d.ExtractionInformation_ID]);
            return x;
        }
        );

        BuildAggregateDimensions();

        //to start with all aggregates are orphans (we prune this as we determine descendency in AddChildren methods
        _lazyOrphanAggregateConfigurations = new LazyWithReset<HashSet<AggregateConfiguration>>(() =>
            new HashSet<AggregateConfiguration>(
                AllAggregateConfigurations.Where(ac => ac.IsCohortIdentificationAggregate)));

        ReportProgress("AggregateDimension injections");

        BuildAggregateFilterContainers();
    }

    private void BuildAggregateDimensions()
    {
        _lazyAllAggregateDimensions = new LazyWithReset<AggregateDimension[]>(() => GetAllObjects<AggregateDimension>(_catalogueRepository));
        _lazyAllAggregateContinuousDataAxis = new LazyWithReset<AggregateContinuousDateAxis[]>(() => GetAllObjects<AggregateContinuousDateAxis>(_catalogueRepository));
    }

    private void BuildAggregateFilterContainers()
    {
        _lazyAllAggregateContainersDictionary = new LazyWithReset<Dictionary<int, AggregateFilterContainer>>(() => GetAllObjects<AggregateFilterContainer>(_catalogueRepository).ToDictionaryEx(o => o.ID, o2 => o2));
        _lazyAllAggregateFilters = new LazyWithReset<AggregateFilter[]>(() => GetAllObjects<AggregateFilter>(_catalogueRepository));
        _lazyAllAggregateFilterParameters = new LazyWithReset<AggregateFilterParameter[]>(() => GetAllObjects<AggregateFilterParameter>(_catalogueRepository));

        _aggregateFilterManager = _catalogueRepository is CatalogueRepository cataRepo
            ? new FilterManagerFromChildProvider(cataRepo, this)
            : _catalogueRepository.FilterManager;
    }


    protected void ReportProgress(string desc)
    {
        if (UserSettings.DebugPerformance)
        {
            _errorsCheckNotifier.OnCheckPerformed(new CheckEventArgs(
                $"ChildProvider Stage {_progress++} ({desc}):{ProgressStopwatch.ElapsedMilliseconds}ms",
                CheckResult.Success));
            ProgressStopwatch.Restart();
        }
    }

    private HashSet<object> GetChildren(AllPluginsNode allPluginsNode)
    {
        return [.. LoadModuleAssembly.Assemblies];
    }

    private HashSet<object> GetChildren(AllRegexRedactionConfigurationsNode allRegexRedactionConfigurationsNode)
    {
        return new HashSet<object>(AllRegexRedactionConfigurations);
    }

    private HashSet<object> GetChildren(AllDatasetsNode allDatasetsNode)
    {
        return new HashSet<object>(AllDatasets);
    }

    private HashSet<object> GetChildren(AllGovernanceNode allGovernanceNode)
    {
        return new HashSet<object>(AllGovernancePeriods);
    }

    private HashSet<object> GetChildren(GovernancePeriod governancePeriod)
    {
        return new HashSet<object>(AllGovernanceDocuments.Where(d => d.GovernancePeriod_ID == governancePeriod.ID));
    }

    private HashSet<object> GetChildren(AllPermissionWindowsNode allPermissionWindowsNode)
    {
        return new HashSet<object>(AllPermissionWindows);
    }

    private HashSet<object> GetChildren(PermissionWindow permissionWindow)
    {
        var children = new HashSet<object>();

        foreach (var cacheProgress in AllCacheProgresses)
            if (cacheProgress.PermissionWindow_ID == permissionWindow.ID)
                children.Add(new PermissionWindowUsedByCacheProgressNode(cacheProgress, permissionWindow, false));
        return children;
    }

    private HashSet<object> GetChildren(AllExternalServersNode allExternalServersNode)
    {
        return new HashSet<object>(AllExternalServers);
    }

    private HashSet<object> GetChildren(AllRDMPRemotesNode allRDMPRemotesNode)
    {
        return new HashSet<object>(AllRemoteRDMPs);
    }

    private HashSet<object> GetChildren(AllDashboardsNode allDashboardsNode)
    {
        return new HashSet<object>(AllDashboards);
    }

    private HashSet<object> GetChildren(AllObjectExportsNode allObjectExportsNode)
    {
        var allExportsNode = new AllObjectExportsNode();
        var allImportsNode = new AllObjectImportsNode();
        return new HashSet<object>(new object[] { allExportsNode, allImportsNode });
    }

    //private void AddChildren(AllObjectSharingNode allObjectSharingNode)
    //{
    //    var descendancy = new DescendancyList(allObjectSharingNode);

    //    var allExportsNode = new AllObjectExportsNode();
    //    var allImportsNode = new AllObjectImportsNode();

    //    AddToDictionaries(new HashSet<object>(AllExports), descendancy.Add(allExportsNode));
    //    AddToDictionaries(new HashSet<object>(AllImports), descendancy.Add(allImportsNode));

    //    AddToDictionaries(new HashSet<object>(new object[] { allExportsNode, allImportsNode }), descendancy);
    //}

    //private HashSet<object> GetChildren()
    //{
    //    return new HashSet<object>();
    //}


    /// <summary>
    /// Creates new <see cref="StandardPipelineUseCaseNode"/>s and fills it with all compatible Pipelines - do not call this method more than once
    /// </summary>
    protected void AddPipelineUseCases(Dictionary<string, PipelineUseCase> useCases)
    {
        var descendancy = new DescendancyList(AllPipelinesNode);
        var children = new HashSet<object>();

        //pipelines not found to be part of any StandardPipelineUseCase
        var unknownPipelines = new HashSet<object>(AllPipelines);

        foreach (var useCase in useCases)
        {
            var node = new StandardPipelineUseCaseNode(useCase.Key, useCase.Value, _commentStore);

            //keep track of all the use cases
            PipelineUseCases.Add(node);

            foreach (var pipeline in GetChildren(node))
                unknownPipelines.Remove(pipeline);

            children.Add(node);
        }

        children.Add(OtherPipelinesNode);
        OtherPipelinesNode.Pipelines.AddRange(unknownPipelines.Cast<Pipeline>());
        //AddToDictionaries(unknownPipelines, descendancy.Add(OtherPipelinesNode));

        //it is the first standard use case
        //AddToDictionaries(children, descendancy);
    }

    private HashSet<object> GetChildren(StandardPipelineUseCaseNode node)
    {
        var children = new HashSet<object>();

        var repo = new MemoryRepository();

        //Could be an issue here if a pipeline becomes compatible with multiple use cases.
        //Should be impossible currently but one day it could be an issue especially if we were to
        //support plugin use cases in this hierarchy

        //find compatible pipelines useCase.Value
        foreach (var compatiblePipeline in AllPipelines.Where(node.UseCase.GetContext().IsAllowable))
        {
            var useCaseNode = new PipelineCompatibleWithUseCaseNode(repo, compatiblePipeline, node.UseCase);

            node.Pipelines.Add(compatiblePipeline);
            children.Add(useCaseNode);
        }
        return children.Cast<PipelineCompatibleWithUseCaseNode>().Select(u => u.Pipeline).ToHashSet<object>();
    }

    private HashSet<object> GetChildren(PipelineCompatibleWithUseCaseNode pipelineNode)
    {
        var components = AllPipelineComponents.Where(c => c.Pipeline_ID == pipelineNode.Pipeline.ID)
            .OrderBy(o => o.Order)
            .ToArray();
        var children = new HashSet<object>(components);
        return children;
    }

    private HashSet<object> GetChildren(PipelineComponent pipelineComponent)
    {
        var components = AllPipelineComponentsArguments.Where(c => c.PipelineComponent_ID == pipelineComponent.ID)
           .ToArray();

        var children = new HashSet<object>(components);
        return children;
    }

    private void BuildServerNodes()
    {
        //add a root node for all the servers to be children of
        _lazyAllServersNode = new LazyWithReset<AllServersNode>(() => new AllServersNode());
        //AllServersNode = new AllServersNode();

        var descendancy = new DescendancyList(AllServersNode);
        var allServers = new List<TableInfoServerNode>();

        foreach (var typeGroup in AllTableInfos.GroupBy(t => t.DatabaseType))
        {
            var dbType = typeGroup.Key;
            IEnumerable<TableInfo> tables = typeGroup;

            var serversByName = tables
                .GroupBy(c => c.Server ?? TableInfoServerNode.NullServerNode, StringComparer.CurrentCultureIgnoreCase)
                .Select(s => new TableInfoServerNode(s.Key, dbType, s));


            foreach (var server in serversByName)
            {
                allServers.Add(server);
            }
        }

        //create the server nodes
        _lazyAllServers = new LazyWithReset<TableInfoServerNode[]>(() => allServers.ToArray());
        //AllServers = allServers.ToArray();

        //record the fact that all the servers are children of the all servers node
        //AddToDictionaries(new HashSet<object>(AllServers), descendancy);
    }

    private HashSet<object> GetChildren(AllDataAccessCredentialsNode allDataAccessCredentialsNode)
    {

        var children = new HashSet<object>();

        var isKeyMissing = false;
        if (_catalogueRepository.EncryptionManager is PasswordEncryptionKeyLocation keyLocation)
            isKeyMissing = string.IsNullOrWhiteSpace(keyLocation.GetKeyFileLocation());

        children.Add(new DecryptionPrivateKeyNode(isKeyMissing));

        foreach (var creds in AllDataAccessCredentials)
            children.Add(creds);
        return children;
    }

    private HashSet<object> GetChildren(AllANOTablesNode aNOTablesNode)
    {
        return new HashSet<object> { AllANOTables };
    }

    private HashSet<object> GetChildren(FolderNode<Catalogue> folder)
    {
        var children = new List<object>();
        var folders = folder.ChildFolders;
        var objects = folder.ChildObjects;
        children.AddRange(folders);
        children.AddRange(objects);
        return new HashSet<object>(children);
    }
    //private void AddChildren(FolderNode<Catalogue> folder, DescendancyList descendancy)
    //{
    //    foreach (var child in folder.ChildFolders)
    //        //add subfolder children
    //        AddChildren(child, descendancy.Add(child));

    //    //add catalogues in folder
    //    foreach (var c in folder.ChildObjects) AddChildren(c, descendancy.Add(c));

    //    // Children are the folders + objects
    //    AddToDictionaries(new HashSet<object>(
    //            folder.ChildFolders.Cast<object>()
    //                .Union(folder.ChildObjects)), descendancy
    //    );
    //}

    private HashSet<object> GetChildren(FolderNode<LoadMetadata> folder)
    {
        var children = new List<object>();
        var folders = folder.ChildFolders;
        var items = folder.ChildObjects;
        children.AddRange(folders);
        children.AddRange(items);
        return new HashSet<object>(children);
    }
    //private void AddChildren(FolderNode<LoadMetadata> folder, DescendancyList descendancy)
    //{
    //    foreach (var child in folder.ChildFolders)
    //        //add subfolder children
    //        AddChildren(child, descendancy.Add(child));

    //    //add loads in folder
    //    foreach (var lmd in folder.ChildObjects.Where(lmd => lmd.RootLoadMetadata_ID == null).ToArray()) AddChildren(lmd, descendancy.Add(lmd));
    //    // Children are the folders + objects
    //    AddToDictionaries(new HashSet<object>(
    //            folder.ChildFolders.Cast<object>()
    //                .Union(folder.ChildObjects)), descendancy
    //    );
    //}


    private HashSet<object> GetChildren(FolderNode<Curation.Data.Dataset> folder)
    {
        var folders = folder.ChildFolders;
        var children = folder.ChildObjects;
        var c = new List<object>();
        c.AddRange(folders);
        c.AddRange(children);
        return new HashSet<object>(c);
    }
    //private void AddChildren(FolderNode<Curation.Data.Dataset> folder, DescendancyList descendancy)
    //{
    //    foreach (var child in folder.ChildFolders)
    //        //add subfolder children
    //        AddChildren(child, descendancy.Add(child));

    //    //add loads in folder
    //    foreach (var ds in folder.ChildObjects) AddChildren(ds, descendancy.Add(ds));

    //    // Children are the folders + objects
    //    AddToDictionaries(new HashSet<object>(
    //            folder.ChildFolders.Cast<object>()
    //                .Union(folder.ChildObjects)), descendancy
    //    );
    //}

    private HashSet<object> GetChildren(FolderNode<CohortIdentificationConfiguration> folder)
    {
        var folders = folder.ChildFolders;
        var children = folder.ChildObjects;
        var c = new List<object>();
        c.AddRange(folders);
        c.AddRange(children);
        return new HashSet<object>(c);
    }
    //private void AddChildren(FolderNode<CohortIdentificationConfiguration> folder, DescendancyList descendancy)
    //{
    //    foreach (var child in folder.ChildFolders)
    //        //add subfolder children
    //        AddChildren(child, descendancy.Add(child));


    //    //add cics in folder
    //    foreach (var cic in folder.ChildObjects) AddChildren(cic, descendancy.Add(cic));

    //    // Children are the folders + objects
    //    AddToDictionaries(new HashSet<object>(
    //            folder.ChildFolders.Cast<object>()
    //                .Union(folder.ChildObjects)), descendancy
    //    );
    //}

    private HashSet<object> GetChildren(Curation.Data.Dataset dataset)
    {
        return new HashSet<object>(new List<object>());
    }

    #region Load Metadata

    private HashSet<object> GetChildren(LoadMetadata lmd, bool includeSchedule = true, bool includeCatalogues = true, bool includeVersions = true)
    {
        var childObjects = new List<object>();
        if (lmd.OverrideRAWServer_ID.HasValue)
        {
            var server = AllExternalServers.Single(s => s.ID == lmd.OverrideRAWServer_ID.Value);
            var usage = new OverrideRawServerNode(lmd, server);
            childObjects.Add(usage);
        }
        if (includeSchedule)
        {
            var allSchedulesNode = new LoadMetadataScheduleNode(lmd);
            childObjects.Add(allSchedulesNode);
        }

        if (includeCatalogues)
        {
            var allCataloguesNode = new AllCataloguesUsedByLoadMetadataNode(lmd);
            childObjects.Add(allCataloguesNode);
        }

        var processTasksNode = new AllProcessTasksUsedByLoadMetadataNode(lmd);
        childObjects.Add(processTasksNode);

        if (includeVersions)
        {
            var versionsNode = new LoadMetadataVersionNode(lmd);
            childObjects.Add(versionsNode);
        }

        childObjects.Add(new LoadDirectoryNode(lmd));

        return new HashSet<object>(childObjects);
    }

    //private void AddChildren(LoadMetadata lmd, DescendancyList descendancy, bool includeSchedule = true, bool includeCatalogues = true, bool includeVersions = true)
    //{
    //    var childObjects = new List<object>();

    //    if (lmd.OverrideRAWServer_ID.HasValue)
    //    {
    //        var server = AllExternalServers.Single(s => s.ID == lmd.OverrideRAWServer_ID.Value);
    //        var usage = new OverrideRawServerNode(lmd, server);
    //        childObjects.Add(usage);
    //    }
    //    if (includeSchedule)
    //    {
    //        var allSchedulesNode = new LoadMetadataScheduleNode(lmd);
    //        AddChildren(allSchedulesNode, descendancy.Add(allSchedulesNode));
    //        childObjects.Add(allSchedulesNode);
    //    }

    //    if (includeCatalogues)
    //    {
    //        var allCataloguesNode = new AllCataloguesUsedByLoadMetadataNode(lmd);
    //        AddChildren(allCataloguesNode, descendancy.Add(allCataloguesNode));
    //        childObjects.Add(allCataloguesNode);
    //    }

    //    var processTasksNode = new AllProcessTasksUsedByLoadMetadataNode(lmd);
    //    AddChildren(processTasksNode, descendancy.Add(processTasksNode));
    //    childObjects.Add(processTasksNode);

    //    if (includeVersions)
    //    {
    //        var versionsNode = new LoadMetadataVersionNode(lmd);
    //        AddChildren(versionsNode, descendancy.Add(versionsNode));
    //        childObjects.Add(versionsNode);
    //    }

    //    childObjects.Add(new LoadDirectoryNode(lmd));

    //    AddToDictionaries(new HashSet<object>(childObjects), descendancy);
    //}

    private HashSet<object> GetChildren(LoadMetadataScheduleNode allScheduleNode)
    {
        var children = new HashSet<object>();
        var lmd = allScheduleNode.LoadMetadata;
        foreach (var lp in AllLoadProgresses.Where(p => p.LoadMetadata_ID == lmd.ID))
        {
            children.Add(lp);
        }
        return children;
    }

    //private void AddChildren(LoadMetadataScheduleNode allSchedulesNode, DescendancyList descendancy)
    //{
    //    var childObjects = new HashSet<object>();

    //    var lmd = allSchedulesNode.LoadMetadata;

    //    foreach (var lp in AllLoadProgresses.Where(p => p.LoadMetadata_ID == lmd.ID))
    //    {
    //        AddChildren(lp, descendancy.Add(lp));
    //        childObjects.Add(lp);
    //    }

    //    if (childObjects.Any())
    //        AddToDictionaries(childObjects, descendancy);
    //}

    private HashSet<object> GetChildren(LoadProgress loadProgress)
    {
        var children = new HashSet<object>();
        var cacheProgresses = AllCacheProgresses.Where(cp => cp.LoadProgress_ID == loadProgress.ID).ToArray();
        foreach (var cacheProgress in cacheProgresses)
            children.Add(cacheProgress);
        return children;
    }

    //private void AddChildren(LoadProgress loadProgress, DescendancyList descendancy)
    //{
    //    var cacheProgresses = AllCacheProgresses.Where(cp => cp.LoadProgress_ID == loadProgress.ID).ToArray();

    //    foreach (var cacheProgress in cacheProgresses)
    //        AddChildren(cacheProgress, descendancy.Add(cacheProgress));

    //    if (cacheProgresses.Any())
    //        AddToDictionaries(new HashSet<object>(cacheProgresses), descendancy);
    //}

    private HashSet<object> GetChildren(CacheProgress cacheProgress)
    {
        var children = new HashSet<object>();

        if (cacheProgress.PermissionWindow_ID != null)
        {
            var window = AllPermissionWindows.Single(w => w.ID == cacheProgress.PermissionWindow_ID);
            var windowNode = new PermissionWindowUsedByCacheProgressNode(cacheProgress, window, true);

            children.Add(windowNode);
        }
        return children;
    }

    private HashSet<object> GetChildren(AllProcessTasksUsedByLoadMetadataNode allProcessTasksUsedByLoadMetadataNode)
    {
        var childObjects = new HashSet<object>();

        var lmd = allProcessTasksUsedByLoadMetadataNode.LoadMetadata;
        childObjects.Add(new LoadStageNode(lmd, LoadStage.GetFiles));
        childObjects.Add(new LoadStageNode(lmd, LoadStage.Mounting));
        childObjects.Add(new LoadStageNode(lmd, LoadStage.AdjustRaw));
        childObjects.Add(new LoadStageNode(lmd, LoadStage.AdjustStaging));
        childObjects.Add(new LoadStageNode(lmd, LoadStage.PostLoad));
        return childObjects;
    }

    private HashSet<object> GetChildren(LoadStageNode loadStageNode)
    {
        var tasks = AllProcessTasks.Where(
              p => p.LoadMetadata_ID == loadStageNode.LoadMetadata.ID && p.LoadStage == loadStageNode.LoadStage)
          .OrderBy(o => o.Order).ToArray();
        return new HashSet<object>(tasks);
    }

    private HashSet<object> GetChildren(ProcessTask processTask)
    {
        var args = AllProcessTasksArguments.Where(
           a => a.ProcessTask_ID == processTask.ID).ToArray();
        return new HashSet<object>(args);
    }

    private HashSet<object> GetChildren(LoadMetadataVersionNode loadMetadataVersionNode)
    {
        var childObjects = AllLoadMetadatas.Where(lmd => lmd.RootLoadMetadata_ID == loadMetadataVersionNode.LoadMetadata.ID).ToList();
        return new HashSet<object> { childObjects };
    }

    private HashSet<object> GetChildren(AllCataloguesUsedByLoadMetadataNode allCataloguesUsedByLoadMetadataNode)
    {
        var loadMetadataId = allCataloguesUsedByLoadMetadataNode.LoadMetadata.ID;
        var linkedCatalogueIDs = AllLoadMetadataLinkage.Where(link => link.LoadMetadataID == loadMetadataId).Select(static link => link.CatalogueID);
        var usedCatalogues = linkedCatalogueIDs.Select(catalogueId => AllCatalogues.FirstOrDefault(c => c.ID == catalogueId)).Where(static foundCatalogue => foundCatalogue is not null).ToList();
        allCataloguesUsedByLoadMetadataNode.UsedCatalogues = usedCatalogues;
        var childObjects = usedCatalogues.Select(foundCatalogue => new CatalogueUsedByLoadMetadataNode(allCataloguesUsedByLoadMetadataNode.LoadMetadata, foundCatalogue)).Cast<object>().ToHashSet();
        return new HashSet<object>(childObjects);
    }
    #endregion

    private HashSet<object> GetChildren(Catalogue c)
    {
        var childObjects = new HashSet<object>();
        var catalogueAggregates = AllAggregateConfigurations.Where(a => a.Catalogue_ID == c.ID).ToArray();
        var cohortAggregates = catalogueAggregates.Where(a => a.IsCohortIdentificationAggregate).ToArray();
        var regularAggregates = catalogueAggregates.Except(cohortAggregates).ToArray();

        //get all the CatalogueItems for this Catalogue (TryGet because Catalogue may not have any items
        var cis = _catalogueToCatalogueItems.TryGetValue(c.ID, out var result)
            ? result.ToArray()
            : Array.Empty<CatalogueItem>();

        //tell the CatalogueItems that we are are their parent
        foreach (var ci in cis)
            ci.InjectKnown(c);

        // core includes project specific which basically means the same thing
        var core = new CatalogueItemsNode(c,
            cis.Where(ci => ci.ExtractionInformation?.ExtractionCategory == ExtractionCategory.Core ||
                            ci.ExtractionInformation?.ExtractionCategory == ExtractionCategory.ProjectSpecific)
            , ExtractionCategory.Core);

        c.InjectKnown(cis);

        var deprecated = new CatalogueItemsNode(c,
            cis.Where(ci => ci.ExtractionInformation?.ExtractionCategory == ExtractionCategory.Deprecated),
            ExtractionCategory.Deprecated);
        var special = new CatalogueItemsNode(c,
            cis.Where(ci => ci.ExtractionInformation?.ExtractionCategory == ExtractionCategory.SpecialApprovalRequired),
            ExtractionCategory.SpecialApprovalRequired);
        var intern = new CatalogueItemsNode(c,
            cis.Where(ci => ci.ExtractionInformation?.ExtractionCategory == ExtractionCategory.Internal),
            ExtractionCategory.Internal);
        var supplemental = new CatalogueItemsNode(c,
            cis.Where(ci => ci.ExtractionInformation?.ExtractionCategory == ExtractionCategory.Supplemental),
            ExtractionCategory.Supplemental);
        var notExtractable = new CatalogueItemsNode(c, cis.Where(ci => ci.ExtractionInformation == null), null);

        childObjects.Add(core);

        foreach (var optional in new[] { deprecated, special, intern, supplemental, notExtractable })
            if (optional.CatalogueItems.Any())
            {
                childObjects.Add(optional);
            }

        //do we have any foreign key fields into this lookup table
        var lookups = AllLookups.Where(l => c.CatalogueItems.Any(ci => ci.ColumnInfo_ID == l.ForeignKey_ID)).ToArray();

        var docs = AllSupportingDocuments.Where(d => d.Catalogue_ID == c.ID).ToArray();
        var sql = AllSupportingSQL.Where(d => d.Catalogue_ID == c.ID).ToArray();

        //if there are supporting documents or supporting sql files then add  documentation node
        if (docs.Any() || sql.Any())
        {
            var documentationNode = new DocumentationNode(c, docs, sql);

            //add the documentations node
            childObjects.Add(documentationNode);
        }

        if (lookups.Any())
        {
            var lookupsNode = new CatalogueLookupsNode(c, lookups);
            //add the documentations node
            childObjects.Add(lookupsNode);
        }

        if (regularAggregates.Any())
        {
            var aggregatesNode = new AggregatesNode(c, regularAggregates);
            childObjects.Add(aggregatesNode);
        }

        return childObjects;
    }

    //protected void AddChildren(Catalogue c, DescendancyList descendancy)
    //{
    //    var childObjects = new List<object>();

    //    var catalogueAggregates = AllAggregateConfigurations.Where(a => a.Catalogue_ID == c.ID).ToArray();
    //    var cohortAggregates = catalogueAggregates.Where(a => a.IsCohortIdentificationAggregate).ToArray();
    //    var regularAggregates = catalogueAggregates.Except(cohortAggregates).ToArray();

    //    //get all the CatalogueItems for this Catalogue (TryGet because Catalogue may not have any items
    //    var cis = _catalogueToCatalogueItems.TryGetValue(c.ID, out var result)
    //        ? result.ToArray()
    //        : Array.Empty<CatalogueItem>();

    //    //tell the CatalogueItems that we are are their parent
    //    foreach (var ci in cis)
    //        ci.InjectKnown(c);

    //    // core includes project specific which basically means the same thing
    //    var core = new CatalogueItemsNode(c,
    //        cis.Where(ci => ci.ExtractionInformation?.ExtractionCategory == ExtractionCategory.Core ||
    //                        ci.ExtractionInformation?.ExtractionCategory == ExtractionCategory.ProjectSpecific)
    //        , ExtractionCategory.Core);

    //    c.InjectKnown(cis);

    //    var deprecated = new CatalogueItemsNode(c,
    //        cis.Where(ci => ci.ExtractionInformation?.ExtractionCategory == ExtractionCategory.Deprecated),
    //        ExtractionCategory.Deprecated);
    //    var special = new CatalogueItemsNode(c,
    //        cis.Where(ci => ci.ExtractionInformation?.ExtractionCategory == ExtractionCategory.SpecialApprovalRequired),
    //        ExtractionCategory.SpecialApprovalRequired);
    //    var intern = new CatalogueItemsNode(c,
    //        cis.Where(ci => ci.ExtractionInformation?.ExtractionCategory == ExtractionCategory.Internal),
    //        ExtractionCategory.Internal);
    //    var supplemental = new CatalogueItemsNode(c,
    //        cis.Where(ci => ci.ExtractionInformation?.ExtractionCategory == ExtractionCategory.Supplemental),
    //        ExtractionCategory.Supplemental);
    //    var notExtractable = new CatalogueItemsNode(c, cis.Where(ci => ci.ExtractionInformation == null), null);

    //    AddChildren(core, descendancy.Add(core));
    //    childObjects.Add(core);

    //    foreach (var optional in new[] { deprecated, special, intern, supplemental, notExtractable })
    //        if (optional.CatalogueItems.Any())
    //        {
    //            AddChildren(optional, descendancy.Add(optional));
    //            childObjects.Add(optional);
    //        }

    //    //do we have any foreign key fields into this lookup table
    //    var lookups = AllLookups.Where(l => c.CatalogueItems.Any(ci => ci.ColumnInfo_ID == l.ForeignKey_ID)).ToArray();

    //    var docs = AllSupportingDocuments.Where(d => d.Catalogue_ID == c.ID).ToArray();
    //    var sql = AllSupportingSQL.Where(d => d.Catalogue_ID == c.ID).ToArray();

    //    //if there are supporting documents or supporting sql files then add  documentation node
    //    if (docs.Any() || sql.Any())
    //    {
    //        var documentationNode = new DocumentationNode(c, docs, sql);

    //        //add the documentations node
    //        childObjects.Add(documentationNode);

    //        //record the children
    //        AddToDictionaries(new HashSet<object>(docs.Cast<object>().Union(sql)), descendancy.Add(documentationNode));
    //    }

    //    if (lookups.Any())
    //    {
    //        var lookupsNode = new CatalogueLookupsNode(c, lookups);
    //        //add the documentations node
    //        childObjects.Add(lookupsNode);


    //        //record the children
    //        AddToDictionaries(new HashSet<object>(lookups.Select(l => new CatalogueLookupUsageNode(c, l))),
    //            descendancy.Add(lookupsNode));
    //    }

    //    if (regularAggregates.Any())
    //    {
    //        var aggregatesNode = new AggregatesNode(c, regularAggregates);
    //        childObjects.Add(aggregatesNode);

    //        var nodeDescendancy = descendancy.Add(aggregatesNode);
    //        AddToDictionaries(new HashSet<object>(regularAggregates), nodeDescendancy);

    //        foreach (var regularAggregate in regularAggregates)
    //            AddChildren(regularAggregate, nodeDescendancy.Add(regularAggregate));
    //    }

    //    //finalise
    //    AddToDictionaries(new HashSet<object>(childObjects), descendancy);
    //}

    private void InjectCatalogueItems()
    {
        foreach (var ci in AllCatalogueItems)
            if (_extractionInformationsByCatalogueItem.TryGetValue(ci.ID, out var ei))
                ci.InjectKnown(ei);
            else
                ci.InjectKnown((ExtractionInformation)null);
    }

    private HashSet<object> GetChildren(CatalogueItemsNode node)
    {
        return new HashSet<object>(node.CatalogueItems);
    }

    private HashSet<object> GetChildren(AggregateConfiguration aggregateConfiguration)
    {
        var childrenObjects = new HashSet<object>();

        var parameters = AllAnyTableParameters.Where(p => p.IsReferenceTo(aggregateConfiguration)).Cast<ISqlParameter>()
            .ToArray();

        foreach (var p in parameters)
            childrenObjects.Add(p);

        // show the dimensions in the tree
        foreach (var dim in aggregateConfiguration.AggregateDimensions) childrenObjects.Add(dim);

        // show the axis (if any) in the tree.  If there are multiple axis in this tree then that is bad but maybe the user can delete one of them to fix the situation
        foreach (var axis in AllAggregateContinuousDateAxis.Where(a =>
                     aggregateConfiguration.AggregateDimensions.Any(d => d.ID == a.AggregateDimension_ID)))
            childrenObjects.Add(axis);

        //we can step into this twice, once via Catalogue children and once via CohortIdentificationConfiguration children
        //if we get in via Catalogue children then descendancy will be Ignore=true we don't end up emphasising into CatalogueCollectionUI when
        //really user wants to see it in CohortIdentificationCollectionUI
        if (aggregateConfiguration.RootFilterContainer_ID != null)
        {
            var container = AllAggregateContainersDictionary[(int)aggregateConfiguration.RootFilterContainer_ID];

            childrenObjects.Add(container);
        }
        return childrenObjects;
    }
    private HashSet<object> GetChildren(AggregateFilterContainer container)
    {
        var childrenObjects = new List<object>();

        var subcontainers = _aggregateFilterManager.GetSubContainers(container);
        var filters = _aggregateFilterManager.GetFilters(container);

        foreach (AggregateFilterContainer subcontainer in subcontainers)
        {
            //one of our children is this subcontainer
            childrenObjects.Add(subcontainer);

            //but also document its children
        }

        //also add the filters for the container
        foreach (var f in filters)
        {
            // for filters add the parameters under them
            childrenObjects.Add(f);
        }
        return new HashSet<object>(childrenObjects);
    }

    private HashSet<object> GetChildren(AggregateFilter f)
    {
        return new HashSet<object>(AllAggregateFilterParameters.Where(p => p.AggregateFilter_ID == f.ID));
    }

    private HashSet<object> GetChildren(CatalogueItem ci)
    {
        var childObjects = new HashSet<object>();
        var ei = ci.ExtractionInformation;
        if (ei != null)
        {
            childObjects.Add(ei);
        }
        else
        {
            ci.InjectKnown(
                (ExtractionInformation)null); // we know the CatalogueItem has no ExtractionInformation child because it's not in the dictionary
        }

        if (ci.ColumnInfo_ID.HasValue && _allColumnInfos.TryGetValue(ci.ColumnInfo_ID.Value, out var col))
            childObjects.Add(new LinkedColumnInfoNode(ci, col));
        return childObjects;
    }

    private HashSet<object> GetChildren(ExtractionInformation extractionInformation)
    {

        var children = new HashSet<object>();

        foreach (var filter in AllCatalogueFilters.Where(f => f.ExtractionInformation_ID == extractionInformation.ID))
        {
            //add the filter as a child of the
            children.Add(filter);
        }
        return children;
    }


    private HashSet<object> GetChildren(ExtractionFilter filter)
    {
        var children = new HashSet<object>();
        var parameters = AllCatalogueParameters.Where(p => p.ExtractionFilter_ID == filter.ID).ToArray();
        var parameterSets = AllCatalogueValueSets.Where(vs => vs.ExtractionFilter_ID == filter.ID).ToArray();

        filter.InjectKnown(parameterSets);

        foreach (var p in parameters)
            children.Add(p);

        foreach (var set in parameterSets)
        {
            children.Add(set);
        }
        return children;
    }

    private HashSet<object> GetChildren(ExtractionFilterParameterSet set, ExtractionFilterParameter[] filterParameters)
    {
        var children = new HashSet<object>();

        foreach (var setValue in AllCatalogueValueSetValues.Where(v => v.ExtractionFilterParameterSet_ID == set.ID))
        {
            setValue.InjectKnown(filterParameters.SingleOrDefault(p => p.ID == setValue.ExtractionFilterParameter_ID));
            children.Add(setValue);
        }
        return children;
    }

    private HashSet<object> GetChildren(CohortIdentificationConfiguration cic)
    {

        var children = new HashSet<object>();

        //it has an associated query cache
        if (cic.QueryCachingServer_ID != null)
            children.Add(new QueryCacheUsedByCohortIdentificationNode(cic,
                AllExternalServers.Single(s => s.ID == cic.QueryCachingServer_ID)));

        var parameters = AllAnyTableParameters.Where(p => p.IsReferenceTo(cic)).Cast<ISqlParameter>().ToArray();
        foreach (var p in parameters) children.Add(p);

        //if it has a root container
        if (cic.RootCohortAggregateContainer_ID != null)
        {
            var container = AllCohortAggregateContainers.Single(c => c.ID == cic.RootCohortAggregateContainer_ID);
            children.Add(container);
        }

        //get the patient index tables
        var joinableNode = new JoinableCollectionNode(cic,
            AllJoinables.Where(j => j.CohortIdentificationConfiguration_ID == cic.ID).ToArray());
        children.Add(joinableNode);
        return children;
    }

    private HashSet<object> GetChildren(JoinableCollectionNode joinablesNode)
    {
        var children = new HashSet<object>();

        foreach (var joinable in joinablesNode.Joinables)
            try
            {
                var agg = AllAggregateConfigurations.Single(ac => ac.ID == joinable.AggregateConfiguration_ID);
                //ForceAggregateNaming(agg, descendancy);
                children.Add(agg);

                //it's no longer an orphan because it's in a known cic (as a patient index table)
                OrphanAggregateConfigurations.Remove(agg);
            }
            catch (Exception e)
            {
                throw new Exception(
                    $"JoinableCohortAggregateConfiguration (patient index table) object (ID={joinable.ID}) references AggregateConfiguration_ID {joinable.AggregateConfiguration_ID} but that AggregateConfiguration was not found",
                    e);
            }
        return children;
    }

    private HashSet<object> GetChildren(CohortAggregateContainer container)
    {
        var subcontainers = _cohortContainerManager.GetChildren(container).OfType<CohortAggregateContainer>().ToList();
        var configurations = _cohortContainerManager.GetChildren(container).OfType<AggregateConfiguration>().ToList();
        var children = subcontainers.Union(configurations.Cast<IOrderable>()).OrderBy(o => o.Order).ToList();
        return new HashSet<object>(children);
    }

    //private void AddChildren(CohortAggregateContainer container, DescendancyList descendancy)
    //{
    //    //get subcontainers
    //    var subcontainers = _cohortContainerManager.GetChildren(container).OfType<CohortAggregateContainer>().ToList();

    //    //if there are subcontainers
    //    foreach (var subcontainer in subcontainers)
    //        AddChildren(subcontainer, descendancy.Add(subcontainer));

    //    //get our configurations
    //    var configurations = _cohortContainerManager.GetChildren(container).OfType<AggregateConfiguration>().ToList();

    //    //record the configurations children including full descendancy
    //    foreach (var configuration in configurations)
    //    {
    //        ForceAggregateNaming(configuration, descendancy);
    //        AddChildren(configuration, descendancy.Add(configuration));

    //        //it's no longer an orphan because it's in a known cic
    //        OrphanAggregateConfigurations.Remove(configuration);
    //    }

    //    //all our children (containers and aggregates)
    //    //children are all aggregates and containers at the current hierarchy level in order
    //    var children = subcontainers.Union(configurations.Cast<IOrderable>()).OrderBy(o => o.Order).ToList();

    //    AddToDictionaries(new HashSet<object>(children), descendancy);
    //}

    private void ForceAggregateNaming(AggregateConfiguration configuration, DescendancyList descendancy)
    {
        //configuration has the wrong name
        if (!configuration.IsCohortIdentificationAggregate)
        {
            _errorsCheckNotifier.OnCheckPerformed(new CheckEventArgs(
                $"Had to fix naming of configuration '{configuration}' because it didn't start with correct cic prefix",
                CheckResult.Warning));
            descendancy.Parents.OfType<CohortIdentificationConfiguration>().Single()
                .EnsureNamingConvention(configuration);
            configuration.SaveToDatabase();
        }
    }


    private HashSet<object> GetChildren(TableInfoServerNode serverNode)
    {
        var children = new HashSet<object>();

        var databases =
            serverNode.Tables.GroupBy(
                    k => k.Database ?? TableInfoDatabaseNode.NullDatabaseNode, StringComparer.CurrentCultureIgnoreCase)
                .Select(g => new TableInfoDatabaseNode(g.Key, serverNode, g));

        foreach (var db in databases)
        {
            children.Add(db);
        }
        return children;
    }
    //private void AddChildren(TableInfoServerNode serverNode, DescendancyList descendancy)
    //{
    //    //add empty hashset
    //    var children = new HashSet<object>();

    //    var databases =
    //        serverNode.Tables.GroupBy(
    //                k => k.Database ?? TableInfoDatabaseNode.NullDatabaseNode, StringComparer.CurrentCultureIgnoreCase)
    //            .Select(g => new TableInfoDatabaseNode(g.Key, serverNode, g));

    //    foreach (var db in databases)
    //    {
    //        children.Add(db);
    //        AddChildren(db, descendancy.Add(db));
    //    }

    //    //now we have recorded all the children add them with descendancy
    //    AddToDictionaries(children, descendancy);
    //}

    private HashSet<object> GetChildren(TableInfoDatabaseNode dbNode)
    {
        var children = new HashSet<object>();

        foreach (var t in dbNode.Tables)
        {
            //record the children of the table infos (mostly column infos)
            children.Add(t);
        }
        return children;
    }
    private HashSet<object> GetChildren(TableInfo tableInfo)
    {
        //add empty hashset
        var children = new HashSet<object>();

        //if the table has an identifier dump listed
        if (tableInfo.IdentifierDumpServer_ID != null)
        {
            //if there is a dump (e.g. for dilution and dumping - not appearing in the live table)
            var server = AllExternalServers.Single(s => s.ID == tableInfo.IdentifierDumpServer_ID.Value);

            children.Add(new IdentifierDumpServerUsageNode(tableInfo, server));
        }

        //get the discarded columns in this table
        var discardedCols = new HashSet<object>(AllPreLoadDiscardedColumns.Where(c => c.TableInfo_ID == tableInfo.ID));

        //tell the column who their parent is so they don't need to look up the database
        foreach (PreLoadDiscardedColumn discardedCol in discardedCols)
            discardedCol.InjectKnown(tableInfo);

        //if there are discarded columns
        if (discardedCols.Any())
        {
            var identifierDumpNode = new PreLoadDiscardedColumnsNode(tableInfo);

            //record that the usage is a child of TableInfo
            children.Add(identifierDumpNode);

            //record that the discarded columns are children of identifier dump usage node
        }

        //if it is a table valued function
        if (tableInfo.IsTableValuedFunction)
        {
            //that has parameters
            var parameters = tableInfo.GetAllParameters();

            foreach (var p in parameters) children.Add(p);
        }

        //next add the column infos
        if (TableInfosToColumnInfos.TryGetValue(tableInfo.ID, out var result))
            foreach (var c in result)
            {
                c.InjectKnown(tableInfo);
                children.Add(c);
            }

        //finally add any credentials objects
        if (AllDataAccessCredentialUsages.TryGetValue(tableInfo, out var nodes))
            foreach (var node in nodes)
                children.Add(node);

        return children;
    }

    private HashSet<object> GetChildren(ColumnInfo columnInfo)
    {
        var lookups = AllLookups.Where(l => l.Description_ID == columnInfo.ID).ToArray();
        var joinInfos = AllJoinInfos.Where(j => j.PrimaryKey_ID == columnInfo.ID);

        var children = new HashSet<object>();

        foreach (var l in lookups)
            children.Add(l);

        foreach (var j in joinInfos)
            children.Add(j);
        return children;
    }

    //protected void AddToDictionaries(HashSet<object> children, DescendancyList list)
    //{
    //    if (list.IsEmpty)
    //        throw new ArgumentException("DescendancyList cannot be empty", nameof(list));

    //    //document that the last parent has these as children
    //    var parent = list.Last();

    //    _childDictionary.AddOrUpdate(parent,
    //        children, (p, s) => children);

    //    //now document the entire parent order to reach each child object i.e. 'Root=>Grandparent=>Parent'  is how you get to 'Child'
    //    foreach (var o in children)
    //        _descendancyDictionary.AddOrUpdate(o, list, (k, v) => HandleDescendancyCollision(k, v, list));


    //    foreach (var masquerader in children.OfType<IMasqueradeAs>())
    //    {
    //        var key = masquerader.MasqueradingAs();

    //        if (!AllMasqueraders.ContainsKey(key))
    //            AllMasqueraders.AddOrUpdate(key, new HashSet<IMasqueradeAs>(), (o, set) => set);

    //        lock (AllMasqueraders)
    //        {
    //            AllMasqueraders[key].Add(masquerader);
    //        }
    //    }
    //}

    private static DescendancyList HandleDescendancyCollision(object key, DescendancyList oldRoute,
        DescendancyList newRoute)
    {
        //if the new route is the best best
        if (newRoute.NewBestRoute && !oldRoute.NewBestRoute)
            return newRoute;

        // If the new one is marked BetterRouteExists just throw away the new one
        return newRoute.BetterRouteExists ? oldRoute : newRoute;
        // If in doubt use the newest one
    }

    //private HashSet<object> GetAllObjects()
    //{
    //    //anything which has children or is a child of someone else (distinct because HashSet)
    //    return new HashSet<object>(_childDictionary.SelectMany(kvp => kvp.Value).Union(_childDictionary.Keys));
    //}

    public virtual object[] GetChildren(object model)
    {
        lock (WriteLock)
        {
            //if we have a record of any children in the child dictionary for the parent model object
            //if (_childDictionary.TryGetValue(model, out var cached))
            //    return cached.OrderBy(static o => o.ToString()).ToArray();
            return model switch
            {
                FolderNode<Catalogue> c => GetChildren(c).ToArray(),
                AllPluginsNode a => GetChildren(a).ToArray(),
                AllRegexRedactionConfigurationsNode a => GetChildren(a).ToArray(),
                AllDashboardsNode a => GetChildren(a).ToArray(),
                AllGovernanceNode a => GetChildren(a).ToArray(),
                GovernancePeriod a => GetChildren(a).ToArray(),
                AllPermissionWindowsNode a => GetChildren(a).ToArray(),
                PermissionWindow a => GetChildren(a).ToArray(),
                AllExternalServersNode a => GetChildren(a).ToArray(),
                AllRDMPRemotesNode a => GetChildren(a).ToArray(),
                AllObjectExportsNode a => GetChildren(a).ToArray(),
                AllDatasetsNode a => GetChildren(a).ToArray(),
                StandardPipelineUseCaseNode a => GetChildren(a).ToArray(),
                PipelineCompatibleWithUseCaseNode a => GetChildren(a).ToArray(),
                AllDataAccessCredentialsNode a => GetChildren(a).ToArray(),
                AllANOTablesNode a => GetChildren(a).ToArray(),
                FolderNode<LoadMetadata> a => GetChildren(a).ToArray(),
                FolderNode<Curation.Data.Dataset> a => GetChildren(a).ToArray(),
                FolderNode<CohortIdentificationConfiguration> a => GetChildren(a).ToArray(),
                Curation.Data.Dataset a => GetChildren(a).ToArray(),
                LoadMetadata a => GetChildren(a).ToArray(),
                LoadMetadataScheduleNode a => GetChildren(a).ToArray(),
                LoadProgress a => GetChildren(a).ToArray(),
                CacheProgress a => GetChildren(a).ToArray(),
                AllProcessTasksUsedByLoadMetadataNode a => GetChildren(a).ToArray(),
                LoadStageNode a => GetChildren(a).ToArray(),
                ProcessTask a => GetChildren(a).ToArray(),
                LoadMetadataVersionNode a => GetChildren(a).ToArray(),
                AllCataloguesUsedByLoadMetadataNode a => GetChildren(a).ToArray(),
                Catalogue a => GetChildren(a).ToArray(),
                CatalogueItemsNode a => GetChildren(a).ToArray(),
                AggregateConfiguration a => GetChildren(a).ToArray(),
                AggregateFilterContainer a => GetChildren(a).ToArray(),
                AggregateFilter a => GetChildren(a).ToArray(),
                CatalogueItem a => GetChildren(a).ToArray(),
                ExtractionInformation a => GetChildren(a).ToArray(),
                ExtractionFilter a => GetChildren(a).ToArray(),
                //ExtractionFilterParameterSet a => GetChildren(a).ToArray(),
                CohortIdentificationConfiguration a => GetChildren(a).ToArray(),
                JoinableCollectionNode a => GetChildren(a).ToArray(),
                CohortAggregateContainer a => GetChildren(a).ToArray(),
                TableInfoDatabaseNode a => GetChildren(a).ToArray(),
                TableInfoServerNode a => GetChildren(a).ToArray(),
                TableInfo a => GetChildren(a).ToArray(),
                ColumnInfo a => GetChildren(a).ToArray(),
                //if they want the children of a Pipeline (which we don't track) just serve the components
                Pipeline p => p.PipelineComponents.ToArray(),
                //if they want the children of a PipelineComponent (which we don't track) just serve the arguments
                PipelineComponent pc => pc.PipelineComponentArguments.ToArray(),
                _ => Array.Empty<object>()
            };
        }
    }

    public IEnumerable<IMapsDirectlyToDatabaseTable> GetAllObjects(Type type, bool unwrapMasqueraders)
    {
        lock (WriteLock)
        {
            //things that are a match on Type but not IMasqueradeAs
            var exactMatches = GetAllSearchables().Keys.Where(t => t is not IMasqueradeAs).Where(type.IsInstanceOfType);

            //Union the unwrapped masqueraders
            return unwrapMasqueraders
                ? exactMatches.Union(
                        AllMasqueraders
                            .Select(kvp => kvp.Key)
                            .OfType<IMapsDirectlyToDatabaseTable>()
                            .Where(type.IsInstanceOfType))
                    .Distinct()
                : exactMatches;
        }
    }

    public DescendancyList GetDescendancyListIfAnyFor(object model)
    {
        //TODO
        return new DescendancyList();
        //lock (WriteLock)
        //{
        //    return _descendancyDictionary.GetValueOrDefault(model);
        //}
    }


    public object GetRootObjectOrSelf(object objectToEmphasise)
    {
        lock (WriteLock)
        {
            var descendancy = GetDescendancyListIfAnyFor(objectToEmphasise);

            return descendancy != null && descendancy.Parents.Any() ? descendancy.Parents[0] : objectToEmphasise;
        }
    }


    public virtual Dictionary<IMapsDirectlyToDatabaseTable, DescendancyList> GetAllSearchables()
    {
        lock (WriteLock)
        {
            var toReturn = new Dictionary<IMapsDirectlyToDatabaseTable, DescendancyList>();
            foreach (var c in AllCatalogues) toReturn.Add(c, new DescendancyList());
            foreach (var c in AllCohortAggregateContainers) toReturn.Add(c, new DescendancyList());
            //AllLoadMetadatas
            //    AllProcessTasks
            //    AllProcessTasksArguments
            //    AllLoadProgresses
            //    AllCacheProgresses
            //    AllPermissionWindows
            //    AllDatasets
            //    AllSupportingDocuments
            //    AllSupportingSQL
            //    AllCatalogueItems
            //    AllAggregateConfigurations
            //    AllAggregateDimensions
            //    AllAggregateContinuousDateAxis
            //    AllRemoteRDMPs
            //    AllDashboards
            //    AllImports
            //    AllExports
            //    AllPipelines
            //    AllPipelineComponents
            //    AllPipelineComponentsArguments
            //    AllANOTables
            //    AllExternalServers
            //    AllServers
            //    AllTableInfos
            //    AllDataAccessCredentials
            //    AllColumnInfos
            //    AllLookups
            //    AllAnyTableParameters
            //    AllExtractionInformations
            //    AllConnectionStringKeywords
            //    AllAggregateContainers
            //    AllAggregateFilters
            //    AllAggregateFilterParameters
            //    AllCatalogueFilters
            //    AllCatalogueParameters
            //    AllCatalogueValueSets
            //    AllCatalogueValueSetValues
            //    AllCohortIdentificationConfigurations
            //    AllCohortAggregateContainers
            //    AllJoinables
            //    AllGovernancePeriods
            //    AllGovernanceDocuments
            //    GovernanceCoverage
            //    AllRegexRedactionConfigurations
            //    OrphanAggregateConfigurations
            //    TemplateAggregateConfigurations
            return toReturn;
        }
    }

    public IEnumerable<object> GetAllChildrenRecursively(object o)
    {
        lock (WriteLock)
        {
            var toReturn = new List<object>();

            foreach (var child in GetChildren(o))
            {
                toReturn.Add(child);
                toReturn.AddRange(GetAllChildrenRecursively(child));
            }

            return toReturn;
        }
    }

    /// <summary>
    /// Asks all plugins to provide the child objects for every object we have found so far.  This method is recursive, call it with null the first time to use all objects.  It will then
    /// call itself with all the new objects that were sent back by the plugin (so that new objects found can still have children).
    /// </summary>
    /// <param name="objectsToAskAbout"></param>
    protected void GetPluginChildren(HashSet<object> objectsToAskAbout = null)
    {
        //TODO
        //lock (WriteLock)
        //{
        //    var newObjectsFound = new HashSet<object>();

        //    var sw = new Stopwatch();

        //    var providers = _pluginChildProviders.Except(_blockedPlugins).ToArray();

        //    //for every object found so far
        //    if (providers.Any())
        //        foreach (var o in objectsToAskAbout ?? GetAllObjects())
        //            //for every plugin loaded (that is not forbidlisted)
        //            foreach (var plugin in providers)
        //                //ask about the children
        //                try
        //                {
        //                    sw.Restart();
        //                    //otherwise ask plugin what its children are
        //                    var pluginChildren = plugin.GetChildren(o);

        //                    //if the plugin takes too long to respond we need to stop
        //                    if (sw.ElapsedMilliseconds > 1000)
        //                    {
        //                        _blockedPlugins.Add(plugin);
        //                        throw new Exception(
        //                            $"Plugin '{plugin}' was forbidlisted for taking too long to respond to GetChildren(o) where o was a '{o.GetType().Name}' ('{o}')");
        //                    }

        //                    //it has children
        //                    if (pluginChildren != null && pluginChildren.Any())
        //                    {
        //                        //get the descendancy of the parent
        //                        var parentDescendancy = GetDescendancyListIfAnyFor(o);
        //                        var newDescendancy = parentDescendancy == null
        //                            ? new DescendancyList(new[] { o })
        //                            : //if the parent is a root level object start a new descendancy list from it
        //                            parentDescendancy
        //                                .Add(o); //otherwise keep going down, returns a new DescendancyList so doesn't corrupt the dictionary one
        //                        newDescendancy =
        //                            parentDescendancy
        //                                .Add(o); //otherwise keep going down, returns a new DescendancyList so doesn't corrupt the dictionary one

        //                        //record that
        //                        foreach (var pluginChild in pluginChildren)
        //                        {
        //                            //if the parent didn't have any children before
        //                            if (!_childDictionary.ContainsKey(o))
        //                                _childDictionary.AddOrUpdate(o, new HashSet<object>(),
        //                                    (o1, set) => set); //it does now


        //                            //add us to the parent objects child collection
        //                            _childDictionary[o].Add(pluginChild);

        //                            //add to the child collection of the parent object kvp.Key
        //                            _descendancyDictionary.AddOrUpdate(pluginChild, newDescendancy,
        //                                (s, e) => newDescendancy);

        //                            //we have found a new object so we must ask other plugins about it (chances are a plugin will have a whole tree of sub objects)
        //                            newObjectsFound.Add(pluginChild);
        //                        }
        //                    }
        //                }
        //                catch (Exception e)
        //                {
        //                    _errorsCheckNotifier.OnCheckPerformed(new CheckEventArgs(e.Message, CheckResult.Fail, e));
        //                }

        //    if (newObjectsFound.Any())
        //        GetPluginChildren(newObjectsFound);
        //}
    }

    public IEnumerable<IMasqueradeAs> GetMasqueradersOf(object o)
    {
        lock (WriteLock)
        {
            return AllMasqueraders.TryGetValue(o, out var result) ? result : Array.Empty<IMasqueradeAs>();
        }
    }

    protected T[] GetAllObjects<T>(IRepository repository) where T : IMapsDirectlyToDatabaseTable
    {
        lock (WriteLock)
        {
            return repository.GetAllObjects<T>();
        }
    }


    protected void AddToReturnSearchablesWithNoDecendancy(
        Dictionary<IMapsDirectlyToDatabaseTable, DescendancyList> toReturn,
        IEnumerable<IMapsDirectlyToDatabaseTable> toAdd)
    {
        lock (WriteLock)
        {
            foreach (var m in toAdd)
                toReturn.Add(m, null);
        }
    }

    public virtual bool SelectiveRefresh(IMapsDirectlyToDatabaseTable databaseEntity)
    {
        ProgressStopwatch.Restart();
        return databaseEntity switch
        {
            Catalogue _ => SelectiveRefresh(typeof(Catalogue)),
            CatalogueItem _ => SelectiveRefresh(typeof(CatalogueItem)),
            LoadMetadata _ => SelectiveRefresh(typeof(LoadMetadata)),
            LoadMetadataCatalogueLinkage _ => SelectiveRefresh(typeof(LoadMetadataCatalogueLinkage)),
            ProcessTask _ => SelectiveRefresh(typeof(ProcessTask)),
            ProcessTaskArgument _ => SelectiveRefresh(typeof(ProcessTaskArgument)),
            LoadProgress _ => SelectiveRefresh(typeof(LoadProgress)),
            CacheProgress _ => SelectiveRefresh(typeof(CacheProgress)),
            PermissionWindow _ => SelectiveRefresh(typeof(PermissionWindow)),
            Curation.Data.Dataset _ => SelectiveRefresh(typeof(Curation.Data.Dataset)),
            SupportingDocument _ => SelectiveRefresh(typeof(SupportingDocument)),
            SupportingSQLTable _ => SelectiveRefresh(typeof(SupportingSQLTable)),
            ColumnInfo _ => SelectiveRefresh(typeof(ColumnInfo)),
            AggregateConfiguration _ => SelectiveRefresh(typeof(AggregateConfiguration)),
            AggregateDimension _ => SelectiveRefresh(typeof(AggregateDimension)),
            AggregateContinuousDateAxis _ => SelectiveRefresh(typeof(AggregateContinuousDateAxis)),
            RemoteRDMP _ => SelectiveRefresh(typeof(RemoteRDMP)),
            DashboardLayout _ => SelectiveRefresh(typeof(DashboardLayout)),
            ObjectImport _ => SelectiveRefresh(typeof(ObjectImport)),
            ObjectExport _ => SelectiveRefresh(typeof(ObjectExport)),
            StandardRegex _ => SelectiveRefresh(typeof(StandardRegex)),
            Pipeline _ => SelectiveRefresh(typeof(Pipeline)),
            PipelineComponent _ => SelectiveRefresh(typeof(PipelineComponent)),
            PipelineComponentArgument _ => SelectiveRefresh(typeof(PipelineComponentArgument)),
            ANOTable _ => SelectiveRefresh(typeof(ANOTable)),
            ExternalDatabaseServer _ => SelectiveRefresh(typeof(ExternalDatabaseServer)),
            TableInfo _ => SelectiveRefresh(typeof(TableInfo)),
            DataAccessCredentials _ => SelectiveRefresh(typeof(DataAccessCredentials)),
            PreLoadDiscardedColumn _ => SelectiveRefresh(typeof(PreLoadDiscardedColumn)),
            Lookup _ => SelectiveRefresh(typeof(Lookup)),
            JoinInfo _ => SelectiveRefresh(typeof(JoinInfo)),
            AnyTableSqlParameter _ => SelectiveRefresh(typeof(AnyTableSqlParameter)),
            ExtractionInformation _ => SelectiveRefresh(typeof(ExtractionInformation)),
            ConnectionStringKeyword _ => SelectiveRefresh(typeof(ConnectionStringKeyword)),
            AggregateFilterContainer _ => SelectiveRefresh(typeof(AggregateFilterContainer)),
            AggregateFilter _ => SelectiveRefresh(typeof(AggregateFilter)),
            AggregateFilterParameter _ => SelectiveRefresh(typeof(AggregateFilterParameter)),
            ExtractionFilter _ => SelectiveRefresh(typeof(ExtractionFilter)),
            ExtractionFilterParameter _ => SelectiveRefresh(typeof(ExtractionFilterParameter)),
            ExtractionFilterParameterSet _ => SelectiveRefresh(typeof(ExtractionFilterParameterSet)),
            ExtractionFilterParameterSetValue _ => SelectiveRefresh(typeof(ExtractionFilterParameterSetValue)),
            CohortIdentificationConfiguration _ => SelectiveRefresh(typeof(CohortIdentificationConfiguration)),
            CohortAggregateContainer _ => SelectiveRefresh(typeof(CohortAggregateContainer)),
            JoinableCohortAggregateConfiguration _ => SelectiveRefresh(typeof(JoinableCohortAggregateConfiguration)),
            JoinableCohortAggregateConfigurationUse _ => SelectiveRefresh(typeof(JoinableCohortAggregateConfigurationUse)),
            GovernancePeriod _ => SelectiveRefresh(typeof(GovernancePeriod)),
            GovernanceDocument _ => SelectiveRefresh(typeof(GovernanceDocument)),
            RegexRedactionConfiguration _ => SelectiveRefresh(typeof(RegexRedactionConfiguration)),
            _ => false
        };
    }



    public bool SelectiveRefreshParents(Type t)
    {
        var results = new List<bool>();
        //TODO
        //if (_descendancyDictionary.TryGetValue(t, out DescendancyList parents))
        //{
        //    foreach (var parent in parents.Parents)
        //    {
        //        results.Add(SelectiveRefresh(parent.GetType()));
        //    }
        //}
        return !results.Contains(false);
    }

    public virtual bool SelectiveRefresh(Type t)
    {
        if (t == typeof(Catalogue))
        {
            _lazyAllCatalogues.Reset();
            _lazyCatalogueRootFolder.Reset();
            _lazyAllServersNode.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(CatalogueItem))
        {
            _lazyAllCatalogueItemsDictionary.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(LoadMetadata))
        {
            _lazyAllLoadMetadatas.Reset();
            _lazyLoadMetadataRootFolder.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(LoadMetadataCatalogueLinkage))
        {
            _lazyAllLoadMetadataLinkage.Reset();
            _lazyAllLoadMetadataCatalogueLinkages.Reset();
            _lazyAllLoadMetadatas.Reset();
            _lazyLoadMetadataRootFolder.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(ProcessTask))
        {
            _lazyAllProcessTasks.Reset();
            _lazyAllLoadMetadatas.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(ProcessTaskArgument))
        {
            _lazyAllProcessTasksArguments.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(LoadProgress))
        {
            _lazyAllLoadProgress.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(CacheProgress))
        {
            _lazyAllCacheProgresses.Reset();
            _lazyAllPermissionWindowsNode.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(PermissionWindow))
        {
            _lazyAllPermissionWindows.Reset();
            _lazyAllPermissionWindowsNode.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(Curation.Data.Dataset))
        {
            _lazyAllDatasets.Reset();
            _lazyAllDatasetsNode.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(SupportingDocument))
        {
            _lazyAllSupportingDocuments.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(SupportingSQLTable))
        {
            _lazyAllSupportingSQL.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(ColumnInfo))
        {
            _lazyAllColumnInfos.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(AggregateConfiguration))
        {
            _lazyAllAggregateConfigurations.Reset();
            _lazyOrphanAggregateConfigurations.Reset();
            _lazyOrphanAggregateConfigurationsNode.Reset();
            _lazyTemplateAggregateConfigurations.Reset();
            _lazyTemplateAggregateConfigurationsNode.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(AggregateDimension))
        {
            _lazyAllAggregateDimensions.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(AggregateContinuousDateAxis))
        {
            _lazyAllAggregateContinuousDataAxis.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(RemoteRDMP))
        {
            _lazyAllRemoteRDMPs.Reset();
            _lazyAllRDMPRemotesNode.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(DashboardLayout))
        {
            _lazyAllDashboards.Reset();
            _lazyAllDashboardsNode.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(ObjectImport))
        {
            _lazyAllImports.Reset();
            _lazyAllObjectSharingNode.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(ObjectExport))
        {
            _lazyAllExports.Reset();
            _lazyAllObjectSharingNode.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(StandardRegex))
        {
            _lazyAllStandardRegex.Reset();
            _lazyAllStandardRegexesNode.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(Pipeline))
        {
            _lazyAllPipelines.Reset();
            _lazyAllPipelineComponents.Reset();
            _lazyAllPipelinesNode.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(PipelineComponent))
        {
            _lazyAllPipelineComponents.Reset();
            _lazyAllPipelines.Reset();
            _lazyAllPipelinesNode.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(PipelineComponentArgument))
        {
            _lazyAllPipelineComponentArgument.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(ANOTable))
        {
            _lazyAllANOTables.Reset();
            _lazyAllANOTableNodes.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(ExternalDatabaseServer))
        {
            _lazyAllExternalServers.Reset();
            _lazyAllExternalServersNode.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(TableInfo))
        {
            _lazyAllTableInfos.Reset();
            _lazyAllANOTableNodes.Reset();
            _lazyAllServersNode.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(DataAccessCredentials))
        {
            _lazyAllDataAccessCredentials.Reset();
            _lazyAllDataAccessCredentialsNode.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(PreLoadDiscardedColumn))
        {
            _lazyAllPreLoadDiscardedColumns.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(Lookup))
        {
            _lazyAllLookups.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(JoinInfo))
        {
            _lazyAllJoinInfos.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(AnyTableSqlParameter))
        {
            _lazyAllAnyTableParameters.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(ExtractionInformation))
        {
            _lazyAllExtractionInformationsDictionary.Reset();
            _lazyAllCatalogueItemsDictionary.Reset();
            _lazy_extractionInformationsByCatalogueItem.Reset();
            _lazyAllCatalogues.Reset();
            _lazyCatalogueRootFolder.Reset();
            InjectCatalogueItems();
            InjectCatalogueItems();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(ConnectionStringKeyword))
        {
            _lazyAllConnectionStringKeywords.Reset();
            _lazyAllConnectionStringKeyworksNode.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(AggregateFilterContainer))
        {
            _lazyAllAggregateContainersDictionary.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(AggregateFilter))
        {
            _lazyAllAggregateFilters.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(AggregateFilterParameter))
        {
            _lazyAllAggregateFilterParameters.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(ExtractionFilter))
        {
            _lazyAllCatalogueFilters.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(ExtractionFilterParameter))
        {
            _lazyAllCatalogueParameters.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(ExtractionFilterParameterSet))
        {
            _lazyAllCatalogueValueSets.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(ExtractionFilterParameterSetValue))
        {
            _lazyAllCatalogueValueSetValues.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(CohortIdentificationConfiguration))
        {
            _lazyAllCohortIdentificationConfigurations.Reset();
            _lazyCohortIdentificationConfigurationRootFolderWithoutVersionedConfigurations.Reset();
            _lazyAllCohortAggregateContainers.Reset();
            _lazyCohortidentificationConfigurationRootFolder.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(CohortAggregateContainer))
        {
            _lazyAllCohortAggregateContainers.Reset();
            _lazyAllCohortIdentificationConfigurations.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(JoinableCohortAggregateConfiguration))
        {
            _lazyAllJoinables.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(JoinableCohortAggregateConfigurationUse))
        {
            _lazyAllJoinableCohortAggregateConfigurationUse.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(GovernancePeriod))
        {
            _lazyAllGovernancePeriods.Reset();
            _lazyAllGovernanceNode.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(GovernanceDocument))
        {
            _lazyAllGovernanceDocuments.Reset();
            _lazyAllGovernanceNode.Reset();
            return SelectiveRefreshParents(t);
        }
        if (t == typeof(RegexRedactionConfiguration))
        {
            _lazyAllRegexRedactionConfigurations.Reset();
            _lazyAllRegexRedactionConfigurationsNode.Reset();
            return SelectiveRefreshParents(t);
        }
        return false;
    }
}