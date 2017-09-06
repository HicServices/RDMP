using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.PerformanceImprovement;
using CatalogueLibrary.Nodes;
using CatalogueLibrary.Nodes.LoadMetadataNodes;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using Microsoft.SqlServer.Management.Smo;

namespace CatalogueLibrary.Providers
{
    public class CatalogueChildProvider :ICoreChildProvider
    {
        public LoadMetadata[] AllLoadMetadatas { get; set; }
        public ProcessTask[] AllProcessTasks { get; set; }

        public LoadProgress[] AllLoadProgresses { get; set; }
        public CacheProgress[] AllCacheProgresses { get; set; }
        public LoadPeriodically[] AllLoadPeriodicallies { get; set; }

        //Catalogue side of things
        public Catalogue[] AllCatalogues { get; set; }

        public SupportingDocument[] AllSupportingDocuments { get; set; }
        public SupportingSQLTable[] AllSupportingSQL { get; set; }

        //tells you the imediate children of a given node.  Do not add to this directly instead add using AddToDictionaries unless you want the Key to be an 'on the sly' no known descendency child
        private Dictionary<object, HashSet<object>> _childDictionary = new Dictionary<object, HashSet<object>>();
        
        //This is the reverse of _childDictionary in some ways.  _childDictionary tells you the immediate children while
        //this tells you for a given child object what the navigation tree down to get to it is e.g. ascendancy[child] would return [root,grandParent,parent]
        private Dictionary<object, DescendancyList> _descendancyDictionary = new Dictionary<object, DescendancyList>();

        private CatalogueItem[] _allCatalogueItems;
        private Dictionary<int,ColumnInfo> _allColumnInfos;
        public AggregateConfiguration[] AllAggregateConfigurations { get; private set; }
        public Dictionary<int, CatalogueItemClassification> CatalogueItemClassifications { get; private set; }

        private CatalogueItemIssue[] _allCatalogueItemIssues;

        //TableInfo side of things
        public ANOTablesNode ANOTablesNode { get; private set; }
        public ANOTable[] AllANOTables { get; set; }

        public ExternalDatabaseServer[] AllExternalServers { get; private set; }
        public TableInfoServerNode[] AllServers { get;private set; }
        public TableInfo[] AllTableInfos { get; private set; }

        public DataAccessCredentialsNode DataAccessCredentialsNode { get; set; }

        public AllExternalServersNode AllExternalServersNode { get; private set; }
        public AllServersNode AllServersNode { get; private set; }

        public DataAccessCredentials[] AllDataAccessCredentials { get; set; }
        public Dictionary<TableInfo, List<DataAccessCredentialUsageNode>> AllDataAccessCredentialUsages { get; set; }

        public ColumnInfo[] AllColumnInfos { get; private set; }

        public Lookup[] AllLookups { get; set; }

        public JoinInfo[] AllJoinInfos { get; set; }

        //Filter / extraction side of things

        private Dictionary<int,ExtractionInformation> _allExtractionInformations;

        private readonly CatalogueFilterHierarchy _filterChildProvider;

        private readonly CohortHierarchy _cohortContainerChildProvider;

        public List<Exception> Exceptions { get; private set; }

        public CohortIdentificationConfiguration[] AllCohortIdentificationConfigurations { get; private set; }

        public readonly IChildProvider[] PluginChildProviders;

