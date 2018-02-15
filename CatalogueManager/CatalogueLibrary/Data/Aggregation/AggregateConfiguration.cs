using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Configuration;
using System.Text.RegularExpressions;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.Cohort.Joinables;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.QueryBuilding;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Revertable;

using ReusableLibraryCode;
using ReusableLibraryCode.Checks;

namespace CatalogueLibrary.Data.Aggregation
{
    /// <summary>
    /// Entry point for the aggregation system.  This class describes what a given aggregation is supposed to achieve (e.g. summarise the number of records in a 
    /// dataset by region over time since 2001 to present).  An AggregateConfiguration belongs to a given Catalogue and is the hanging-off point for the rest of
    /// the configuration (e.g. AggregateDimension / AggregateFilter)
    /// 
    /// AggregateConfigurations can be used with an AggregateBuilder to produce runnable SQL which will return a DataTable containing results appropriate to the
    /// query being built.
    /// 
    /// There are Three types of AggregateConfiguration:
    ///  1. 'Aggregate Graph' - Produce summary information about a dataset designed to be displayed in a graph e.g. number of records each year by healthboard
    ///  2. 'Cohort Aggregate' - Produce a list of unique patient identifiers from a dataset (e.g. 'all patients with HBA1c test code > 50 in biochemistry')
    ///  3. 'Joinable PatientIndex Table' - Produce a patient identifier fact table for joining to other Cohort Aggregates during cohort building (See 
    /// JoinableCohortAggregateConfiguration).
    /// 
    /// The above labels are informal terms.  Use IsCohortIdentificationAggregate and IsJoinablePatientIndexTable to determine what type a given
    /// AggregateConfiguration is. 
    /// 
    /// If your Aggregate is part of cohort identification (Identifier List or Patient Index Table) then its name will start with cic_X_ where X is the ID of the cohort identification 
    /// configuration.  Depending on the user interface though this might not appear (See ToString implementation).
    /// </summary>
    public class AggregateConfiguration : VersionedDatabaseEntity, ICheckable, IOrderable, ICollectSqlParameters,INamed,IHasDependencies
    {
        #region Database Properties
        private string _countSQL;
        private int _catalogueID;
        private string _name;
        private string _description;
        private DateTime _dtCreated;
        private int? _pivotOnDimensionID;
        private bool _isExtractable;
        private string _havingSQL;


        public string CountSQL
        {
            get { return _countSQL; }
            set { SetField(ref  _countSQL, value); }
        }

        public int Catalogue_ID
        {
            get { return _catalogueID; }
            set { SetField(ref  _catalogueID, value); }
        }

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

        public DateTime dtCreated
        {
            get { return _dtCreated; }
            set { SetField(ref  _dtCreated, value); }
        }

        public int? PivotOnDimensionID
        {
            get { return _pivotOnDimensionID; }
            set { SetField(ref  _pivotOnDimensionID, value); }
        }

        public bool IsExtractable
        {
            get { return _isExtractable; }
            set { SetField(ref  _isExtractable, value); }
        }

        public string HavingSQL
        {
            get { return _havingSQL; }
            set { SetField(ref  _havingSQL, value); }
        }
        
        public int? RootFilterContainer_ID
        {
            get { return _rootFilterContainerID; }
            set
            {
                if (OverrideFiltersByUsingParentAggregateConfigurationInstead_ID != null && value != null)
                    throw new NotSupportedException(
                        string.Format(
                            "This AggregateConfiguration has a shortcut to another AggregateConfiguration's Filters (It's OverrideFiltersByUsingParentAggregateConfigurationInstead_ID is {0}) which means it cannot be assigned it's own RootFilterContainerID",
                            OverrideFiltersByUsingParentAggregateConfigurationInstead_ID));

                SetField(ref _rootFilterContainerID ,value);
            }
        }

        public int? OverrideFiltersByUsingParentAggregateConfigurationInstead_ID
        {
            get { return _overrideFiltersByUsingParentAggregateConfigurationInsteadID; }
            set
            {
                if (RootFilterContainer_ID != null && value != null)
                    throw new NotSupportedException(
                        "Cannot set OverrideFiltersByUsingParentAggregateConfigurationInstead_ID because this AggregateConfiguration already has a filter container set (if you were to be a shortcut and also have a filter tree set it would be very confusing)");

                SetField(ref _overrideFiltersByUsingParentAggregateConfigurationInsteadID, value);
            }
        }

        #endregion

        #region Relationships

        [NoMappingToDatabase]
        public Catalogue Catalogue
        {
            get { return Repository.GetObjectByID<Catalogue>(Catalogue_ID); }
        }

