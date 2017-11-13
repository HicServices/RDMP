using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort.Joinables;
using CatalogueLibrary.FilterImporting;
using CatalogueLibrary.FilterImporting.Construction;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Revertable;

using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;

namespace CatalogueLibrary.Data.Cohort
{
    /// <summary>
    /// Cohort identification is the job identifying which patients fit certain study criteria.  E.g. "I want all patients who have been prescribed Diazepam for the first time after 2000
    /// and who are still alive today".  Every time the data analyst has a new project/cohort to identify he should create a new CohortIdentificationConfiguration, it is the entry point
    /// for cohort generation and includes a high level description of what the cohort requirements are, an optional ticket and is the hanging off point for all the 
    /// RootCohortAggregateContainers (the bit that provides the actual filtering/technical data about how the cohort is identified).
    /// </summary>
    public class CohortIdentificationConfiguration : DatabaseEntity, ICollectSqlParameters,INamed, IHasDependencies,ICustomSearchString
    {
        public const string CICPrefix = "cic_";

        #region Database Properties
        private string _name;
        private string _description;
        private string _ticket;
        private int? _rootCohortAggregateContainerID;
        private int? _queryCachingServerID;
        private bool _frozen;
        private string _frozenBy;
        private DateTime? _frozenDate;

        public string Name
        {
            get { return _name; }
            set { SetField(ref  _name, value); }
        }

        public string Description
        {
            get { return _description; }
            set { SetField(ref  _description, value); }
        }

        public string Ticket
        {
            get { return _ticket; }
            set { SetField(ref  _ticket, value); }
        }

        public int? RootCohortAggregateContainer_ID
        {
            get { return _rootCohortAggregateContainerID; }
            set { SetField(ref  _rootCohortAggregateContainerID, value); }
        }

        public int? QueryCachingServer_ID
        {
            get { return _queryCachingServerID; }
            set { SetField(ref  _queryCachingServerID, value); }
        }

        public bool Frozen
        {
            get { return _frozen; }
            set { SetField(ref  _frozen, value); }
        }

        public string FrozenBy
        {
            get { return _frozenBy; }
            set { SetField(ref  _frozenBy, value); }
        }

        public DateTime? FrozenDate
        {
            get { return _frozenDate; }
            set { SetField(ref  _frozenDate, value); }
        }

        #endregion

        #region Relationships
        [NoMappingToDatabase]
        public CohortAggregateContainer RootCohortAggregateContainer {
            get { return 
                RootCohortAggregateContainer_ID == null?  null:
                Repository.GetObjectByID<CohortAggregateContainer>((int) RootCohortAggregateContainer_ID); }
        }

        [NoMappingToDatabase]
        public ExternalDatabaseServer QueryCachingServer {
            get
            {
                return QueryCachingServer_ID == null
                    ? null
                    : Repository.GetObjectByID<ExternalDatabaseServer>(QueryCachingServer_ID.Value);
            }
        }

        

        #endregion

        public CohortIdentificationConfiguration(ICatalogueRepository repository, string name)
        {
            repository.InsertAndHydrate(this,new Dictionary<string, object>
            {
                {"Name", name}
            });
        }

        public CohortIdentificationConfiguration(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            Name = r["Name"].ToString();
            Description = r["Description"] as string;
            RootCohortAggregateContainer_ID = ObjectToNullableInt(r["RootCohortAggregateContainer_ID"]);
            Ticket = r["Ticket"] as string;
            QueryCachingServer_ID = ObjectToNullableInt(r["QueryCachingServer_ID"]);

            Frozen = Convert.ToBoolean(r["Frozen"]);
            FrozenBy = r["FrozenBy"] as string;
            FrozenDate = ObjectToNullableDateTime(r["FrozenDate"]);
        }
        
        public bool StillExists()
        {
            return HasLocalChanges().Evaluation != ChangeDescription.DatabaseCopyWasDeleted;
        }
        
        public override void DeleteInDatabase()
        {
            //container is the parent class even though this is a 1 to 1 and there is a CASCADE which will actually nuke ourselves when we delete the root container!
            if (RootCohortAggregateContainer_ID != null)
                RootCohortAggregateContainer.DeleteInDatabase();

            //shouldnt ever happen but double check anyway incase somebody removes the CASCADE
            if(StillExists())
                base.DeleteInDatabase();
        }