        public CatalogueChildProvider(CatalogueRepository repository, IChildProvider[] pluginChildProviders)
        {
            PluginChildProviders = pluginChildProviders;

            Exceptions = new List<Exception>();
            AllANOTables = repository.GetAllObjects<ANOTable>();
            ANOTablesNode = new ANOTablesNode();
            AddChildren(ANOTablesNode);
            
            AllCatalogues = repository.GetAllObjects<Catalogue>();
            AllLoadMetadatas = repository.GetAllObjects<LoadMetadata>();
            AllProcessTasks = repository.GetAllObjects<ProcessTask>();
            AllLoadProgresses = repository.GetAllObjects<LoadProgress>();
            AllCacheProgresses = repository.GetAllObjects<CacheProgress>();
            AllLoadPeriodicallies = repository.GetAllObjects<LoadPeriodically>();

            AllExternalServers = repository.GetAllObjects<ExternalDatabaseServer>();

            AllTableInfos = repository.GetAllObjects<TableInfo>();
            AllDataAccessCredentials = repository.GetAllObjects<DataAccessCredentials>();
            DataAccessCredentialsNode = new DataAccessCredentialsNode();
            AddChildren(DataAccessCredentialsNode);
            
            //which TableInfos use which Credentials under which DataAccessContexts
            AllDataAccessCredentialUsages = repository.TableInfoToCredentialsLinker.GetAllCredentialUsagesBy(AllDataAccessCredentials, AllTableInfos);
            
            AllColumnInfos = repository.GetAllObjects<ColumnInfo>();

            AllSupportingDocuments = repository.GetAllObjects<SupportingDocument>();
            AllSupportingSQL = repository.GetAllObjects<SupportingSQLTable>();

            AllCohortIdentificationConfigurations = repository.GetAllObjects<CohortIdentificationConfiguration>();
            
            _allCatalogueItems = repository.GetAllObjects<CatalogueItem>();
            _allColumnInfos = repository.GetAllObjects<ColumnInfo>().ToDictionary(i=>i.ID,o=>o);
            _allExtractionInformations = repository.GetAllObjects<ExtractionInformation>().ToDictionary(i => i.ID, o => o);

            _allCatalogueItemIssues = repository.GetAllObjects<CatalogueItemIssue>();

            AllAggregateConfigurations = repository.GetAllObjects<AggregateConfiguration>();

            CatalogueItemClassifications = repository.ClassifyAllCatalogueItems();

            _filterChildProvider = new CatalogueFilterHierarchy(repository);
            _cohortContainerChildProvider = new CohortHierarchy(repository,this);

            AllLookups = repository.GetAllObjects<Lookup>();

            foreach (Lookup l in AllLookups)
                l.SetKnownColumns(_allColumnInfos[l.PrimaryKey_ID], _allColumnInfos[l.ForeignKey_ID],_allColumnInfos[l.Description_ID]);

            AllJoinInfos = repository.JoinInfoFinder.GetAllJoinInfos();

            foreach (JoinInfo j in AllJoinInfos)
                j.SetKnownColumns(_allColumnInfos[j.PrimaryKey_ID], _allColumnInfos[j.ForeignKey_ID]);

            AllExternalServersNode = new AllExternalServersNode();
            AddChildren(AllExternalServersNode);

            //All the things for TableInfoCollectionUI
            BuildServerNodes();

            AddChildren(CatalogueFolder.Root,new DescendancyList(CatalogueFolder.Root));

            foreach (LoadMetadata lmd in AllLoadMetadatas)
                AddChildren(lmd);

            foreach (CohortIdentificationConfiguration cic in AllCohortIdentificationConfigurations)
                AddChildren(cic);
        }

        private void AddChildren(AllExternalServersNode allExternalServersNode)
        {
            AddToDictionaries(new HashSet<object>(AllExternalServers), new DescendancyList(allExternalServersNode));
        }

        private void BuildServerNodes()
        {
            Dictionary<TableInfoServerNode,List<TableInfo>> allServers = new Dictionary<TableInfoServerNode,List<TableInfo>>();

            //add a root node for all the servers to be children of
            AllServersNode = new AllServersNode();

            //find the unique server names among TableInfos
            foreach (TableInfo t in AllTableInfos)
            {
                //make sure we have the in our dictionary
                if(!allServers.Keys.Any(k=>k.IsSameServer(t)))
                    allServers.Add(new TableInfoServerNode(t.Server,t.DatabaseType),new List<TableInfo>());

                var match = allServers.Single(kvp => kvp.Key.IsSameServer(t));
                match.Value.Add(t);
            }

            //create the server nodes
            AllServers = allServers.Keys.ToArray();

            //document the children
            foreach (var kvp in allServers)
            {
                var tableInfos = kvp.Value;

                //record the fact that the TableInfos are children of their specific TableInfoServerNode
                AddToDictionaries(new HashSet<object>(tableInfos), new DescendancyList(AllServersNode, kvp.Key));
                
                //record the children of the table infos (mostly column infos)
                foreach (var t in tableInfos)
                    AddChildren(t, 
                        
                        //t descends from :
                        //the all servers node=>the TableInfoServerNode => the t
                        new DescendancyList(AllServersNode, kvp.Key, t));
            }

            //record the fact that all the servers are children of the all servers node
            AddToDictionaries(new HashSet<object>(AllServers),new DescendancyList(AllServersNode));
        }