        [NoMappingToDatabase]
        public AggregateFilterContainer RootFilterContainer
        {
            get
            {
                //if there is an override
                if (OverrideFiltersByUsingParentAggregateConfigurationInstead_ID != null)
                    return Repository.GetObjectByID<AggregateConfiguration>(
                        (int) OverrideFiltersByUsingParentAggregateConfigurationInstead_ID).RootFilterContainer;
                        //return the overriding one

                //else return the actual root filter container or null if there isn't one
                return RootFilterContainer_ID == null
                    ? null
                    : Repository.GetObjectByID<AggregateFilterContainer>((int) RootFilterContainer_ID);
            }
        }

        [NoMappingToDatabase]
        public IEnumerable<AnyTableSqlParameter> Parameters
        {
            get { return ((CatalogueRepository) Repository).GetAllParametersForParentTable(this); }
        }

        //Satisfies interface
        public ISqlParameter[] GetAllParameters()
        {
            return Parameters.ToArray();
        }

        [NoMappingToDatabase]
        public TableInfo[] ForcedJoins
        {
            get { return ((CatalogueRepository) Repository).AggregateForcedJoiner.GetAllForcedJoinsFor(this); }
        }

        [NoMappingToDatabase]
        public AggregateDimension[] AggregateDimensions
        {
            get { return Repository.GetAllObjectsWithParent<AggregateDimension>(this).ToArray(); }
        }


        [NoMappingToDatabase]
        public AggregateDimension PivotDimension
        {
            get
            {
                return PivotOnDimensionID == null
                    ? null
                    : Repository.GetObjectByID<AggregateDimension>((int) PivotOnDimensionID);
            }
        }

        [NoMappingToDatabase]
        public AggregateConfiguration OverrideFiltersByUsingParentAggregateConfigurationInstead
        {
            get
            {
                return _overrideFiltersByUsingParentAggregateConfigurationInsteadID == null
                    ? null
                    : Repository.GetObjectByID<AggregateConfiguration>(
                        (int) _overrideFiltersByUsingParentAggregateConfigurationInsteadID);
            }
        }
         
        [NoMappingToDatabase]
        public JoinableCohortAggregateConfigurationUse[] PatientIndexJoinablesUsed {
            get { return Repository.GetAllObjectsWithParent<JoinableCohortAggregateConfigurationUse>(this).ToArray(); }
        }

        #endregion

        [NoMappingToDatabase]
        public int Order
        {
            get
            {
                if (!orderFetchAttempted)
                {
                    ReFetchOrder();
                    orderFetchAttempted = true;
                }


                if (_orderWithinKnownContainer == null)
                    throw new NotSupportedException(this + " is not part of any known containers");

                return (int)_orderWithinKnownContainer;
            }

            set
            {
                CohortAggregateContainer.SetOrderIfExistsFor(this, value);
                _orderWithinKnownContainer = value;
            }
        }

        [NoMappingToDatabase]
        public bool IsCohortIdentificationAggregate
        {
            get { return Name.StartsWith(CohortIdentificationConfiguration.CICPrefix); }
        }

        public AggregateConfiguration(ICatalogueRepository repository, ICatalogue catalogue, string name)
        {
            repository.InsertAndHydrate(this, new Dictionary<string, object>
            {
                {"Name", name},
                {"Catalogue_ID", catalogue.ID}
            });
        }
        
        public AggregateConfiguration(ICatalogueRepository repository, DbDataReader r) : base(repository, r)
        {
            Name = r["Name"] as string;
            Description = r["Description"] as string;
            Catalogue_ID = int.Parse(r["Catalogue_ID"].ToString());
            dtCreated = DateTime.Parse(r["dtCreated"].ToString());

            CountSQL = r["CountSQL"] as string;
            HavingSQL = r["HavingSQL"] as string;

            object rootFilterID = r["RootFilterContainer_ID"];

            if (rootFilterID == null || rootFilterID == DBNull.Value)
                RootFilterContainer_ID = null;
            else
                RootFilterContainer_ID = int.Parse(rootFilterID.ToString());

            if (r["PivotOnDimensionID"] == DBNull.Value)
                PivotOnDimensionID = null;
            else
                PivotOnDimensionID = Convert.ToInt32(r["PivotOnDimensionID"]);

            IsExtractable = Convert.ToBoolean(r["IsExtractable"]);

            OverrideFiltersByUsingParentAggregateConfigurationInstead_ID =
                ObjectToNullableInt(r["OverrideFiltersByUsingParentAggregateConfigurationInstead_ID"]);

            
        }

        public void ReFetchOrder()
        {
            _orderWithinKnownContainer = ((CatalogueRepository) Repository).GetOrderIfExistsFor(this);
        }

        public override string ToString()
        {
            //strip the cic section from the front
            return Regex.Replace(Name, @"cic_\d+_?", "");
        }


