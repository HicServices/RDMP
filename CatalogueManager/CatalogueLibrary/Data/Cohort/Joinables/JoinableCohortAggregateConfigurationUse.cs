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
    /// Indicates that a given AggregateConfiguration in a CohortIdentificationConfiguration is implicitly joined against a 'PatientIndexTable' See JoinableCohortAggregateConfiguration
    /// </summary>
    public class JoinableCohortAggregateConfigurationUse:DatabaseEntity
    {
        #region Database Properties
        private int _joinableCohortAggregateConfigurationID;
        private int _aggregateConfigurationID;
        private ExtractionJoinType _joinType;

        public int JoinableCohortAggregateConfiguration_ID
        {
            get { return _joinableCohortAggregateConfigurationID; }
            set { SetField(ref  _joinableCohortAggregateConfigurationID, value); }
        }

        public int AggregateConfiguration_ID
        {
            get { return _aggregateConfigurationID; }
            set { SetField(ref  _aggregateConfigurationID, value); }
        }

        public ExtractionJoinType JoinType
        {
            get { return _joinType; }
            set { SetField(ref  _joinType, value); }
        }

        #endregion

        #region Relationships
        [NoMappingToDatabase]
        public JoinableCohortAggregateConfiguration JoinableCohortAggregateConfiguration
        {
            get
            {
                return Repository.GetObjectByID<JoinableCohortAggregateConfiguration>(JoinableCohortAggregateConfiguration_ID);
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

        public JoinableCohortAggregateConfigurationUse(ICatalogueRepository repository, DbDataReader r)
            : base(repository, r)
        {
            ExtractionJoinType joinType;

            if (ExtractionJoinType.TryParse(r["JoinType"].ToString(), true, out joinType))
                JoinType = joinType;

            JoinableCohortAggregateConfiguration_ID = Convert.ToInt32(r["JoinableCohortAggregateConfiguration_ID"]);
            AggregateConfiguration_ID = Convert.ToInt32(r["AggregateConfiguration_ID"]);

        }

        internal JoinableCohortAggregateConfigurationUse(ICatalogueRepository repository, AggregateConfiguration user, JoinableCohortAggregateConfiguration joinable)
        {
            if (repository.GetAllObjects<JoinableCohortAggregateConfiguration>("WHERE AggregateConfiguration_ID = " + user.ID).Any())
                throw new NotSupportedException("Cannot add user " + user + " because that AggregateConfiguration is itself a JoinableCohortAggregateConfiguration");
         
            if(user.AggregateDimensions.Count(u=>u.IsExtractionIdentifier) != 1)
                throw new NotSupportedException("Cannot configure AggregateConfiguration " + user + " as join user because it does not contain exactly 1 IsExtractionIdentifier dimension");

            repository.InsertAndHydrate(this,new Dictionary<string, object>()
            {
                {"JoinableCohortAggregateConfiguration_ID",joinable.ID},
                {"AggregateConfiguration_ID",user.ID},
                {"JoinType",ExtractionJoinType.Left.ToString()}

            });
        }

        public string GetJoinDirectionSQL()
        {
            switch (JoinType)
            {
                case ExtractionJoinType.Left:
                    return "LEFT";
                case ExtractionJoinType.Right:
                    return "RIGHT";
                case ExtractionJoinType.Inner:
                    return "INNER";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        private const string ToStringPrefix = "JOIN Against:";
        private string _toStringName;
        

        public override string ToString()
        {
            return _toStringName ?? GetCachedName();
        }

        private string GetCachedName()
        {
            _toStringName =  ToStringPrefix + JoinableCohortAggregateConfiguration.AggregateConfiguration.Name;//cached answer
            return _toStringName;
        }

        public string GetJoinTableAlias()
        {
            return "ix" + JoinableCohortAggregateConfiguration_ID;
        }
    }
}