        private void AddChildren(DataAccessCredentialsNode dataAccessCredentialsNode)
        {
            AddToDictionaries(new HashSet<object>(AllDataAccessCredentials), new DescendancyList(dataAccessCredentialsNode));
        }

        private void AddChildren(ANOTablesNode anoTablesNode)
        {
            AddToDictionaries(new HashSet<object>(AllANOTables), new DescendancyList(anoTablesNode));
        }

        private void AddChildren(CatalogueFolder folder, DescendancyList descendancy)
        {
            List<object> childObjects = new List<object>();

            
            //add subfolders
            foreach (CatalogueFolder f in folder.GetImmediateSubFoldersUsing(AllCatalogues))
            {
                childObjects.Add(f);
                AddChildren(f,descendancy.Add(f));
            }
            
            //add catalogues in folder
            foreach (Catalogue c in AllCatalogues.Where(c => c.Folder.Equals(folder)))
            {
                AddChildren(c,descendancy.Add(c));
                childObjects.Add(c);
            }

            //finalise
            AddToDictionaries(new HashSet<object>(childObjects),descendancy );
            
        }

        #region Load Metadata
        private void AddChildren(LoadMetadata lmd)
        {
            var descendancy = new DescendancyList(lmd);
            List<object> childObjects = new List<object>();

            var allSchedulesNode = new LoadMetadataScheduleNode(lmd);
            AddChildren(allSchedulesNode,descendancy.Add(allSchedulesNode));
            childObjects.Add(allSchedulesNode);

            var allCataloguesNode = new AllCataloguesUsedByLoadMetadataNode(lmd);
            AddChildren(allCataloguesNode, descendancy.Add(allCataloguesNode));
            childObjects.Add(allCataloguesNode);

            var processTasksNode = new AllProcessTasksUsedByLoadMetadataNode(lmd);
            AddChildren(processTasksNode, descendancy.Add(processTasksNode));
            childObjects.Add(processTasksNode);

            childObjects.Add(new HICProjectDirectoryNode(lmd));

            AddToDictionaries(new HashSet<object>(childObjects), descendancy);
        }

        private void AddChildren(LoadMetadataScheduleNode allSchedulesNode, DescendancyList descendancy)
        {
            HashSet<object> childObjects = new HashSet<object>();

            var lmd = allSchedulesNode.LoadMetadata;

            foreach (var p in AllLoadPeriodicallies.Where(p => p.LoadMetadata_ID == lmd.ID))
                childObjects.Add(p);

            foreach (var lp in AllLoadProgresses.Where(p => p.LoadMetadata_ID == lmd.ID))
            {
                AddChildren(lp,descendancy.Add(lp));
                childObjects.Add(lp);
            }

            if(childObjects.Any())
                AddToDictionaries(childObjects,descendancy);
        }

        private void AddChildren(LoadProgress loadProgress, DescendancyList descendancy)
        {
            var cacheProgresses = AllCacheProgresses.Where(cp => cp.LoadProgress_ID == loadProgress.ID).ToArray();

            if (cacheProgresses.Any())
                AddToDictionaries(new HashSet<object>(cacheProgresses),descendancy);
        }