        public void AddDimension(ExtractionInformation basedOnColumn)
        {
            new AggregateDimension((ICatalogueRepository) basedOnColumn.Repository, basedOnColumn, this);
        }

        public AggregateBuilder GetQueryBuilder(int? topX = null)
        {
            if(string.IsNullOrWhiteSpace(CountSQL))
                throw new NotSupportedException("Cannot generate an AggregateBuilder because the AggregateConfiguration '" + this + "' has no Count SQL, usually this is the case for 'Cohort Set' Aggregates or 'Patient Index Table' Aggregates.  In either case you should use CohortQueryBuilder instead of AggregateBuilder");

            var allForcedJoins = ForcedJoins.ToArray();

            AggregateBuilder builder;
            var limitationSQLIfAny = topX == null ? null : "TOP " + topX.Value;

            if (allForcedJoins.Any())
                builder = new AggregateBuilder(limitationSQLIfAny, CountSQL, this, allForcedJoins);
            else
                builder = new AggregateBuilder(limitationSQLIfAny, CountSQL, this);

            builder.AddColumnRange(AggregateDimensions.ToArray());
            builder.RootFilterContainer = RootFilterContainer;

            if (PivotOnDimensionID != null)
                builder.SetPivotToDimensionID(PivotDimension);

            return builder;
        }


        public AggregateContinuousDateAxis GetAxisIfAny()
        {
            //for each dimension
            foreach (AggregateDimension aggregateDimension in AggregateDimensions)
            {
                //if it has an axis
                var axis = aggregateDimension.AggregateContinuousDateAxis;
                if (axis != null)
                    return axis; //return it
            }

            //done
            return null;
        }

        public AggregateTopX GetTopXIfAny()
        {
            return Repository.GetAllObjectsWithParent<AggregateTopX>(this).SingleOrDefault();
        }
        public bool IsAcceptableAsCohortGenerationSource(out string reason)
        {
            reason = null;

            var dimensions = AggregateDimensions.ToArray();

            if (dimensions.Count(d => d.IsExtractionIdentifier) != 1)
                reason = "There must be exactly 1 Dimension which is marked IsExtractionIdentifier";

            if (PivotOnDimensionID != null)
                reason = "It cannot contain a pivot";

            if (GetAxisIfAny() != null)
                reason = "It cannot have any axises";

            return reason == null;
        }


        public void Check(ICheckNotifier notifier)
        {
            //these are not checkable since they are intended for use by CohortQueryBuilder instead
            if(IsCohortIdentificationAggregate)
                return;

            try
            {
                var qb = GetQueryBuilder();
                notifier.OnCheckPerformed(new CheckEventArgs("successfully generated Aggregate SQL:" + qb.SQL,
                    CheckResult.Success));
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Failed to generate Aggregate SQL", CheckResult.Fail, e));
            }
        }

        private int? _orderWithinKnownContainer;
        private int? _overrideFiltersByUsingParentAggregateConfigurationInsteadID;
        private int? _rootFilterContainerID;
        
        private bool orderFetchAttempted;
        


        public CohortAggregateContainer GetCohortAggregateContainerIfAny()
        {
            return
                Repository.SelectAllWhere<CohortAggregateContainer>(
                    "SELECT CohortAggregateContainer_ID FROM CohortAggregateContainer_AggregateConfiguration WHERE AggregateConfiguration_ID = @AggregateConfiguration_ID",
                    "CohortAggregateContainer_ID",
                    new Dictionary<string, object>
                    {
                        {"AggregateConfiguration_ID", ID}
                    }).SingleOrDefault();
        }

        /// <summary>
        /// All AggregateConfigurations have the potential a'Joinable Patient Index Table' (see AggregateConfiguration class documentation).  This method returns
        /// true if there is an associated JoinableCohortAggregateConfiguration that would make an ordinary AggregateConfiguration into a 'Patient Index Table'.
        /// </summary>
        /// <returns>true if the AggregateConfiguration is part of a cic fulfilling the role of 'Patient Index Table' as defined by the existence of a JoinableCohortAggregateConfiguration object</returns>
        public bool IsJoinablePatientIndexTable()
        {
            if (!_databaseLookupPerformed)
            {
                _cachedJoinable = Repository.GetAllObjectsWithParent<JoinableCohortAggregateConfiguration>(this).SingleOrDefault();
                _databaseLookupPerformed = true;
            }

            return _cachedJoinable != null;
        }

