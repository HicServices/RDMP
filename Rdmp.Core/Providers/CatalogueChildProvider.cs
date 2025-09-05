// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
	public LoadMetadata[] AllLoadMetadatas { get; set; }
	public LoadMetadataCatalogueLinkage[] AllLoadMetadataCatalogueLinkages { get; set; }

    private LoadMetadataCatalogueLinkage[] AllLoadMetadataLinkage { get; set; }
	public ProcessTask[] AllProcessTasks { get; set; }
	public ProcessTaskArgument[] AllProcessTasksArguments { get; set; }

	public LoadProgress[] AllLoadProgresses { get; set; }
	public CacheProgress[] AllCacheProgresses { get; set; }
	public PermissionWindow[] AllPermissionWindows { get; set; }

	//Catalogue side of things
	public Catalogue[] AllCatalogues { get; set; }
	public Curation.Data.Dataset[] AllDatasets { get; set; }
	public Dictionary<int, Catalogue> AllCataloguesDictionary { get; private set; }

	public SupportingDocument[] AllSupportingDocuments { get; set; }
	public SupportingSQLTable[] AllSupportingSQL { get; set; }

	//tells you the immediate children of a given node.  Do not add to this directly instead add using AddToDictionaries unless you want the Key to be an 'on the sly' no known descendency child
	private ConcurrentDictionary<object, HashSet<object>> _childDictionary = new();

	//This is the reverse of _childDictionary in some ways.  _childDictionary tells you the immediate children while
	//this tells you for a given child object what the navigation tree down to get to it is e.g. ascendancy[child] would return [root,grandParent,parent]
	private ConcurrentDictionary<object, DescendancyList> _descendancyDictionary = new();

	public IEnumerable<CatalogueItem> AllCatalogueItems => AllCatalogueItemsDictionary.Values;

	private Dictionary<int, List<CatalogueItem>> _catalogueToCatalogueItems;
	public Dictionary<int, CatalogueItem> AllCatalogueItemsDictionary { get; private set; }

	private Dictionary<int, ColumnInfo> _allColumnInfos;

	public AggregateConfiguration[] AllAggregateConfigurations { get; private set; }
	public AggregateDimension[] AllAggregateDimensions { get; private set; }

	public AggregateContinuousDateAxis[] AllAggregateContinuousDateAxis { get; private set; }

	public AllRDMPRemotesNode AllRDMPRemotesNode { get; private set; }
	public RemoteRDMP[] AllRemoteRDMPs { get; set; }

	public AllDashboardsNode AllDashboardsNode { get; set; }
	public DashboardLayout[] AllDashboards { get; set; }

	public AllObjectSharingNode AllObjectSharingNode { get; private set; }
	public ObjectImport[] AllImports { get; set; }
	public ObjectExport[] AllExports { get; set; }

	public AllStandardRegexesNode AllStandardRegexesNode { get; private set; }
	public AllPipelinesNode AllPipelinesNode { get; private set; }
	public OtherPipelinesNode OtherPipelinesNode { get; private set; }
	public Pipeline[] AllPipelines { get; set; }
	public PipelineComponent[] AllPipelineComponents { get; set; }

	public PipelineComponentArgument[] AllPipelineComponentsArguments { get; set; }

	public StandardRegex[] AllStandardRegexes { get; set; }

	//TableInfo side of things
	public AllANOTablesNode AllANOTablesNode { get; private set; }
	public ANOTable[] AllANOTables { get; set; }

	public ExternalDatabaseServer[] AllExternalServers { get; private set; }
	public TableInfoServerNode[] AllServers { get; private set; }
	public TableInfo[] AllTableInfos { get; private set; }

	public AllDataAccessCredentialsNode AllDataAccessCredentialsNode { get; set; }

	public AllExternalServersNode AllExternalServersNode { get; private set; }
	public AllServersNode AllServersNode { get; private set; }

	public DataAccessCredentials[] AllDataAccessCredentials { get; set; }
	public Dictionary<ITableInfo, List<DataAccessCredentialUsageNode>> AllDataAccessCredentialUsages { get; set; }

	public Dictionary<int, List<ColumnInfo>> TableInfosToColumnInfos { get; private set; }
	public ColumnInfo[] AllColumnInfos { get; private set; }
	public PreLoadDiscardedColumn[] AllPreLoadDiscardedColumns { get; private set; }

	public Lookup[] AllLookups { get; set; }

	public JoinInfo[] AllJoinInfos { get; set; }

	public AnyTableSqlParameter[] AllAnyTableParameters;

	//Filter / extraction side of things
	public IEnumerable<ExtractionInformation> AllExtractionInformations => AllExtractionInformationsDictionary.Values;

	public AllPermissionWindowsNode AllPermissionWindowsNode { get; set; }
	public FolderNode<LoadMetadata> LoadMetadataRootFolder { get; set; }

	public FolderNode<Curation.Data.Dataset> DatasetRootFolder { get; set; }
	public FolderNode<CohortIdentificationConfiguration> CohortIdentificationConfigurationRootFolder { get; set; }
	public FolderNode<CohortIdentificationConfiguration> CohortIdentificationConfigurationRootFolderWithoutVersionedConfigurations { get; set; }

	public AllConnectionStringKeywordsNode AllConnectionStringKeywordsNode { get; set; }
	public ConnectionStringKeyword[] AllConnectionStringKeywords { get; set; }

	public Dictionary<int, ExtractionInformation> AllExtractionInformationsDictionary { get; private set; }
	protected Dictionary<int, ExtractionInformation> _extractionInformationsByCatalogueItem;

	private IFilterManager _aggregateFilterManager;

	//Filters for Aggregates (includes filter containers (AND/OR)
	public Dictionary<int, AggregateFilterContainer> AllAggregateContainersDictionary { get; private set; }
	public AggregateFilterContainer[] AllAggregateContainers => AllAggregateContainersDictionary.Values.ToArray();

	public AggregateFilter[] AllAggregateFilters { get; private set; }
	public AggregateFilterParameter[] AllAggregateFilterParameters { get; private set; }

	//Catalogue master filters (does not include any support for filter containers (AND/OR)
	private ExtractionFilter[] AllCatalogueFilters;
	public ExtractionFilterParameter[] AllCatalogueParameters;
	public ExtractionFilterParameterSet[] AllCatalogueValueSets;
	public ExtractionFilterParameterSetValue[] AllCatalogueValueSetValues;

	private ICohortContainerManager _cohortContainerManager;

	public CohortIdentificationConfiguration[] AllCohortIdentificationConfigurations { get; private set; }
	public CohortAggregateContainer[] AllCohortAggregateContainers { get; set; }
	public JoinableCohortAggregateConfiguration[] AllJoinables { get; set; }
	public JoinableCohortAggregateConfigurationUse[] AllJoinUses { get; set; }

	/// <summary>
	/// Collection of all objects for which there are masqueraders
	/// </summary>
	public ConcurrentDictionary<object, HashSet<IMasqueradeAs>> AllMasqueraders { get; private set; }

	private IChildProvider[] _pluginChildProviders;
	private readonly ICatalogueRepository _catalogueRepository;
	private readonly ICheckNotifier _errorsCheckNotifier;
	private readonly List<IChildProvider> _blockedPlugins = new();

	public AllGovernanceNode AllGovernanceNode { get; private set; }
	public GovernancePeriod[] AllGovernancePeriods { get; private set; }
	public GovernanceDocument[] AllGovernanceDocuments { get; private set; }
	public Dictionary<int, HashSet<int>> GovernanceCoverage { get; private set; }

	private CommentStore _commentStore;

	public JoinableCohortAggregateConfigurationUse[] AllJoinableCohortAggregateConfigurationUse { get; private set; }
	public AllPluginsNode AllPluginsNode { get; private set; }
	public HashSet<StandardPipelineUseCaseNode> PipelineUseCases { get; set; } = new();

	/// <summary>
	/// Lock for changes to Child provider
	/// </summary>
	protected object WriteLock = new();

	public AllOrphanAggregateConfigurationsNode OrphanAggregateConfigurationsNode { get; set; } = new();
	public AllTemplateAggregateConfigurationsNode TemplateAggregateConfigurationsNode { get; set; } = new();
	public FolderNode<Catalogue> CatalogueRootFolder { get; private set; }

	public AllDatasetsNode AllDatasetsNode { get; set; }

	public RegexRedactionConfiguration[] AllRegexRedactionConfigurations { get; set; }
	public AllRegexRedactionConfigurationsNode AllRegexRedactionConfigurationsNode { get; set; }

	public HashSet<AggregateConfiguration> OrphanAggregateConfigurations;
	public AggregateConfiguration[] TemplateAggregateConfigurations;


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
		_commentStore = repository.CommentStore;
		_catalogueRepository = repository;
		_catalogueRepository?.EncryptionManager?.ClearAllInjections();

		_errorsCheckNotifier = errorsCheckNotifier ?? IgnoreAllErrorsCheckNotifier.Instance;

		if (UserSettings.DebugPerformance)
			_errorsCheckNotifier.OnCheckPerformed(new CheckEventArgs(
				$"Refresh generated by:{Environment.NewLine}{Environment.StackTrace}", CheckResult.Success));

		// all the objects which are
		AllMasqueraders = new ConcurrentDictionary<object, HashSet<IMasqueradeAs>>();

		_pluginChildProviders = pluginChildProviders ?? Array.Empty<IChildProvider>();

		ReportProgress("Before object fetches");

		AllAnyTableParameters = GetAllObjects<AnyTableSqlParameter>(repository);

		AllANOTables = GetAllObjects<ANOTable>(repository);
		AllANOTablesNode = new AllANOTablesNode();
		AddChildren(AllANOTablesNode);

		AllCatalogues = GetAllObjects<Catalogue>(repository);
		AllCataloguesDictionary = AllCatalogues.ToDictionaryEx(i => i.ID, o => o);

		AllDatasets = GetAllObjects<Curation.Data.Dataset>(repository);

		AllLoadMetadatas = GetAllObjects<LoadMetadata>(repository);
		AllLoadMetadataCatalogueLinkages = GetAllObjects<LoadMetadataCatalogueLinkage>(repository);
        AllLoadMetadataLinkage = GetAllObjects<LoadMetadataCatalogueLinkage>(repository);
		AllProcessTasks = GetAllObjects<ProcessTask>(repository);
		AllProcessTasksArguments = GetAllObjects<ProcessTaskArgument>(repository);
		AllLoadProgresses = GetAllObjects<LoadProgress>(repository);
		AllCacheProgresses = GetAllObjects<CacheProgress>(repository);

		AllPermissionWindows = GetAllObjects<PermissionWindow>(repository);
		AllPermissionWindowsNode = new AllPermissionWindowsNode();
		AddChildren(AllPermissionWindowsNode);

		AllRemoteRDMPs = GetAllObjects<RemoteRDMP>(repository);

		AllExternalServers = GetAllObjects<ExternalDatabaseServer>(repository);

		AllTableInfos = GetAllObjects<TableInfo>(repository);
		AllDataAccessCredentials = GetAllObjects<DataAccessCredentials>(repository);
		AllDataAccessCredentialsNode = new AllDataAccessCredentialsNode();
		AddChildren(AllDataAccessCredentialsNode);

		AllConnectionStringKeywordsNode = new AllConnectionStringKeywordsNode();
		AllConnectionStringKeywords = GetAllObjects<ConnectionStringKeyword>(repository).ToArray();
		AddToDictionaries(new HashSet<object>(AllConnectionStringKeywords),
			new DescendancyList(AllConnectionStringKeywordsNode));

		ReportProgress("after basic object fetches");

		Task.WaitAll(
			//which TableInfos use which Credentials under which DataAccessContexts
			Task.Factory.StartNew(() =>
			{
				AllDataAccessCredentialUsages =
					repository.TableInfoCredentialsManager.GetAllCredentialUsagesBy(AllDataAccessCredentials,
						AllTableInfos);
			}),
			Task.Factory.StartNew(() => { AllColumnInfos = GetAllObjects<ColumnInfo>(repository); })
		);

		ReportProgress("After credentials");

		TableInfosToColumnInfos = AllColumnInfos.GroupBy(c => c.TableInfo_ID)
			.ToDictionaryEx(gdc => gdc.Key, gdc => gdc.ToList());

		ReportProgress("After TableInfo to ColumnInfo mapping");

		AllPreLoadDiscardedColumns = GetAllObjects<PreLoadDiscardedColumn>(repository);

		AllSupportingDocuments = GetAllObjects<SupportingDocument>(repository);
		AllSupportingSQL = GetAllObjects<SupportingSQLTable>(repository);

		AllCohortIdentificationConfigurations = GetAllObjects<CohortIdentificationConfiguration>(repository);

		FetchCatalogueItems();

		ReportProgress("After CatalogueItem injection");

		FetchExtractionInformations();

		ReportProgress("After ExtractionInformation injection");

		BuildAggregateConfigurations();

		BuildCohortCohortAggregateContainers();

		AllJoinables = GetAllObjects<JoinableCohortAggregateConfiguration>(repository);
		AllJoinUses = GetAllObjects<JoinableCohortAggregateConfigurationUse>(repository);

		AllCatalogueFilters = GetAllObjects<ExtractionFilter>(repository);
		AllCatalogueParameters = GetAllObjects<ExtractionFilterParameter>(repository);
		AllCatalogueValueSets = GetAllObjects<ExtractionFilterParameterSet>(repository);
		AllCatalogueValueSetValues = GetAllObjects<ExtractionFilterParameterSetValue>(repository);

		ReportProgress("After Filter and Joinable fetching");


		AllLookups = GetAllObjects<Lookup>(repository);

		foreach (var l in AllLookups)
			l.SetKnownColumns(_allColumnInfos[l.PrimaryKey_ID], _allColumnInfos[l.ForeignKey_ID],
				_allColumnInfos[l.Description_ID]);

		AllJoinInfos = repository.GetAllObjects<JoinInfo>();

		foreach (var j in AllJoinInfos)
			j.SetKnownColumns(_allColumnInfos[j.PrimaryKey_ID], _allColumnInfos[j.ForeignKey_ID]);

		ReportProgress("After SetKnownColumns");

		AllExternalServersNode = new AllExternalServersNode();
		AddChildren(AllExternalServersNode);

		AllRDMPRemotesNode = new AllRDMPRemotesNode();
		AddChildren(AllRDMPRemotesNode);

		AllDashboardsNode = new AllDashboardsNode();
		AllDashboards = GetAllObjects<DashboardLayout>(repository);
		AddChildren(AllDashboardsNode);

		AllObjectSharingNode = new AllObjectSharingNode();
		AllExports = GetAllObjects<ObjectExport>(repository);
		AllImports = GetAllObjects<ObjectImport>(repository);

		AddChildren(AllObjectSharingNode);

		ReportProgress("After Object Sharing discovery");

		//Pipelines setup (see also DataExportChildProvider for calls to AddPipelineUseCases)
		//Root node for all pipelines
		AllPipelinesNode = new AllPipelinesNode();

		//Pipelines not found to be part of any use case after AddPipelineUseCases
		OtherPipelinesNode = new OtherPipelinesNode();
		AllPipelines = GetAllObjects<Pipeline>(repository);
		AllPipelineComponents = GetAllObjects<PipelineComponent>(repository);
		AllPipelineComponentsArguments = GetAllObjects<PipelineComponentArgument>(repository);

		foreach (var p in AllPipelines)
			p.InjectKnown(AllPipelineComponents.Where(pc => pc.Pipeline_ID == p.ID).ToArray());

		AllStandardRegexesNode = new AllStandardRegexesNode();
		AllStandardRegexes = GetAllObjects<StandardRegex>(repository);
		AddToDictionaries(new HashSet<object>(AllStandardRegexes), new DescendancyList(AllStandardRegexesNode));

		ReportProgress("After Pipelines setup");

		//All the things for TableInfoCollectionUI
		BuildServerNodes();

		ReportProgress("BuildServerNodes");

		//add a new CatalogueItemNodes
		InjectCatalogueItems();

		CatalogueRootFolder = FolderHelper.BuildFolderTree(AllCatalogues);
		AddChildren(CatalogueRootFolder, new DescendancyList(CatalogueRootFolder));


		DatasetRootFolder = FolderHelper.BuildFolderTree(AllDatasets);
		AddChildren(DatasetRootFolder, new DescendancyList(DatasetRootFolder));

		ReportProgress("Build Catalogue Folder Root");

		LoadMetadataRootFolder = FolderHelper.BuildFolderTree(AllLoadMetadatas.Where(lmd => lmd.RootLoadMetadata_ID is null).ToArray());
		AddChildren(LoadMetadataRootFolder, new DescendancyList(LoadMetadataRootFolder));

		CohortIdentificationConfigurationRootFolder =
			FolderHelper.BuildFolderTree(AllCohortIdentificationConfigurations);
		AddChildren(CohortIdentificationConfigurationRootFolder,
			new DescendancyList(CohortIdentificationConfigurationRootFolder));

		CohortIdentificationConfigurationRootFolderWithoutVersionedConfigurations = FolderHelper.BuildFolderTree(AllCohortIdentificationConfigurations.Where(cic => cic.Version is null).ToArray());
		AddChildren(CohortIdentificationConfigurationRootFolderWithoutVersionedConfigurations,
		   new DescendancyList(CohortIdentificationConfigurationRootFolderWithoutVersionedConfigurations));
		var templateAggregateConfigurationIds =
			new HashSet<int>(
				repository.GetExtendedProperties(ExtendedProperty.IsTemplate)
					.Where(p => p.ReferencedObjectType.Equals(nameof(AggregateConfiguration)))
					.Select(r => r.ReferencedObjectID));

		TemplateAggregateConfigurations = AllAggregateConfigurations
			.Where(ac => templateAggregateConfigurationIds.Contains(ac.ID)).ToArray();

		//add the orphans under the orphan folder
		AddToDictionaries(new HashSet<object>(OrphanAggregateConfigurations),
			new DescendancyList(OrphanAggregateConfigurationsNode));

		var dec = new DescendancyList(TemplateAggregateConfigurationsNode);
		dec.SetBetterRouteExists();
		AddToDictionaries(new HashSet<object>(TemplateAggregateConfigurations), dec);

		//Some AggregateConfigurations are 'Patient Index Tables', this happens when there is an existing JoinableCohortAggregateConfiguration declared where
		//the AggregateConfiguration_ID is the AggregateConfiguration.ID.  We can inject this knowledge now so to avoid database lookups later (e.g. at icon provision time)
		var joinableDictionaryByAggregateConfigurationId =
			AllJoinables.ToDictionaryEx(j => j.AggregateConfiguration_ID, v => v);

		foreach (var ac in AllAggregateConfigurations) //if there's a joinable
			ac.InjectKnown( //inject that we know the joinable (and what it is)
				joinableDictionaryByAggregateConfigurationId.GetValueOrDefault(ac.ID)); //otherwise inject that it is not a joinable (suppresses database checking later)

		ReportProgress("After AggregateConfiguration injection");

		AllGovernanceNode = new AllGovernanceNode();
		AllGovernancePeriods = GetAllObjects<GovernancePeriod>(repository);
		AllGovernanceDocuments = GetAllObjects<GovernanceDocument>(repository);
		GovernanceCoverage = repository.GovernanceManager.GetAllGovernedCataloguesForAllGovernancePeriods();

		AddChildren(AllGovernanceNode);

		ReportProgress("After Governance");

		AllPluginsNode = new AllPluginsNode();
		AddChildren(AllPluginsNode);

		ReportProgress("After Plugins");

		AllRegexRedactionConfigurations = GetAllObjects<RegexRedactionConfiguration>(repository);
		AllRegexRedactionConfigurationsNode = new AllRegexRedactionConfigurationsNode();
		AddChildren(AllRegexRedactionConfigurationsNode);


		AllDatasets = GetAllObjects<Curation.Data.Dataset>(repository);
		AllDatasetsNode = new AllDatasetsNode();
		AddChildren(AllDatasetsNode);

		ReportProgress("After Configurations");

		var searchables = new Dictionary<int, HashSet<IMapsDirectlyToDatabaseTable>>();

		foreach (var o in _descendancyDictionary.Keys.OfType<IMapsDirectlyToDatabaseTable>())
		{
			if (!searchables.ContainsKey(o.ID))
				searchables.Add(o.ID, new HashSet<IMapsDirectlyToDatabaseTable>());

			searchables[o.ID].Add(o);
		}

		ReportProgress("After building Searchables");

		foreach (var e in AllExports)
		{
			if (!searchables.TryGetValue(e.ReferencedObjectID, out var searchable))
				continue;

			var known = searchable
				.FirstOrDefault(s => e.ReferencedObjectType == s.GetType().FullName);

			if (known != null)
				e.InjectKnown(known);
		}

		ReportProgress("After building exports");
	}


	private void FetchCatalogueItems()
	{
		AllCatalogueItemsDictionary =
			GetAllObjects<CatalogueItem>(_catalogueRepository).ToDictionaryEx(i => i.ID, o => o);

		ReportProgress("After CatalogueItem getting");

		_catalogueToCatalogueItems = AllCatalogueItems.GroupBy(c => c.Catalogue_ID)
			.ToDictionaryEx(gdc => gdc.Key, gdc => gdc.ToList());
		_allColumnInfos = AllColumnInfos.ToDictionaryEx(i => i.ID, o => o);

		ReportProgress("After CatalogueItem Dictionary building");

		//Inject known ColumnInfos into CatalogueItems
		Parallel.ForEach(AllCatalogueItems, ci =>
		{
			if (ci.ColumnInfo_ID != null && _allColumnInfos.TryGetValue(ci.ColumnInfo_ID.Value, out var col))
				ci.InjectKnown(col);
			else
				ci.InjectKnown((ColumnInfo)null);
		});
	}

	private void FetchExtractionInformations()
	{
		AllExtractionInformationsDictionary = GetAllObjects<ExtractionInformation>(_catalogueRepository)
			.ToDictionaryEx(i => i.ID, o => o);
		_extractionInformationsByCatalogueItem =
			AllExtractionInformationsDictionary.Values.ToDictionaryEx(k => k.CatalogueItem_ID, v => v);

		//Inject known CatalogueItems into ExtractionInformations
		foreach (var ei in AllExtractionInformationsDictionary.Values)
			if (AllCatalogueItemsDictionary.TryGetValue(ei.CatalogueItem_ID, out var ci))
			{
				ei.InjectKnown(ci.ColumnInfo);
				ei.InjectKnown(ci);
			}
	}

	private void BuildCohortCohortAggregateContainers()
	{
		AllCohortAggregateContainers = GetAllObjects<CohortAggregateContainer>(_catalogueRepository);


		//if we have a database repository then we should get answers from the caching version CohortContainerManagerFromChildProvider otherwise
		//just use the one that is configured on the repository.

		_cohortContainerManager = _catalogueRepository is CatalogueRepository cataRepo
			? new CohortContainerManagerFromChildProvider(cataRepo, this)
			: _catalogueRepository.CohortContainerManager;
	}

	private void BuildAggregateConfigurations()
	{
		AllJoinableCohortAggregateConfigurationUse =
			GetAllObjects<JoinableCohortAggregateConfigurationUse>(_catalogueRepository);
		AllAggregateConfigurations = GetAllObjects<AggregateConfiguration>(_catalogueRepository);

		BuildAggregateDimensions();

		//to start with all aggregates are orphans (we prune this as we determine descendency in AddChildren methods
		OrphanAggregateConfigurations =
			new HashSet<AggregateConfiguration>(
				AllAggregateConfigurations.Where(ac => ac.IsCohortIdentificationAggregate));

		foreach (var configuration in AllAggregateConfigurations)
		{
			configuration.InjectKnown(AllCataloguesDictionary[configuration.Catalogue_ID]);
			configuration.InjectKnown(AllAggregateDimensions.Where(d => d.AggregateConfiguration_ID == configuration.ID)
				.ToArray());
		}

		foreach (var d in AllAggregateDimensions)
			d.InjectKnown(AllExtractionInformationsDictionary[d.ExtractionInformation_ID]);

		ReportProgress("AggregateDimension injections");

		BuildAggregateFilterContainers();
	}

	private void BuildAggregateDimensions()
	{
		AllAggregateDimensions = GetAllObjects<AggregateDimension>(_catalogueRepository);
		AllAggregateContinuousDateAxis = GetAllObjects<AggregateContinuousDateAxis>(_catalogueRepository);
	}

	private void BuildAggregateFilterContainers()
	{
		AllAggregateContainersDictionary = GetAllObjects<AggregateFilterContainer>(_catalogueRepository)
			.ToDictionaryEx(o => o.ID, o2 => o2);
		AllAggregateFilters = GetAllObjects<AggregateFilter>(_catalogueRepository);
		AllAggregateFilterParameters = GetAllObjects<AggregateFilterParameter>(_catalogueRepository);

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

	private void AddChildren(AllPluginsNode allPluginsNode)
	{
		var children = new HashSet<object>(LoadModuleAssembly.Assemblies);
		var descendancy = new DescendancyList(allPluginsNode);
		AddToDictionaries(children, descendancy);
	}

	private void AddChildren(AllRegexRedactionConfigurationsNode allRegexRedactionConfigurationsNode)
	{
		var children = new HashSet<object>(AllRegexRedactionConfigurations);
		var descendancy = new DescendancyList(allRegexRedactionConfigurationsNode);
		AddToDictionaries(children, descendancy);
	}

	private void AddChildren(AllDatasetsNode allDatasetsNode)
	{
		var children = new HashSet<object>(AllDatasets);
		var descendancy = new DescendancyList(allDatasetsNode);
		AddToDictionaries(children, descendancy);
	}

	private void AddChildren(AllGovernanceNode allGovernanceNode)
	{
		var children = new HashSet<object>();
		var descendancy = new DescendancyList(allGovernanceNode);

		foreach (var gp in AllGovernancePeriods)
		{
			children.Add(gp);
			AddChildren(gp, descendancy.Add(gp));
		}

		AddToDictionaries(children, descendancy);
	}

	private void AddChildren(GovernancePeriod governancePeriod, DescendancyList descendancy)
	{
		var children = new HashSet<object>();

		foreach (var doc in AllGovernanceDocuments.Where(d => d.GovernancePeriod_ID == governancePeriod.ID))
			children.Add(doc);

		AddToDictionaries(children, descendancy);
	}

	private void AddChildren(AllPermissionWindowsNode allPermissionWindowsNode)
	{
		var descendancy = new DescendancyList(allPermissionWindowsNode);

		foreach (var permissionWindow in AllPermissionWindows)
			AddChildren(permissionWindow, descendancy.Add(permissionWindow));


		AddToDictionaries(new HashSet<object>(AllPermissionWindows), descendancy);
	}

	private void AddChildren(PermissionWindow permissionWindow, DescendancyList descendancy)
	{
		var children = new HashSet<object>();

		foreach (var cacheProgress in AllCacheProgresses)
			if (cacheProgress.PermissionWindow_ID == permissionWindow.ID)
				children.Add(new PermissionWindowUsedByCacheProgressNode(cacheProgress, permissionWindow, false));

		AddToDictionaries(children, descendancy);
	}

	private void AddChildren(AllExternalServersNode allExternalServersNode)
	{
		AddToDictionaries(new HashSet<object>(AllExternalServers), new DescendancyList(allExternalServersNode));
	}

	private void AddChildren(AllRDMPRemotesNode allRDMPRemotesNode)
	{
		AddToDictionaries(new HashSet<object>(AllRemoteRDMPs), new DescendancyList(allRDMPRemotesNode));
	}

	private void AddChildren(AllDashboardsNode allDashboardsNode)
	{
		AddToDictionaries(new HashSet<object>(AllDashboards), new DescendancyList(allDashboardsNode));
	}

	private void AddChildren(AllObjectSharingNode allObjectSharingNode)
	{
		var descendancy = new DescendancyList(allObjectSharingNode);

		var allExportsNode = new AllObjectExportsNode();
		var allImportsNode = new AllObjectImportsNode();

		AddToDictionaries(new HashSet<object>(AllExports), descendancy.Add(allExportsNode));
		AddToDictionaries(new HashSet<object>(AllImports), descendancy.Add(allImportsNode));

		AddToDictionaries(new HashSet<object>(new object[] { allExportsNode, allImportsNode }), descendancy);
	}


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

			foreach (var pipeline in AddChildren(node, descendancy.Add(node)))
				unknownPipelines.Remove(pipeline);

			children.Add(node);
		}

		children.Add(OtherPipelinesNode);
		OtherPipelinesNode.Pipelines.AddRange(unknownPipelines.Cast<Pipeline>());
		AddToDictionaries(unknownPipelines, descendancy.Add(OtherPipelinesNode));

		//it is the first standard use case
		AddToDictionaries(children, descendancy);
	}

	private IEnumerable<Pipeline> AddChildren(StandardPipelineUseCaseNode node, DescendancyList descendancy)
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

			AddChildren(useCaseNode, descendancy.Add(useCaseNode));

			node.Pipelines.Add(compatiblePipeline);
			children.Add(useCaseNode);
		}

		//it is the first standard use case
		AddToDictionaries(children, descendancy);

		return children.Cast<PipelineCompatibleWithUseCaseNode>().Select(u => u.Pipeline);
	}

	private void AddChildren(PipelineCompatibleWithUseCaseNode pipelineNode, DescendancyList descendancy)
	{
		var components = AllPipelineComponents.Where(c => c.Pipeline_ID == pipelineNode.Pipeline.ID)
			.OrderBy(o => o.Order)
			.ToArray();

		foreach (var component in components)
			AddChildren(component, descendancy.Add(component));

		var children = new HashSet<object>(components);

		AddToDictionaries(children, descendancy);
	}

	private void AddChildren(PipelineComponent pipelineComponent, DescendancyList descendancy)
	{
		var components = AllPipelineComponentsArguments.Where(c => c.PipelineComponent_ID == pipelineComponent.ID)
			.ToArray();

		var children = new HashSet<object>(components);

		AddToDictionaries(children, descendancy);
	}

	private void BuildServerNodes()
	{
		//add a root node for all the servers to be children of
		AllServersNode = new AllServersNode();

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
				AddChildren(server, descendancy.Add(server));
			}
		}

		//create the server nodes
		AllServers = allServers.ToArray();

		//record the fact that all the servers are children of the all servers node
		AddToDictionaries(new HashSet<object>(AllServers), descendancy);
	}


	private void AddChildren(AllDataAccessCredentialsNode allDataAccessCredentialsNode)
	{
		var children = new HashSet<object>();

		var isKeyMissing = false;
		if (_catalogueRepository.EncryptionManager is PasswordEncryptionKeyLocation keyLocation)
			isKeyMissing = string.IsNullOrWhiteSpace(keyLocation.GetKeyFileLocation());

		children.Add(new DecryptionPrivateKeyNode(isKeyMissing));

		foreach (var creds in AllDataAccessCredentials)
			children.Add(creds);


		AddToDictionaries(children, new DescendancyList(allDataAccessCredentialsNode));
	}

	private void AddChildren(AllANOTablesNode anoTablesNode)
	{
		AddToDictionaries(new HashSet<object>(AllANOTables), new DescendancyList(anoTablesNode));
	}

	private void AddChildren(FolderNode<Catalogue> folder, DescendancyList descendancy)
	{
		foreach (var child in folder.ChildFolders)
			//add subfolder children
			AddChildren(child, descendancy.Add(child));

		//add catalogues in folder
		foreach (var c in folder.ChildObjects) AddChildren(c, descendancy.Add(c));

		// Children are the folders + objects
		AddToDictionaries(new HashSet<object>(
				folder.ChildFolders.Cast<object>()
					.Union(folder.ChildObjects)), descendancy
		);
	}

	private void AddChildren(FolderNode<LoadMetadata> folder, DescendancyList descendancy)
	{
		foreach (var child in folder.ChildFolders)
			//add subfolder children
			AddChildren(child, descendancy.Add(child));

		//add loads in folder
		foreach (var lmd in folder.ChildObjects.Where(lmd => lmd.RootLoadMetadata_ID == null).ToArray()) AddChildren(lmd, descendancy.Add(lmd));
		// Children are the folders + objects
		AddToDictionaries(new HashSet<object>(
				folder.ChildFolders.Cast<object>()
					.Union(folder.ChildObjects)), descendancy
		);
	}

	private void AddChildren(FolderNode<Curation.Data.Dataset> folder, DescendancyList descendancy)
	{
		foreach (var child in folder.ChildFolders)
			//add subfolder children
			AddChildren(child, descendancy.Add(child));

		//add loads in folder
		foreach (var ds in folder.ChildObjects) AddChildren(ds, descendancy.Add(ds));

		// Children are the folders + objects
		AddToDictionaries(new HashSet<object>(
				folder.ChildFolders.Cast<object>()
					.Union(folder.ChildObjects)), descendancy
		);
	}

	private void AddChildren(FolderNode<CohortIdentificationConfiguration> folder, DescendancyList descendancy)
	{
		foreach (var child in folder.ChildFolders)
			//add subfolder children
			AddChildren(child, descendancy.Add(child));


		//add cics in folder
		foreach (var cic in folder.ChildObjects) AddChildren(cic, descendancy.Add(cic));

		// Children are the folders + objects
		AddToDictionaries(new HashSet<object>(
				folder.ChildFolders.Cast<object>()
					.Union(folder.ChildObjects)), descendancy
		);
	}

	private void AddChildren(Curation.Data.Dataset lmd, DescendancyList descendancy)
	{
		var childObjects = new List<object>();
		AddToDictionaries(new HashSet<object>(childObjects), descendancy);
	}


	#region Load Metadata

	private void AddChildren(LoadMetadata lmd, DescendancyList descendancy, bool includeSchedule = true, bool includeCatalogues = true, bool includeVersions = true)
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
			AddChildren(allSchedulesNode, descendancy.Add(allSchedulesNode));
			childObjects.Add(allSchedulesNode);
		}

		if (includeCatalogues)
		{
			var allCataloguesNode = new AllCataloguesUsedByLoadMetadataNode(lmd);
			AddChildren(allCataloguesNode, descendancy.Add(allCataloguesNode));
			childObjects.Add(allCataloguesNode);
		}

		var processTasksNode = new AllProcessTasksUsedByLoadMetadataNode(lmd);
		AddChildren(processTasksNode, descendancy.Add(processTasksNode));
		childObjects.Add(processTasksNode);

		if (includeVersions)
		{
			var versionsNode = new LoadMetadataVersionNode(lmd);
			AddChildren(versionsNode, descendancy.Add(versionsNode));
			childObjects.Add(versionsNode);
		}

		childObjects.Add(new LoadDirectoryNode(lmd));

		AddToDictionaries(new HashSet<object>(childObjects), descendancy);
	}

	private void AddChildren(LoadMetadataScheduleNode allSchedulesNode, DescendancyList descendancy)
	{
		var childObjects = new HashSet<object>();

		var lmd = allSchedulesNode.LoadMetadata;

		foreach (var lp in AllLoadProgresses.Where(p => p.LoadMetadata_ID == lmd.ID))
		{
			AddChildren(lp, descendancy.Add(lp));
			childObjects.Add(lp);
		}

		if (childObjects.Any())
			AddToDictionaries(childObjects, descendancy);
	}

	private void AddChildren(LoadProgress loadProgress, DescendancyList descendancy)
	{
		var cacheProgresses = AllCacheProgresses.Where(cp => cp.LoadProgress_ID == loadProgress.ID).ToArray();

		foreach (var cacheProgress in cacheProgresses)
			AddChildren(cacheProgress, descendancy.Add(cacheProgress));

		if (cacheProgresses.Any())
			AddToDictionaries(new HashSet<object>(cacheProgresses), descendancy);
	}

	private void AddChildren(CacheProgress cacheProgress, DescendancyList descendancy)
	{
		var children = new HashSet<object>();

		if (cacheProgress.PermissionWindow_ID != null)
		{
			var window = AllPermissionWindows.Single(w => w.ID == cacheProgress.PermissionWindow_ID);
			var windowNode = new PermissionWindowUsedByCacheProgressNode(cacheProgress, window, true);

			children.Add(windowNode);
		}

		if (children.Any())
			AddToDictionaries(children, descendancy);
	}

	private void AddChildren(AllProcessTasksUsedByLoadMetadataNode allProcessTasksUsedByLoadMetadataNode,
		DescendancyList descendancy)
	{
		var childObjects = new HashSet<object>();

		var lmd = allProcessTasksUsedByLoadMetadataNode.LoadMetadata;
		childObjects.Add(new LoadStageNode(lmd, LoadStage.GetFiles));
		childObjects.Add(new LoadStageNode(lmd, LoadStage.Mounting));
		childObjects.Add(new LoadStageNode(lmd, LoadStage.AdjustRaw));
		childObjects.Add(new LoadStageNode(lmd, LoadStage.AdjustStaging));
		childObjects.Add(new LoadStageNode(lmd, LoadStage.PostLoad));

		foreach (LoadStageNode node in childObjects)
			AddChildren(node, descendancy.Add(node));

		AddToDictionaries(childObjects, descendancy);
	}

	private void AddChildren(LoadStageNode loadStageNode, DescendancyList descendancy)
	{
		var tasks = AllProcessTasks.Where(
			   p => p.LoadMetadata_ID == loadStageNode.LoadMetadata.ID && p.LoadStage == loadStageNode.LoadStage)
		   .OrderBy(o => o.Order).ToArray();

		foreach (var processTask in tasks)
			AddChildren(processTask, descendancy.Add(processTask));

		if (tasks.Any())
			AddToDictionaries(new HashSet<object>(tasks), descendancy);
	}

	private void AddChildren(ProcessTask procesTask, DescendancyList descendancy)
	{
		var args = AllProcessTasksArguments.Where(
			a => a.ProcessTask_ID == procesTask.ID).ToArray();

		if (args.Any())
			AddToDictionaries(new HashSet<object>(args), descendancy);
	}

	private void AddChildren(LoadMetadataVersionNode LoadMetadataVersionNode, DescendancyList descendancy)
	{
		LoadMetadataVersionNode.LoadMetadataVersions = AllLoadMetadatas.Where(lmd => lmd.RootLoadMetadata_ID == LoadMetadataVersionNode.LoadMetadata.ID).ToList();
		var childObjects = new List<object>();

		foreach (var lmd in LoadMetadataVersionNode.LoadMetadataVersions)
		{
			AddChildren(lmd, descendancy.Add(lmd), false, false, false);
			childObjects.Add(lmd);
		}
		AddToDictionaries(new HashSet<object>(childObjects), descendancy);

	}

	private void AddChildren(AllCataloguesUsedByLoadMetadataNode allCataloguesUsedByLoadMetadataNode,
		DescendancyList descendancy)
	{
		var loadMetadataId = allCataloguesUsedByLoadMetadataNode.LoadMetadata.ID;
		var linkedCatalogueIDs = AllLoadMetadataLinkage.Where(link => link.LoadMetadataID == loadMetadataId).Select(static link => link.CatalogueID);
		var usedCatalogues = linkedCatalogueIDs.Select(catalogueId => AllCatalogues.FirstOrDefault(c => c.ID == catalogueId)).Where(static foundCatalogue => foundCatalogue is not null).ToList();
		allCataloguesUsedByLoadMetadataNode.UsedCatalogues = usedCatalogues;
		var childObjects = usedCatalogues.Select(foundCatalogue => new CatalogueUsedByLoadMetadataNode(allCataloguesUsedByLoadMetadataNode.LoadMetadata, foundCatalogue)).Cast<object>().ToHashSet();

		AddToDictionaries(childObjects, descendancy);
	}

	#endregion

	protected void AddChildren(Catalogue c, DescendancyList descendancy)
	{
		var childObjects = new List<object>();

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

		AddChildren(core, descendancy.Add(core));
		childObjects.Add(core);

		foreach (var optional in new[] { deprecated, special, intern, supplemental, notExtractable })
			if (optional.CatalogueItems.Any())
			{
				AddChildren(optional, descendancy.Add(optional));
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

			//record the children
			AddToDictionaries(new HashSet<object>(docs.Cast<object>().Union(sql)), descendancy.Add(documentationNode));
		}

		if (lookups.Any())
		{
			var lookupsNode = new CatalogueLookupsNode(c, lookups);
			//add the documentations node
			childObjects.Add(lookupsNode);


			//record the children
			AddToDictionaries(new HashSet<object>(lookups.Select(l => new CatalogueLookupUsageNode(c, l))),
				descendancy.Add(lookupsNode));
		}

		if (regularAggregates.Any())
		{
			var aggregatesNode = new AggregatesNode(c, regularAggregates);
			childObjects.Add(aggregatesNode);

			var nodeDescendancy = descendancy.Add(aggregatesNode);
			AddToDictionaries(new HashSet<object>(regularAggregates), nodeDescendancy);

			foreach (var regularAggregate in regularAggregates)
				AddChildren(regularAggregate, nodeDescendancy.Add(regularAggregate));
		}

		//finalise
		AddToDictionaries(new HashSet<object>(childObjects), descendancy);
	}

	private void InjectCatalogueItems()
	{
		foreach (var ci in AllCatalogueItems)
			if (_extractionInformationsByCatalogueItem.TryGetValue(ci.ID, out var ei))
				ci.InjectKnown(ei);
			else
				ci.InjectKnown((ExtractionInformation)null);
	}

	private void AddChildren(CatalogueItemsNode node, DescendancyList descendancyList)
	{
		AddToDictionaries(new HashSet<object>(node.CatalogueItems), descendancyList);

		foreach (var ci in node.CatalogueItems)
			AddChildren(ci, descendancyList.Add(ci));
	}

	private void AddChildren(AggregateConfiguration aggregateConfiguration, DescendancyList descendancy)
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

			AddChildren(container, descendancy.Add(container));
			childrenObjects.Add(container);
		}

		AddToDictionaries(childrenObjects, descendancy);
	}

	private void AddChildren(AggregateFilterContainer container, DescendancyList descendancy)
	{
		var childrenObjects = new List<object>();

		var subcontainers = _aggregateFilterManager.GetSubContainers(container);
		var filters = _aggregateFilterManager.GetFilters(container);

		foreach (AggregateFilterContainer subcontainer in subcontainers)
		{
			//one of our children is this subcontainer
			childrenObjects.Add(subcontainer);

			//but also document its children
			AddChildren(subcontainer, descendancy.Add(subcontainer));
		}

		//also add the filters for the container
		foreach (var f in filters)
		{
			// for filters add the parameters under them
			AddChildren((AggregateFilter)f, descendancy.Add(f));
			childrenObjects.Add(f);
		}

		//add our children to the dictionary
		AddToDictionaries(new HashSet<object>(childrenObjects), descendancy);
	}

	private void AddChildren(AggregateFilter f, DescendancyList descendancy)
	{
		AddToDictionaries(new HashSet<object>(AllAggregateFilterParameters.Where(p => p.AggregateFilter_ID == f.ID)),
			descendancy);
	}

	private void AddChildren(CatalogueItem ci, DescendancyList descendancy)
	{
		var childObjects = new List<object>();

		var ei = ci.ExtractionInformation;
		if (ei != null)
		{
			childObjects.Add(ei);
			AddChildren(ei, descendancy.Add(ei));
		}
		else
		{
			ci.InjectKnown(
				(ExtractionInformation)null); // we know the CatalogueItem has no ExtractionInformation child because it's not in the dictionary
		}

		if (ci.ColumnInfo_ID.HasValue && _allColumnInfos.TryGetValue(ci.ColumnInfo_ID.Value, out var col))
			childObjects.Add(new LinkedColumnInfoNode(ci, col));

		AddToDictionaries(new HashSet<object>(childObjects), descendancy);
	}

	private void AddChildren(ExtractionInformation extractionInformation, DescendancyList descendancy)
	{
		var children = new HashSet<object>();

		foreach (var filter in AllCatalogueFilters.Where(f => f.ExtractionInformation_ID == extractionInformation.ID))
		{
			//add the filter as a child of the
			children.Add(filter);
			AddChildren(filter, descendancy.Add(filter));
		}

		AddToDictionaries(children, descendancy);
	}

	private void AddChildren(ExtractionFilter filter, DescendancyList descendancy)
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
			AddChildren(set, descendancy.Add(set), parameters);
		}

		if (children.Any())
			AddToDictionaries(children, descendancy);
	}

	private void AddChildren(ExtractionFilterParameterSet set, DescendancyList descendancy,
		ExtractionFilterParameter[] filterParameters)
	{
		var children = new HashSet<object>();

		foreach (var setValue in AllCatalogueValueSetValues.Where(v => v.ExtractionFilterParameterSet_ID == set.ID))
		{
			setValue.InjectKnown(filterParameters.SingleOrDefault(p => p.ID == setValue.ExtractionFilterParameter_ID));
			children.Add(setValue);
		}

		AddToDictionaries(children, descendancy);
	}

	private void AddChildren(CohortIdentificationConfiguration cic, DescendancyList descendancy)
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
			AddChildren(container, descendancy.Add(container).SetBetterRouteExists());
			children.Add(container);
		}

		//get the patient index tables
		var joinableNode = new JoinableCollectionNode(cic,
			AllJoinables.Where(j => j.CohortIdentificationConfiguration_ID == cic.ID).ToArray());
		AddChildren(joinableNode, descendancy.Add(joinableNode).SetBetterRouteExists());
		children.Add(joinableNode);

		AddToDictionaries(children, descendancy.SetBetterRouteExists());
	}

	private void AddChildren(JoinableCollectionNode joinablesNode, DescendancyList descendancy)
	{
		var children = new HashSet<object>();

		foreach (var joinable in joinablesNode.Joinables)
			try
			{
				var agg = AllAggregateConfigurations.Single(ac => ac.ID == joinable.AggregateConfiguration_ID);
				ForceAggregateNaming(agg, descendancy);
				children.Add(agg);

				//it's no longer an orphan because it's in a known cic (as a patient index table)
				OrphanAggregateConfigurations.Remove(agg);

				AddChildren(agg, descendancy.Add(agg));
			}
			catch (Exception e)
			{
				throw new Exception(
					$"JoinableCohortAggregateConfiguration (patient index table) object (ID={joinable.ID}) references AggregateConfiguration_ID {joinable.AggregateConfiguration_ID} but that AggregateConfiguration was not found",
					e);
			}

		AddToDictionaries(children, descendancy);
	}

	private void AddChildren(CohortAggregateContainer container, DescendancyList descendancy)
	{
		//get subcontainers
		var subcontainers = _cohortContainerManager.GetChildren(container).OfType<CohortAggregateContainer>().ToList();

		//if there are subcontainers
		foreach (var subcontainer in subcontainers)
			AddChildren(subcontainer, descendancy.Add(subcontainer));

		//get our configurations
		var configurations = _cohortContainerManager.GetChildren(container).OfType<AggregateConfiguration>().ToList();

		//record the configurations children including full descendancy
		foreach (var configuration in configurations)
		{
			ForceAggregateNaming(configuration, descendancy);
			AddChildren(configuration, descendancy.Add(configuration));

			//it's no longer an orphan because it's in a known cic
			OrphanAggregateConfigurations.Remove(configuration);
		}

		//all our children (containers and aggregates)
		//children are all aggregates and containers at the current hierarchy level in order
		var children = subcontainers.Union(configurations.Cast<IOrderable>()).OrderBy(o => o.Order).ToList();

		AddToDictionaries(new HashSet<object>(children), descendancy);
	}

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

	private void AddChildren(TableInfoServerNode serverNode, DescendancyList descendancy)
	{
		//add empty hashset
		var children = new HashSet<object>();

		var databases =
			serverNode.Tables.GroupBy(
					k => k.Database ?? TableInfoDatabaseNode.NullDatabaseNode, StringComparer.CurrentCultureIgnoreCase)
				.Select(g => new TableInfoDatabaseNode(g.Key, serverNode, g));

		foreach (var db in databases)
		{
			children.Add(db);
			AddChildren(db, descendancy.Add(db));
		}

		//now we have recorded all the children add them with descendancy
		AddToDictionaries(children, descendancy);
	}

	private void AddChildren(TableInfoDatabaseNode dbNode, DescendancyList descendancy)
	{
		//add empty hashset
		var children = new HashSet<object>();

		foreach (var t in dbNode.Tables)
		{
			//record the children of the table infos (mostly column infos)
			children.Add(t);

			//the all servers node=>the TableInfoServerNode => the t
			AddChildren(t, descendancy.Add(t));
		}

		//now we have recorded all the children add them with descendancy
		AddToDictionaries(children, descendancy);
	}

	private void AddChildren(TableInfo tableInfo, DescendancyList descendancy)
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
			AddToDictionaries(discardedCols, descendancy.Add(identifierDumpNode));
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
				children.Add(c);
				c.InjectKnown(tableInfo);
				AddChildren(c, descendancy.Add(c).SetBetterRouteExists());
			}

		//finally add any credentials objects
		if (AllDataAccessCredentialUsages.TryGetValue(tableInfo, out var nodes))
			foreach (var node in nodes)
				children.Add(node);

		//now we have recorded all the children add them with descendancy via the TableInfo descendancy
		AddToDictionaries(children, descendancy);
	}

	private void AddChildren(ColumnInfo columnInfo, DescendancyList descendancy)
	{
		var lookups = AllLookups.Where(l => l.Description_ID == columnInfo.ID).ToArray();
		var joinInfos = AllJoinInfos.Where(j => j.PrimaryKey_ID == columnInfo.ID);

		var children = new HashSet<object>();

		foreach (var l in lookups)
			children.Add(l);

		foreach (var j in joinInfos)
			children.Add(j);

		if (children.Any())
			AddToDictionaries(children, descendancy);
	}

	protected void AddToDictionaries(HashSet<object> children, DescendancyList list)
	{
		if (list.IsEmpty)
			throw new ArgumentException("DescendancyList cannot be empty", nameof(list));

		//document that the last parent has these as children
		var parent = list.Last();

		_childDictionary.AddOrUpdate(parent,
			children, (p, s) => children);

		//now document the entire parent order to reach each child object i.e. 'Root=>Grandparent=>Parent'  is how you get to 'Child'
		foreach (var o in children)
			_descendancyDictionary.AddOrUpdate(o, list, (k, v) => HandleDescendancyCollision(k, v, list));


		foreach (var masquerader in children.OfType<IMasqueradeAs>())
		{
			var key = masquerader.MasqueradingAs();

			if (!AllMasqueraders.ContainsKey(key))
				AllMasqueraders.AddOrUpdate(key, new HashSet<IMasqueradeAs>(), (o, set) => set);

			lock (AllMasqueraders)
			{
				AllMasqueraders[key].Add(masquerader);
			}
		}
	}

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

	private HashSet<object> GetAllObjects()
	{
		//anything which has children or is a child of someone else (distinct because HashSet)
		return new HashSet<object>(_childDictionary.SelectMany(kvp => kvp.Value).Union(_childDictionary.Keys));
	}

	public virtual object[] GetChildren(object model)
	{
		lock (WriteLock)
		{
			//if we have a record of any children in the child dictionary for the parent model object
			if (_childDictionary.TryGetValue(model, out var cached))
				return cached.OrderBy(static o => o.ToString()).ToArray();

			return model switch
			{
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
		lock (WriteLock)
		{
			return _descendancyDictionary.GetValueOrDefault(model);
		}
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

			foreach (var kvp in _descendancyDictionary.Where(kvp => kvp.Key is IMapsDirectlyToDatabaseTable))
				toReturn.Add((IMapsDirectlyToDatabaseTable)kvp.Key, kvp.Value);

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
		lock (WriteLock)
		{
			var newObjectsFound = new HashSet<object>();

			var sw = new Stopwatch();

			var providers = _pluginChildProviders.Except(_blockedPlugins).ToArray();

			//for every object found so far
			if (providers.Any())
				foreach (var o in objectsToAskAbout ?? GetAllObjects())
					//for every plugin loaded (that is not forbidlisted)
					foreach (var plugin in providers)
						//ask about the children
						try
						{
							sw.Restart();
							//otherwise ask plugin what its children are
							var pluginChildren = plugin.GetChildren(o);

							//if the plugin takes too long to respond we need to stop
							if (sw.ElapsedMilliseconds > 1000)
							{
								_blockedPlugins.Add(plugin);
								throw new Exception(
									$"Plugin '{plugin}' was forbidlisted for taking too long to respond to GetChildren(o) where o was a '{o.GetType().Name}' ('{o}')");
							}

							//it has children
							if (pluginChildren != null && pluginChildren.Any())
							{
								//get the descendancy of the parent
								var parentDescendancy = GetDescendancyListIfAnyFor(o);
								var newDescendancy = parentDescendancy == null
									? new DescendancyList(new[] { o })
									: //if the parent is a root level object start a new descendancy list from it
									parentDescendancy
										.Add(o); //otherwise keep going down, returns a new DescendancyList so doesn't corrupt the dictionary one
								newDescendancy =
									parentDescendancy
										.Add(o); //otherwise keep going down, returns a new DescendancyList so doesn't corrupt the dictionary one

								//record that
								foreach (var pluginChild in pluginChildren)
								{
									//if the parent didn't have any children before
									if (!_childDictionary.ContainsKey(o))
										_childDictionary.AddOrUpdate(o, new HashSet<object>(),
											(o1, set) => set); //it does now


									//add us to the parent objects child collection
									_childDictionary[o].Add(pluginChild);

									//add to the child collection of the parent object kvp.Key
									_descendancyDictionary.AddOrUpdate(pluginChild, newDescendancy,
										(s, e) => newDescendancy);

									//we have found a new object so we must ask other plugins about it (chances are a plugin will have a whole tree of sub objects)
									newObjectsFound.Add(pluginChild);
								}
							}
						}
						catch (Exception e)
						{
							_errorsCheckNotifier.OnCheckPerformed(new CheckEventArgs(e.Message, CheckResult.Fail, e));
						}

			if (newObjectsFound.Any())
				GetPluginChildren(newObjectsFound);
		}
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

	public virtual void UpdateTo(ICoreChildProvider other)
	{
		ArgumentNullException.ThrowIfNull(other);

		if (other is not CatalogueChildProvider otherCat)
			throw new NotSupportedException(
				$"Did not know how to UpdateTo ICoreChildProvider of type {other.GetType().Name}");

		AllLoadMetadatas = otherCat.AllLoadMetadatas;
		AllProcessTasks = otherCat.AllProcessTasks;
		AllProcessTasksArguments = otherCat.AllProcessTasksArguments;
		AllLoadProgresses = otherCat.AllLoadProgresses;
		AllCacheProgresses = otherCat.AllCacheProgresses;
		AllPermissionWindows = otherCat.AllPermissionWindows;
		AllCatalogues = otherCat.AllCatalogues;
		AllCataloguesDictionary = otherCat.AllCataloguesDictionary;
		AllSupportingDocuments = otherCat.AllSupportingDocuments;
		AllSupportingSQL = otherCat.AllSupportingSQL;
		_childDictionary = otherCat._childDictionary;
		_descendancyDictionary = otherCat._descendancyDictionary;
		_catalogueToCatalogueItems = otherCat._catalogueToCatalogueItems;
		AllCatalogueItemsDictionary = otherCat.AllCatalogueItemsDictionary;
		_allColumnInfos = otherCat._allColumnInfos;
		AllAggregateConfigurations = otherCat.AllAggregateConfigurations;
		AllAggregateDimensions = otherCat.AllAggregateDimensions;
		AllAggregateContinuousDateAxis = otherCat.AllAggregateContinuousDateAxis;
		AllRDMPRemotesNode = otherCat.AllRDMPRemotesNode;
		AllRemoteRDMPs = otherCat.AllRemoteRDMPs;
		AllDashboardsNode = otherCat.AllDashboardsNode;
		AllDashboards = otherCat.AllDashboards;
		AllObjectSharingNode = otherCat.AllObjectSharingNode;
		AllImports = otherCat.AllImports;
		AllExports = otherCat.AllExports;
		AllStandardRegexesNode = otherCat.AllStandardRegexesNode;
		AllPipelinesNode = otherCat.AllPipelinesNode;
		OtherPipelinesNode = otherCat.OtherPipelinesNode;
		AllPipelines = otherCat.AllPipelines;
		AllPipelineComponents = otherCat.AllPipelineComponents;
		AllPipelineComponentsArguments = otherCat.AllPipelineComponentsArguments;
		AllStandardRegexes = otherCat.AllStandardRegexes;
		AllANOTablesNode = otherCat.AllANOTablesNode;
		AllANOTables = otherCat.AllANOTables;
		AllExternalServers = otherCat.AllExternalServers;
		AllServers = otherCat.AllServers;
		AllTableInfos = otherCat.AllTableInfos;
		AllDataAccessCredentialsNode = otherCat.AllDataAccessCredentialsNode;
		AllExternalServersNode = otherCat.AllExternalServersNode;
		AllServersNode = otherCat.AllServersNode;
		AllDataAccessCredentials = otherCat.AllDataAccessCredentials;
		AllDataAccessCredentialUsages = otherCat.AllDataAccessCredentialUsages;
		TableInfosToColumnInfos = otherCat.TableInfosToColumnInfos;
		AllColumnInfos = otherCat.AllColumnInfos;
		AllPreLoadDiscardedColumns = otherCat.AllPreLoadDiscardedColumns;
		AllLookups = otherCat.AllLookups;
		AllJoinInfos = otherCat.AllJoinInfos;
		AllAnyTableParameters = otherCat.AllAnyTableParameters;
		AllMasqueraders = otherCat.AllMasqueraders;
		AllExtractionInformationsDictionary = otherCat.AllExtractionInformationsDictionary;
		_pluginChildProviders = otherCat._pluginChildProviders;
		AllPermissionWindowsNode = otherCat.AllPermissionWindowsNode;
		LoadMetadataRootFolder = otherCat.LoadMetadataRootFolder;
		CatalogueRootFolder = otherCat.CatalogueRootFolder;
		CohortIdentificationConfigurationRootFolder = otherCat.CohortIdentificationConfigurationRootFolder;
		AllConnectionStringKeywordsNode = otherCat.AllConnectionStringKeywordsNode;
		AllConnectionStringKeywords = otherCat.AllConnectionStringKeywords;
		AllAggregateContainersDictionary = otherCat.AllAggregateContainersDictionary;
		AllAggregateFilters = otherCat.AllAggregateFilters;
		AllAggregateFilterParameters = otherCat.AllAggregateFilterParameters;
		AllCohortIdentificationConfigurations = otherCat.AllCohortIdentificationConfigurations;
		AllCohortAggregateContainers = otherCat.AllCohortAggregateContainers;
		AllJoinables = otherCat.AllJoinables;
		AllJoinUses = otherCat.AllJoinUses;
		AllGovernanceNode = otherCat.AllGovernanceNode;
		AllGovernancePeriods = otherCat.AllGovernancePeriods;
		AllGovernanceDocuments = otherCat.AllGovernanceDocuments;
		GovernanceCoverage = otherCat.GovernanceCoverage;
		AllJoinableCohortAggregateConfigurationUse = otherCat.AllJoinableCohortAggregateConfigurationUse;
		AllPluginsNode = otherCat.AllPluginsNode;
		PipelineUseCases = otherCat.PipelineUseCases;
		OrphanAggregateConfigurationsNode = otherCat.OrphanAggregateConfigurationsNode;
		TemplateAggregateConfigurationsNode = otherCat.TemplateAggregateConfigurationsNode;
		AllCatalogueParameters = otherCat.AllCatalogueParameters;
		AllCatalogueValueSets = otherCat.AllCatalogueValueSets;
		AllCatalogueValueSetValues = otherCat.AllCatalogueValueSetValues;
		OrphanAggregateConfigurations = otherCat.OrphanAggregateConfigurations;
	}

	public virtual bool SelectiveRefresh(IMapsDirectlyToDatabaseTable databaseEntity)
	{
		ProgressStopwatch.Restart();

		return databaseEntity switch
		{
			AggregateFilterParameter afp => SelectiveRefresh(afp.AggregateFilter),
			AggregateFilter af => SelectiveRefresh(af),
			AggregateFilterContainer afc => SelectiveRefresh(afc),
			CohortAggregateContainer cac => SelectiveRefresh(cac),
			ExtractionInformation ei => SelectiveRefresh(ei),
			CatalogueItem ci => SelectiveRefresh(ci),
			_ => false
		};
	}


	public bool SelectiveRefresh(CatalogueItem ci)
	{
		var descendancy = GetDescendancyListIfAnyFor(ci.Catalogue);
		if (descendancy == null) return false;

		FetchCatalogueItems();
		FetchExtractionInformations();
		AddChildren(ci.Catalogue, descendancy.Add(ci.Catalogue));
		return true;
	}

	public bool SelectiveRefresh(ExtractionInformation ei)
	{
		var descendancy = GetDescendancyListIfAnyFor(ei);
		var cata = descendancy?.Parents.OfType<Catalogue>().LastOrDefault() ?? ei.CatalogueItem.Catalogue;

		if (cata == null)
			return false;

		var cataDescendancy = GetDescendancyListIfAnyFor(cata);

		if (cataDescendancy == null)
			return false;

		FetchCatalogueItems();

		foreach (var ci in AllCatalogueItems.Where(ci => ci.ID == ei.CatalogueItem_ID)) ci.ClearAllInjections();

		// property changes or deleting the ExtractionInformation
		FetchExtractionInformations();

		// refresh the Catalogue
		AddChildren(cata, cataDescendancy.Add(cata));
		return true;
	}

	public bool SelectiveRefresh(CohortAggregateContainer container)
	{
		var parentContainer = container.GetParentContainerIfAny();
		if (parentContainer != null)
		{
			var descendancy = GetDescendancyListIfAnyFor(parentContainer);

			if (descendancy != null)
			{
				BuildAggregateConfigurations();

				BuildCohortCohortAggregateContainers();
				AddChildren(parentContainer, descendancy.Add(parentContainer));
				return true;
			}
		}

		var cic = container.GetCohortIdentificationConfiguration();

		if (cic != null)
		{
			var descendancy = GetDescendancyListIfAnyFor(cic);

			if (descendancy != null)
			{
				BuildAggregateConfigurations();
				BuildCohortCohortAggregateContainers();
				AddChildren(cic, descendancy.Add(cic));
				return true;
			}
		}

		return false;
	}

	public bool SelectiveRefresh(AggregateFilter f)
	{
		var knownContainer = GetDescendancyListIfAnyFor(f.FilterContainer);
		if (knownContainer == null) return false;

		BuildAggregateFilterContainers();
		AddChildren((AggregateFilterContainer)f.FilterContainer, knownContainer.Add(f.FilterContainer));
		return true;
	}

	public bool SelectiveRefresh(AggregateFilterContainer container)
	{
		var aggregate = container.GetAggregate();

		if (aggregate == null) return false;

		var descendancy = GetDescendancyListIfAnyFor(aggregate);

		if (descendancy == null) return false;

		// update just in case we became a root filter for someone
		aggregate.RevertToDatabaseState();

		BuildAggregateFilterContainers();

		AddChildren(aggregate, descendancy.Add(aggregate));
		return true;
	}
}