        private void AddChildren(AllProcessTasksUsedByLoadMetadataNode allProcessTasksUsedByLoadMetadataNode, DescendancyList descendancy)
        {
            HashSet<object> childObjects = new HashSet<object>();

            var lmd = allProcessTasksUsedByLoadMetadataNode.LoadMetadata;
            childObjects.Add(new LoadStageNode(lmd,LoadStage.GetFiles));
            childObjects.Add(new LoadStageNode(lmd, LoadStage.Mounting));
            childObjects.Add(new LoadStageNode(lmd, LoadStage.AdjustRaw));
            childObjects.Add(new LoadStageNode(lmd, LoadStage.AdjustStaging));
            childObjects.Add(new LoadStageNode(lmd, LoadStage.PostLoad));

            foreach (LoadStageNode node in childObjects)
                AddChildren(node,descendancy.Add(node));

            AddToDictionaries(childObjects,descendancy);
        }

        private void AddChildren(LoadStageNode loadStageNode, DescendancyList descendancy)
        {
            var tasks = AllProcessTasks.Where(
                p => p.LoadMetadata_ID == loadStageNode.LoadMetadata.ID && p.LoadStage == loadStageNode.LoadStage)
                .OrderBy(o=>o.Order).ToArray();

            if(tasks.Any())
                AddToDictionaries(new HashSet<object>(tasks),descendancy);
        }

        private void AddChildren(AllCataloguesUsedByLoadMetadataNode allCataloguesUsedByLoadMetadataNode, DescendancyList descendancy)
        {
            HashSet<object> chilObjects = new HashSet<object>();

            var usedCatalogues = AllCatalogues.Where(c => c.LoadMetadata_ID == allCataloguesUsedByLoadMetadataNode.LoadMetadata.ID);


            foreach (Catalogue catalogue in usedCatalogues)
            {
                chilObjects.Add(new CatalogueUsedByLoadMetadataNode(allCataloguesUsedByLoadMetadataNode.LoadMetadata,catalogue));
                
            }
            

            AddToDictionaries(chilObjects,descendancy);
        }

        #endregion

        private void AddChildren(Catalogue c, DescendancyList descendancy)
        {
            List<object> childObjects = new List<object>();

            var catalogueAggregates = AllAggregateConfigurations.Where(a => a.Catalogue_ID == c.ID).ToArray();
            var cohortAggregates = catalogueAggregates.Where(a => a.IsCohortIdentificationAggregate).ToArray();
            var regularAggregates = catalogueAggregates.Except(cohortAggregates).ToArray();

            var docs = AllSupportingDocuments.Where(d => d.Catalogue_ID == c.ID).ToArray();
            var sql = AllSupportingSQL.Where(d => d.Catalogue_ID == c.ID).ToArray();

            //if there are supporting documents or supporting sql files then add  documentation node
            if (docs.Any() || sql.Any())
            {
                var documentationNode = new DocumentationNode(c, docs, sql);

                //add the documentations node
                childObjects.Add(documentationNode);

                //record the children
                AddToDictionaries(new HashSet<object>(docs.Cast<object>().Union(sql)),descendancy.Add(documentationNode));
            }
            
            if(cohortAggregates.Any())
            {
                //the cohort node is our child
                var cohortNode = new CohortSetsNode(c,cohortAggregates);

                childObjects.Add(cohortNode);

                //we also record all the Aggregates that are cohorts under us - but since these are also under Cohort Aggregates we will ignore it for descendancy purposes
                var nodeDescendancy = descendancy.SetBetterRouteExists().Add(cohortNode);

                AddToDictionaries(new HashSet<object>(cohortAggregates),nodeDescendancy);
                foreach (AggregateConfiguration cohortAggregate in cohortAggregates)
                    AddChildren(cohortAggregate, nodeDescendancy.Add(cohortAggregate));
            }

            if (regularAggregates.Any())
            {
                var aggregatesNode = new AggregatesNode(c, regularAggregates);
                childObjects.Add(aggregatesNode);

                var nodeDescendancy = descendancy.Add(aggregatesNode);
                AddToDictionaries(new HashSet<object>(regularAggregates),nodeDescendancy);

                foreach (AggregateConfiguration regularAggregate in regularAggregates)
                    AddChildren(regularAggregate, nodeDescendancy.Add(regularAggregate));
            }
            
            var cis = _allCatalogueItems
                .Where(ci => ci.Catalogue_ID == c.ID).OrderBy(ci2=>
                    //order them by the Order field in the classification (extraction) where not extractable columns (Order is null) appear afterwards and finally unclassified CatalogueItems (really shouldn't happen) appear last
                    CatalogueItemClassifications.ContainsKey(ci2.ID) ? CatalogueItemClassifications[ci2.ID].Order ?? 99999 : 999999)
                .ToArray();

            //add a new CatalogueItemNode (can be empty)
            var catalogueItemsNode = new CatalogueItemsNode(c, cis);
            childObjects.Add(catalogueItemsNode);

            //if there are at least 1 catalogue items add a recording that the CatalogueItemsNode has these children (otherwise node has no children)
            if (cis.Any())
            {
                var ciNodeDescendancy = descendancy.Add(catalogueItemsNode);
                AddToDictionaries(new HashSet<object>(cis), ciNodeDescendancy);

                foreach (CatalogueItem ci in cis)
                    AddChildren(ci,ciNodeDescendancy.Add(ci));
                
            }
            //finalise
            AddToDictionaries(new HashSet<object>(childObjects),descendancy);
        }

