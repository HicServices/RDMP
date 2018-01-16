using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data.Cohort.Joinables
{
    /// <summary>
    /// Relationship object which indicates that a given AggregateConfiguration is a 'PatientIndexTable'.  In order to be compatible as a 'PatientIndexTable' the 
    /// AggregateConfiguration must have one IsReleaseIdentifier AggregateDimension and usually at least one other column which has useful values in it (e.g. 
    /// admission dates).  The patient index table can then be used as part of other AggregateConfigurations in a CohortIdentificationConfiguration (e.g. 'find 
    /// all people in Deaths dataset who died within 3 months of having a prescription for drug Y' - where Prescriptions is the 'PatientIndexTable'. 
    /// </summary>
    public class JoinableCohortAggregateConfiguration : DatabaseEntity
    {
        #region Database Properties

        public int CohortIdentificationConfiguration_ID
        {
            get { return _cohortIdentificationConfigurationID; }
            set { SetField(ref  _cohortIdentificationConfigurationID, value); }
        }

        public int AggregateConfiguration_ID
        {
            get { return _aggregateConfigurationID; }
            set { SetField(ref  _aggregateConfigurationID, value); }
        }

        #endregion

        #region Relationships

        [NoMappingToDatabase]
        public JoinableCohortAggregateConfigurationUse[] Users
        {
            get { return Repository.GetAllObjectsWithParent<JoinableCohortAggregateConfigurationUse>(this).ToArray(); }
        }
        [NoMappingToDatabase]
        public CohortIdentificationConfiguration CohortIdentificationConfiguration {
            get
            {
                return Repository.GetObjectByID<CohortIdentificationConfiguration>(CohortIdentificationConfiguration_ID);
            }
        }

        [NoMappingToDatabase]
        public AggregateConfiguration AggregateConfiguration
        {
            get
            {
                return Repository.GetObjectByID<AggregateConfiguration>(AggregateConfiguration_ID);
            }
        }
        #endregion

        public JoinableCohortAggregateConfiguration(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            CohortIdentificationConfiguration_ID = Convert.ToInt32(r["CohortIdentificationConfiguration_ID"]);
            AggregateConfiguration_ID = Convert.ToInt32(r["AggregateConfiguration_ID"]);
        }

        public JoinableCohortAggregateConfiguration(ICatalogueRepository repository, CohortIdentificationConfiguration cic, AggregateConfiguration aggregate)
        {
            int extractionIdentifiers = aggregate.AggregateDimensions.Count(d => d.IsExtractionIdentifier);

            if( extractionIdentifiers != 1)
                throw new NotSupportedException("Cannot make aggregate " + aggregate + " into a Joinable aggregate because it has " + extractionIdentifiers + " columns marked IsExtractionIdentifier");

            if(aggregate.GetCohortAggregateContainerIfAny() != null)
                throw new NotSupportedException("Cannot make aggregate " + aggregate + " into a Joinable aggregate because it is already in a CohortAggregateContainer");

            repository.InsertAndHydrate(this, new Dictionary<string, object>()
            {
                {"CohortIdentificationConfiguration_ID",cic.ID},
                {"AggregateConfiguration_ID",aggregate.ID}
            });
        }

        public JoinableCohortAggregateConfigurationUse AddUser(AggregateConfiguration user)
        {
            if(user.ID == AggregateConfiguration_ID)
                throw new NotSupportedException("Cannot configure AggregateConfiguration "+ user + " as a Join user to itself!");

            return new JoinableCohortAggregateConfigurationUse((ICatalogueRepository) Repository, user, this);
        }


        private const string ToStringPrefix = "Patient Index Table:";
        private string _toStringName;
        private int _cohortIdentificationConfigurationID;
        private int _aggregateConfigurationID;

        public override string ToString()
        {
            return _toStringName ?? GetCachedName();
        }

        private string GetCachedName()
        {
            _toStringName = ToStringPrefix + AggregateConfiguration.Name;//cached answer
            return _toStringName;
        }
    }
}