        public CohortIdentificationConfiguration GetCohortIdentificationConfigurationIfAny()
        {
            //see if there is a container for this Aggregate
            var container = GetCohortAggregateContainerIfAny();


            if (container != null)
                return container.GetCohortIdentificationConfiguration();
            
            //it is not part of a container, maybe it is a joinable?
            var joinable = Repository.GetAllObjectsWithParent<JoinableCohortAggregateConfiguration>(this).SingleOrDefault();

            //it is a joinable (Patient Index Table) so return it 
            if (joinable != null)
                return joinable.CohortIdentificationConfiguration;

            return null;
        }

        public AggregateConfiguration CreateClone()
        {
            var cataRepo = (CatalogueRepository) Repository;
            var clone = Repository.CloneObjectInTable(this);

            clone.Name = Name + "(Clone)";

            if(clone.PivotOnDimensionID != null)
                throw new NotImplementedException("Cannot clone due to PIVOT");

            foreach (AggregateDimension aggregateDimension in AggregateDimensions)
            {
                var cloneDimension = new AggregateDimension((ICatalogueRepository) Repository, aggregateDimension.ExtractionInformation, clone);
                cloneDimension.Alias = aggregateDimension.Alias;
                cloneDimension.SelectSQL = aggregateDimension.SelectSQL;
                cloneDimension.Order = aggregateDimension.Order;
                cloneDimension.SaveToDatabase();

                if(aggregateDimension.AggregateContinuousDateAxis != null)
                    throw new NotImplementedException("Cannot clone due to AXIS");
            }

            //now clone it's AggregateForcedJoins
            foreach (var t in cataRepo.AggregateForcedJoiner.GetAllForcedJoinsFor(this))
                cataRepo.AggregateForcedJoiner.CreateLinkBetween(clone, t);
            
            if (RootFilterContainer_ID != null)
            {
                var clonedContainerSet = RootFilterContainer.DeepCloneEntireTreeRecursivelyIncludingFilters();
                clone.RootFilterContainer_ID = clonedContainerSet.ID;
            }

            foreach (var p in GetAllParameters())
            {
                var cloneP = new AnyTableSqlParameter(cataRepo, clone, p.ParameterSQL);
                cloneP.Comment = p.Comment;
                cloneP.Value = p.Value;
                cloneP.SaveToDatabase();
            }
            

            clone.SaveToDatabase();

            return clone;
        }
        /// <summary>
        /// Using the set; on Order property changes the Order. if you want to informt his object of it's Order because you already know what it is but this object doesn't know yet
        /// then you can use this method to force a specific known order onto the object in memory.
        /// </summary>
        /// <param name="currentOrder"></param>
        public void SetKnownOrder(int currentOrder)
        {
            orderFetchAttempted = true;
            _orderWithinKnownContainer = currentOrder;
        }

        public override void DeleteInDatabase()
        {
            var container = GetCohortAggregateContainerIfAny();
               
            if(container != null)
                container.RemoveChild(this);

            var isAJoinable = Repository.GetAllObjectsWithParent<JoinableCohortAggregateConfiguration>(this).SingleOrDefault();
            if(isAJoinable != null)
                if (isAJoinable.Users.Any())
                    throw new NotSupportedException("Cannot Delete AggregateConfiguration '" + this + "' because it is a Joinable Patient Index Table AND it has join users.  You must first remove the join usages and then you can delete it.");
                else
                    isAJoinable.DeleteInDatabase();

            base.DeleteInDatabase();
        }

        

        public IHasDependencies[] GetObjectsThisDependsOn()
        {
            List<IHasDependencies> dependencies = new List<IHasDependencies>();
            dependencies.AddRange(AggregateDimensions);
            dependencies.AddRange(Parameters);
            dependencies.AddRange(ForcedJoins);
            dependencies.Add(Catalogue);

            return dependencies.ToArray();
        }

        public IHasDependencies[] GetObjectsDependingOnThis()
        {
            List<IHasDependencies> dependers = new List<IHasDependencies>();

            var cic = GetCohortIdentificationConfigurationIfAny();
            if(cic != null)
                dependers.Add(cic);

            return dependers.ToArray();
        }

        private JoinableCohortAggregateConfiguration _cachedJoinable;
        private bool _databaseLookupPerformed;
        /// <summary>
        /// All AggregateConfigurations have the potential a'Joinable Patient Index Table' (see AggregateConfiguration class documentation).  This method injects
        /// what fact that the AggregateConfiguration is definetly one by passing the JoinableCohortAggregateConfiguration that makes it one.  Pass null in to 
        /// indicate that the AggregateConfiguration is definetly NOT ONE.  See also the method IsJoinablePatientIndexTable
        /// </summary>
        /// <param name="joinable"></param>
        public void InjectKnownJoinableOrNone(JoinableCohortAggregateConfiguration joinable)
        {
            _cachedJoinable = joinable;
            _databaseLookupPerformed = true;
        }
    }
}