        private void AddChildren(AggregateConfiguration aggregateConfiguration, DescendancyList descendancy)
        {
            //we can step into this twice, once via Catalogue children and once via CohortIdentificationConfiguration children
            //if we get in via Catalogue children then descendancy will be Ignore=true we don't end up emphasising into CatalogueCollectionUI when
            //really user wants to see it in CohortIdentificationCollectionUI
            if(aggregateConfiguration.RootFilterContainer_ID != null)
            {
                var container = _filterChildProvider.AllAggregateContainers[(int) aggregateConfiguration.RootFilterContainer_ID];

                AddChildren(container,descendancy.Add(container));
                AddToDictionaries(new HashSet<object>(new object[]{container}),descendancy);
            }
        }

        private void AddChildren(AggregateFilterContainer container, DescendancyList descendancy)
        {
            List<object> childrenObjects = new List<object>();

            var subcontainers = _filterChildProvider.GetSubcontainers(container);
            var filters = _filterChildProvider.GetFilters(container);

            foreach (AggregateFilterContainer subcontainer in subcontainers)
            {
                //one of our children is this subcontainer
                childrenObjects.Add(subcontainer);

                //but also document its children
                AddChildren(subcontainer,descendancy.Add(subcontainer));
            }

            //also add the filters for the container
            childrenObjects.AddRange(filters);
            
            //add our children to the dictionary
            AddToDictionaries(new HashSet<object>(childrenObjects),descendancy);
        }

        private void AddChildren(CatalogueItem ci, DescendancyList descendancy)
        {
            List<object> childObjects = new List<object>();

            if(CatalogueItemClassifications.ContainsKey(ci.ID))
            {  
                var extractionInformationIfAnyId = CatalogueItemClassifications[ci.ID].ExtractionInformation_ID;

                if(extractionInformationIfAnyId != null)
                {
                    var extractionInformation = _allExtractionInformations[extractionInformationIfAnyId.Value];
                    childObjects.Add(extractionInformation);
                    AddChildren(extractionInformation,descendancy.Add(extractionInformation));
                }
                
                var colInfoIfAny = CatalogueItemClassifications[ci.ID].ColumnInfo_ID;
                
                if(colInfoIfAny != null)
                    childObjects.Add(new LinkedColumnInfoNode(ci,_allColumnInfos[colInfoIfAny.Value]));
            }

            childObjects.AddRange(_allCatalogueItemIssues.Where(i => i.CatalogueItem_ID == ci.ID));

            AddToDictionaries(new HashSet<object>(childObjects),descendancy);
        }

