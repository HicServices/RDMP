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
    /// AggregateConfiguration must have one IsExtractionIdentifier AggregateDimension and usually at least one other column which has useful values in it (e.g. 
    /// admission dates).  The patient index table can then be used as part of other AggregateConfigurations in a CohortIdentificationConfiguration (e.g. 'find 
    /// all people in Deaths dataset who died within 3 months of having a prescription for drug Y' - where Prescriptions is the 'PatientIndexTable'. 
    /// </summary>
    public class JoinableCohortAggregateConfiguration : DatabaseEntity
    {
        #region Database Properties

        /// <summary>
        /// ID of the <see cref="CohortIdentificationConfiguration"/> for which the <see cref="AggregateConfiguration_ID"/> acts as a patient index table
        /// </summary>
        public int CohortIdentificationConfiguration_ID
        {
            get { return _cohortIdentificationConfigurationID; }
            set { SetField(ref  _cohortIdentificationConfigurationID, value); }
        }

        /// <summary>
        /// ID of the <see cref="AggregateConfiguration_ID"/> which this class is making act as a patient index table
        /// </summary>
        public int AggregateConfiguration_ID
        {
            get { return _aggregateConfigurationID; }
            set { SetField(ref  _aggregateConfigurationID, value); }
        }

        #endregion

        #region Relationships

        /// <summary>
        /// Gets all the users of the patient index table, these <see cref="AggregateConfiguration"/> will be joined against the patient index table at query generation time.
        /// <para>The returned objects are <see cref="JoinableCohortAggregateConfigurationUse"/> which is the mandate to link against us.  Use 
        /// <see cref="JoinableCohortAggregateConfigurationUse.AggregateConfiguration"/> to fetch the actual <see cref="AggregateConfiguration"/></para>
        /// </summary>
        [NoMappingToDatabase]
        public JoinableCohortAggregateConfigurationUse[] Users
        {
            get { return Repository.GetAllObjectsWithParent<JoinableCohortAggregateConfigurationUse>(this).ToArray(); }
        }
        /// <inheritdoc cref="CohortIdentificationConfiguration_ID"/>
        [NoMappingToDatabase]
        public CohortIdentificationConfiguration CohortIdentificationConfiguration {
            get
            {
                return Repository.GetObjectByID<CohortIdentificationConfiguration>(CohortIdentificationConfiguration_ID);
            }
        }

        /// <inheritdoc cref="AggregateConfiguration_ID"/>
        [NoMappingToDatabase]
        public AggregateConfiguration AggregateConfiguration
        {
            get
            {
                return Repository.GetObjectByID<AggregateConfiguration>(AggregateConfiguration_ID);
            }
        }
        #endregion

        internal JoinableCohortAggregateConfiguration(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            CohortIdentificationConfiguration_ID = Convert.ToInt32(r["CohortIdentificationConfiguration_ID"]);
            AggregateConfiguration_ID = Convert.ToInt32(r["AggregateConfiguration_ID"]);
        }

        /// <summary>
        /// Declares that the passed <see cref="AggregateConfiguration"/> should act as a patient index table and be joinable with other <see cref="AggregateConfiguration"/>s in
        /// the <see cref="CohortIdentificationConfiguration"/>.
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="cic"></param>
        /// <param name="aggregate"></param>
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

        /// <summary>
        /// Mandates that the passed <see cref="AggregateConfiguration"/> should join with this patient index table at query generation time.  The <paramref name="user"/> must 
        /// be part of the same <see cref="CohortIdentificationConfiguration"/> as the patient index table (<see cref="CohortIdentificationConfiguration_ID"/>)
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
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

        /// <inheritdoc/>
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