        public void CreateRootContainerIfNotExists()
        {
            //if it doesn't have one
            if (RootCohortAggregateContainer_ID == null)
            {
                //create a new one and record it's ID
                RootCohortAggregateContainer_ID = new CohortAggregateContainer((ICatalogueRepository) Repository,SetOperation.UNION).ID;

                //save us to database to cement the object
                SaveToDatabase();
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public string GetSearchString()
        {
            //let the cic acronym match cohort identification configuration
            return "cic " + Name;
        }

        public ISqlParameter[] GetAllParameters()
        {
            return ((CatalogueRepository)Repository).GetAllParametersForParentTable(this).ToArray();
        }


        public string GetNamingConventionPrefixForConfigurations()
        {
            return  CICPrefix + ID + "_";
        }

        public bool IsValidNamedConfiguration(AggregateConfiguration aggregate)
        {
            return aggregate.Name.StartsWith(GetNamingConventionPrefixForConfigurations());
        }

        public void EnsureNamingConvention(AggregateConfiguration aggregate)
        {
            //it is already valid
            if (IsValidNamedConfiguration(aggregate))
                return;

            //make it valid by sticking on the prefix
            aggregate.Name = GetNamingConventionPrefixForConfigurations() + aggregate.Name;

            int copy = 0;
            string origName = aggregate.Name;


            var otherConfigurations = Repository.GetAllObjects<AggregateConfiguration>().Except(new[] {aggregate}).ToArray();

            //if there is a conflict on the name
            if (otherConfigurations.Any(c => c.Name.Equals(origName)))
            {
                do
                {
                    //add Copy 1 then Copy 2 etc
                    copy++;
                    aggregate.Name = origName + " (Copy " + copy + ")";
                }
                while (otherConfigurations.Any(c => c.Name.Equals(aggregate.Name)));//until there are no more copies

            }

            
            aggregate.SaveToDatabase();
        }

        public CohortIdentificationConfiguration CreateClone(ICheckNotifier notifier)
        {
            var cataRepo = ((CatalogueRepository) Repository);
            //start a new super transaction
            using (cataRepo.BeginNewTransactedConnection())
            {
                try
                {
                    notifier.OnCheckPerformed(new CheckEventArgs("Super Transaction started on Catalogue Repository", CheckResult.Success));

                    var clone = new CohortIdentificationConfiguration(cataRepo, Name + " (Clone)");

                    notifier.OnCheckPerformed(new CheckEventArgs("Created clone configuration '"+clone.Name+"' with ID " + clone.ID + " called " + clone, CheckResult.Success));

                    //clone the global parameters
                    foreach (var p in GetAllParameters())
                    {
                        notifier.OnCheckPerformed(new CheckEventArgs("Cloning global parameter " + p.ParameterName,CheckResult.Success));
                        var cloneP = new AnyTableSqlParameter(cataRepo, clone, p.ParameterSQL);
                        cloneP.Comment = p.Comment;
                        cloneP.Value = p.Value;
                        cloneP.SaveToDatabase();
                    }

                    //key is the original, value is the clone
                    var parentToCloneJoinablesDictionary = new Dictionary<JoinableCohortAggregateConfiguration, JoinableCohortAggregateConfiguration>();

                    //clone the joinables
                    foreach (var joinable in GetAllJoinables())
                    {
                        //clone the aggregate which has permission to be joinable
                        var cloneJoinableAggregate = joinable.AggregateConfiguration.CreateClone();

                        //clone the join permission
                        var cloneJoinable = new JoinableCohortAggregateConfiguration(cataRepo, clone, cloneJoinableAggregate);

                        parentToCloneJoinablesDictionary.Add(joinable,cloneJoinable);
                    }

                    clone.RootCohortAggregateContainer_ID = RootCohortAggregateContainer.CloneEntireTreeRecursively(notifier, this, clone, parentToCloneJoinablesDictionary).ID;
                    clone.SaveToDatabase();

                    notifier.OnCheckPerformed(new CheckEventArgs("Clone creation successful, about to commit Super Transaction", CheckResult.Success));
                    cataRepo.EndTransactedConnection(true);
                    notifier.OnCheckPerformed(new CheckEventArgs("Super Transaction committed successfully", CheckResult.Success));
                    return clone;
                }
                catch (Exception e)
                {
                    cataRepo.EndTransactedConnection(false);
                    notifier.OnCheckPerformed(new CheckEventArgs("Cloning failed, See Exception for details, the Super Transaction was rolled back successfully though", CheckResult.Fail,e));
                }

            }
            
            return null;
        }

        public AggregateConfiguration ImportAggregateConfigurationAsIdentifierList(AggregateConfiguration toClone, ChooseWhichExtractionIdentifierToUseFromManyHandler resolveMultipleExtractionIdentifiers, bool useTransaction = true)
        {
            if(!useTransaction)
                return CreateCloneOfAggregateConfigurationPrivate(toClone, resolveMultipleExtractionIdentifiers);

            var cataRepo = (CatalogueRepository) Repository;

            
            using (cataRepo.BeginNewTransactedConnection())
            {
                try
                {
                    var toReturn =  CreateCloneOfAggregateConfigurationPrivate(toClone, resolveMultipleExtractionIdentifiers);
                    cataRepo.EndTransactedConnection(true);
                    return toReturn;
                }
                catch (Exception)
                {
                    cataRepo.EndTransactedConnection(false);//abandon
                    throw;
                }
            }
        }

        private AggregateConfiguration CreateCloneOfAggregateConfigurationPrivate(AggregateConfiguration toClone, ChooseWhichExtractionIdentifierToUseFromManyHandler resolveMultipleExtractionIdentifiers)
        {
            var cataRepo = (CatalogueRepository)Repository;

            //two cases here either the import has a custom freaky CHI column (dimension) or it doesn't reference CHI at all if it is freaky we want to preserve it's freakyness
            ExtractionInformation underlyingExtractionInformation;
            IColumn extractionIdentifier = GetExtractionIdentifierFrom(toClone, out underlyingExtractionInformation, resolveMultipleExtractionIdentifiers);
            
            //clone will not have axis or pivot or dimensions other than extraction identifier
            var newConfiguration = cataRepo.CloneObjectInTable(toClone);

            //make it's name follow the naming convention e.g. cic_105_LINK103_MyAggregate 
            EnsureNamingConvention(newConfiguration);

            //now clear it's pivot dimension, make it not extratcable and make it's countSQL basic/sane
            newConfiguration.PivotOnDimensionID = null;
            newConfiguration.IsExtractable = false;
            newConfiguration.CountSQL = null;//clear the count sql

            //clone parameters
            foreach (AnyTableSqlParameter toCloneParameter in toClone.Parameters)
            {
                var newParam = new AnyTableSqlParameter((ICatalogueRepository)newConfiguration.Repository, newConfiguration, toCloneParameter.ParameterSQL);
                newParam.Value = toCloneParameter.Value;
                newParam.Comment = toCloneParameter.Comment;
                newParam.SaveToDatabase();
            }


            //now clone it's AggregateForcedJoins
            foreach (var t in cataRepo.AggregateForcedJoiner.GetAllForcedJoinsFor(toClone))
                cataRepo.AggregateForcedJoiner.CreateLinkBetween(newConfiguration, t);


            //now give it 1 dimension which is the only IsExtractionIdentifier column 
            var newDimension = new AggregateDimension(cataRepo, underlyingExtractionInformation, newConfiguration);

            //the thing we were cloning had a freaky CHI column (probably had a collate or something involved in it or a masterchi) 
            if (extractionIdentifier is AggregateDimension)
            {
                //preserve it's freakyness
                newDimension.Alias = extractionIdentifier.Alias;
                newDimension.SelectSQL = extractionIdentifier.SelectSQL;
                newDimension.Order = extractionIdentifier.Order;
                newDimension.SaveToDatabase();
            }

            //now rewire all it's filters
            if (toClone.RootFilterContainer_ID != null) //if it has any filters
            {
                //get the tree
                AggregateFilterContainer oldRootContainer = toClone.RootFilterContainer;

                //clone the tree
                var newRootContainer = oldRootContainer.DeepCloneEntireTreeRecursivelyIncludingFilters();
                newConfiguration.RootFilterContainer_ID = newRootContainer.ID;
            }

            newConfiguration.SaveToDatabase();

            return newConfiguration;
        }

        public delegate ExtractionInformation ChooseWhichExtractionIdentifierToUseFromManyHandler(Catalogue catalogue, ExtractionInformation[] candidates);

        public AggregateConfiguration CreateNewEmptyConfigurationForCatalogue(Catalogue catalogue, ChooseWhichExtractionIdentifierToUseFromManyHandler resolveMultipleExtractionIdentifiers, bool importMandatoryFilters = true)
        {
            var extractionIdentifier = (ExtractionInformation)GetExtractionIdentifierFrom(catalogue, resolveMultipleExtractionIdentifiers);

            var cataRepo = (ICatalogueRepository) Repository;
            
            AggregateConfiguration configuration = new AggregateConfiguration(cataRepo,catalogue, "People in " + catalogue);
            EnsureNamingConvention(configuration);

            //make the extraction identifier column into the sole dimension on the new configuration
            new AggregateDimension(cataRepo, extractionIdentifier, configuration);

            //no count sql
            configuration.CountSQL = null;
            configuration.SaveToDatabase();

            if(importMandatoryFilters)
                ImportMandatoryFilters(catalogue, configuration,GetAllParameters());

            return configuration;
        }

        private void ImportMandatoryFilters(Catalogue catalogue, AggregateConfiguration configuration, ISqlParameter[] globalParameters)
        {
            var filterImporter = new FilterImporter(new AggregateFilterFactory((ICatalogueRepository) catalogue.Repository), globalParameters);

            //Find any Mandatory Filters
            var mandatoryFilters = catalogue.GetAllMandatoryFilters();

            List<AggregateFilter> createdSoFar = new List<AggregateFilter>();

            if (mandatoryFilters.Any())
            {
                var container = new AggregateFilterContainer((ICatalogueRepository) Repository, FilterContainerOperation.AND);
                configuration.RootFilterContainer_ID = container.ID;
                configuration.SaveToDatabase();

                foreach (ExtractionFilter filter in mandatoryFilters)
                {
                    var newFilter = (AggregateFilter)filterImporter.ImportFilter(filter, createdSoFar.ToArray());

                    container.AddChild(newFilter);
                    createdSoFar.Add(newFilter);
                }
            }
        }

        /// <summary>
        /// returns an underlying ExtractionInformation which IsExtractionIdentifier (e.g. a chi column).  The first pass approach is to look for a suitable AggregateDimension which
        /// has an underlying  ExtractionInformation which IsExtractionIdentifier but if it doesn't find one then it will look in the Catalogue for one instead.  The reason this is 
        /// complex is because you can have datasets with multiple IsExtractionIdentifier columns e.g. SMR02 (birth records) where there is both MotherCHI and BabyCHI which are both
        /// IsExtractionIdentifier - in this case the AggregateConfiguration would need a Dimension of one or the other (but not both!) - if it had neither then the method would throw
        /// when it checked Catalogue and found both.
        /// </summary>
        /// <param name="toClone"></param>
        /// <param name="underlyingExtractionInformation"></param>
        /// <param name="resolveMultipleExtractionIdentifiers"></param>
        /// <returns></returns>
        private IColumn GetExtractionIdentifierFrom(AggregateConfiguration toClone, out ExtractionInformation underlyingExtractionInformation, ChooseWhichExtractionIdentifierToUseFromManyHandler resolveMultipleExtractionIdentifiers)
        {
            //make sure it can be cloned before starting

            //Aggregates have AggregateDimensions which are a subset of the Catalogues ExtractionInformations.  Most likely there won't be an AggregateDimension which IsExtractionIdentifier
            //because why would the user have graphs of chi numbers?

            var existingExtractionIdentifierDimensions = toClone.AggregateDimensions.Where(d => d.IsExtractionIdentifier).ToArray();
            IColumn extractionIdentifier = null;

            //very unexpected, he had multiple IsExtractionIdentifier Dimensions all configured for simultaneous use on this Aggregate, it is very freaky and we definetly don't want to let him import it for cohort identification
            if (existingExtractionIdentifierDimensions.Count() > 1)
                throw new NotSupportedException("Cannot clone the AggregateConfiguration " + toClone + " because it has multiple IsExtractionIdentifier dimensions (must be 0 or 1)");

            //It has 1... maybe it's already being used in cohort identification from another project or he just likes to graph chi numbers for some reason
            if (existingExtractionIdentifierDimensions.Count() == 1)
            {
                extractionIdentifier = existingExtractionIdentifierDimensions[0];
                underlyingExtractionInformation = existingExtractionIdentifierDimensions[0].ExtractionInformation;
            }
            else
            {
                //As expected the user isn't graphing chis or otherwise configured any Dimensions on this aggregate.  It is probably just a normal aggregate e.g. 'Prescriptions of Morphine over time between 2005-2001' where the only dimension is 'date prescribed'
                //now when we import this for cohort identification we want to keep the cool filters and stuff but we can junk all the dimensions.  BUT we do need to add the IsExtractionIdentifier column from the dataset (and if there are for some reason multiple
                //e.g. babychi, motherchi then we can invoke the delegate to let the user pick one.
                
                //get the unique IsExtractionIdentifier from the cloned configurations parent catalogue
                underlyingExtractionInformation = 
                    (ExtractionInformation)GetExtractionIdentifierFrom(
                    toClone.Catalogue, resolveMultipleExtractionIdentifiers);

                return underlyingExtractionInformation;
            }

            return extractionIdentifier;
        }

        private IColumn GetExtractionIdentifierFrom(Catalogue catalogue, ChooseWhichExtractionIdentifierToUseFromManyHandler resolveMultipleExtractionIdentifiers)
        {
            //the aggregate they are cloning does not have an extraction identifier but the dataset might still have one
            var catalogueCandidates = catalogue.GetAllExtractionInformation(ExtractionCategory.Any).Where(e => e.IsExtractionIdentifier).ToArray();

            //if there are multiple IsExtractionInformation columns
            if (catalogueCandidates.Count() != 1)
                if (resolveMultipleExtractionIdentifiers == null)//no delegate has been provided for resolving this
                    throw new NotSupportedException("Cannot create AggregateConfiguration because the Catalogue "+catalogue+" has " + catalogueCandidates.Length + " IsExtractionIdentifier ExtractionInformations");
                else
                {
                    //there is a delegate to resolve this, invoke it
                    var answer = resolveMultipleExtractionIdentifiers(catalogue, catalogueCandidates);

                    //the delegate returned null
                    if (answer == null)
                        throw new Exception("User did not pick a candidate ExtractionInformation column from those we offered");
                    else
                        return answer;//the delegate picked one
                }

            return catalogueCandidates[0];
        }

        public JoinableCohortAggregateConfiguration[] GetAllJoinables()
        {
            return Repository.GetAllObjectsWithParent<JoinableCohortAggregateConfiguration>(this).ToArray();
        }

        public TableInfo[] GetDistinctTableInfos()
        {
            return
                RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively() //get all the aggregate sets
                    .SelectMany(a=>a.AggregateDimensions //get all the Dimensions (should really be 1 per aggregate which is the IsExtractionIdentifier column but who are we to check)
                        .Select(d=>d.ColumnInfo)) //get the underlying Column for the dimension
                            .Select(c => c.TableInfo) //get the TableInfo from the column
                                .Distinct() //return distinct array of them
                                    .ToArray(); 

            
        }

        public void Freeze()
        {
            if(Frozen)
                return;

            Frozen = true;
            FrozenBy = Environment.UserName;
            FrozenDate = DateTime.Now;
            SaveToDatabase();
        }

        public void Unfreeze()
        {
            if(!Frozen)
                return;

            Frozen = false;
            FrozenBy = null;
            FrozenDate = null;
            SaveToDatabase();
        }

        public IHasDependencies[] GetObjectsThisDependsOn()
        {
            List<IHasDependencies> dependencies = new List<IHasDependencies>();
            
            dependencies.AddRange(GetAllParameters().Cast<AnyTableSqlParameter>());
            
            if(RootCohortAggregateContainer_ID != null)
                dependencies.AddRange(RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively());

            return dependencies.ToArray();
        }

        public IHasDependencies[] GetObjectsDependingOnThis()
        {
            return new IHasDependencies[0];
        }
    }
}