        private void AddChildren(ExtractionInformation extractionInformation, DescendancyList descendancy)
        {
            var children = new HashSet<object>();
            
            foreach (var filter in _filterChildProvider.GetFilters(extractionInformation))
            {
                //add the filter as a child of the 
                children.Add(filter);
                AddChildren(filter,descendancy.Add(filter));
            }

            AddToDictionaries(children,descendancy);
        }

        private void AddChildren(ExtractionFilter filter, DescendancyList descendancy)
        {
            var children = new HashSet<object>();
            var parameters = _filterChildProvider.GetParameters(filter).ToArray();
            var parameterSets = _filterChildProvider.GetValueSets(filter);

            if (parameters.Any())
                children.Add(new ParametersNode(filter, parameters));

            foreach (ExtractionFilterParameterSet set in parameterSets)
                children.Add(set);

            if(children.Any())
                AddToDictionaries(children,descendancy);
        }

        public virtual object[] GetChildren(object model)
        {
            List<object> children = new List<object>();

            foreach (var plugin in PluginChildProviders)
            {
                //this plugin is broken
                if(plugin.Exceptions != null && plugin.Exceptions.Any() )
                    continue;

                //otherwise ask plugin what it's children are
                var pluginChildren = plugin.GetChildren(model);

                //it has children
                if(pluginChildren != null)
                    children.AddRange(pluginChildren);//add them
            }

            //if we don't have a record of any children in the child dictionary for the parent model object
            if(!_childDictionary.ContainsKey(model))
                return children.ToArray();//return the plugin ones only

            //otherwise add the plugin ones to the core ones
            children.AddRange(_childDictionary[model]);

            //no children at all anywhere
            if(!children.Any())
                return new object[0];
            
            return children.ToArray();
        }

        private void AddChildren(CohortIdentificationConfiguration cic)
        {
            HashSet<object> children = new HashSet<object>();

            //if it has a root container
            if (cic.RootCohortAggregateContainer_ID != null)
            {
                var container = _cohortContainerChildProvider.AllContainers.Single(c => c.ID == cic.RootCohortAggregateContainer_ID);
                AddChildren(container,new DescendancyList(cic,container));
                children.Add(container);
            }
            
            //get the patient index tables
            var joinableNode = new JoinableCollectionNode(cic, _cohortContainerChildProvider.AllJoinables.Where(j => j.CohortIdentificationConfiguration_ID == cic.ID).ToArray());
            AddChildren(joinableNode,new DescendancyList(cic,joinableNode));
            children.Add(joinableNode);

            AddToDictionaries(children,new DescendancyList(cic));
        }

        private void AddChildren(JoinableCollectionNode joinablesNode, DescendancyList descendancy)
        {
            HashSet<object> children = new HashSet<object>();

            foreach (var joinable in joinablesNode.Joinables)
            {
                try
                {
                    var agg = AllAggregateConfigurations.Single(ac => ac.ID == joinable.AggregateConfiguration_ID);
                    ForceAggregateNaming(agg,descendancy);
                    children.Add(agg);
                    AddChildren(agg,descendancy.Add(agg));
                }
                catch (Exception e)
                {
                    throw new Exception("JoinableCohortAggregateConfiguration (patient index table) object (ID="+joinable.ID+") references AggregateConfiguration_ID " + joinable.AggregateConfiguration_ID + " but that AggregateConfiguration was not found",e);
                }
            }

            AddToDictionaries(children, descendancy);
        }

        private void AddChildren(CohortAggregateContainer container, DescendancyList descendancy)
        {
            //all our children (containers and aggregates)
            List<IOrderable> children = new List<IOrderable>();

            //get subcontainers
            var subcontainers = _cohortContainerChildProvider.GetSubContainers(container);

            //if there are subcontainers
            foreach (CohortAggregateContainer subcontainer in subcontainers)
                AddChildren(subcontainer,descendancy.Add(subcontainer));

            //get our configurations
            var configurations = _cohortContainerChildProvider.GetAggregateConfigurations(container);

            //record the configurations children including full descendancy
            foreach (AggregateConfiguration configuration in configurations)
            {
                ForceAggregateNaming(configuration, descendancy);
                AddChildren(configuration, descendancy.Add(configuration));
            }

            //children are all aggregates and containers at the current hierarchy level in order
            children = subcontainers.Union(configurations.Cast<IOrderable>()).OrderBy(o => o.Order).ToList();

            AddToDictionaries(new HashSet<object>(children),descendancy);
        }

        private void ForceAggregateNaming(AggregateConfiguration configuration, DescendancyList descendancy)
        {
            //configuration has the wrong name
            if (!configuration.IsCohortIdentificationAggregate)
            {
                Exceptions.Add(new Exception("Had to fix naming of configuration '" + configuration + "' because it didn't start with correct cic prefix"));
                descendancy.Parents.OfType<CohortIdentificationConfiguration>().Single().EnsureNamingConvention(configuration);
                configuration.SaveToDatabase();
            }
        }

        private void AddChildren(TableInfo tableInfo,DescendancyList descendancy)
        {
            //add empty hashset
            var children =  new HashSet<object>();

            //if it is a table valued function
            if (tableInfo.IsTableValuedFunction)
            {
                //that has parameters
                var parameters = tableInfo.GetAllParameters();

                //then add those as a node
                if (parameters.Any())
                    children.Add(new ParametersNode(tableInfo, parameters));
            }

            //next add the column infos
            foreach (ColumnInfo c in AllColumnInfos.Where(ci => ci.TableInfo_ID == tableInfo.ID))
            {
                children.Add(c);
                AddChildren(c,descendancy.Add(c));
            }

            //finally add any credentials objects
            if (AllDataAccessCredentialUsages.ContainsKey(tableInfo))
                foreach (DataAccessCredentialUsageNode node in AllDataAccessCredentialUsages[tableInfo])
                    children.Add(node);

            //now we have recorded all the children add them with descendancy via the TableInfo descendancy
            AddToDictionaries(children,descendancy);
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
                AddToDictionaries(children,descendancy);
        }

        
        protected void AddToDictionaries(HashSet<object> children, DescendancyList list)
        {
            if(list.IsEmpty)
                throw new ArgumentException("DescendancyList cannot be empty","list");
            
            //document that the last parent has these as children
            var parent = list.Last();

            //we have already seen it before
            if(_childDictionary.ContainsKey(parent))
            {
                if (!_childDictionary[parent].SetEquals(children))
                    throw new Exception("Ambiguous children collections for object '" + parent  +"'");
            }
            else
                _childDictionary.Add(parent,children);

            //now document the entire parent order to reach each child object i.e. 'Root=>Grandparent=>Parent'  is how you get to 'Child'
            foreach (object o in children)
            {
                if(_descendancyDictionary.ContainsKey(o))
                    if (_descendancyDictionary[o].BetterRouteExists) //the object o has been seen before with a different route but it is not the preferred one, so throw away old route
                        _descendancyDictionary.Remove(o);
                    else if (list.BetterRouteExists)
                        //the current one is a good route and the replacement is a BetterRouteExists so just throw away the new one
                        return;
                    else
                    {
                        //there was a horrible problem with 
                        Exceptions.Add(new Exception("Could not add '" + o + "' to Ascendancy Tree with parents " + list + " because it is already listed under hierarchy " + _descendancyDictionary[o]));
                        return;
                    }

                _descendancyDictionary.Add(o, list);
            }
        }

        public DescendancyList GetDescendancyListIfAnyFor(object model)
        {
            if (_descendancyDictionary.ContainsKey(model))
                return _descendancyDictionary[model];

            return null;
        }

        public object[] GetAllDescendableObjects()
        {
            return _descendancyDictionary.Keys.ToArray();
        }

        public object[] GetAllSearchables()
        {

            return
                AllLoadMetadatas.Cast<object>().Union(AllCohortIdentificationConfigurations).Union(GetAllDescendableObjects()).ToArray();
        }
    }
}