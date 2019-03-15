using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using ReusableLibraryCode;

namespace CatalogueLibrary.Repositories.Managers
{
    class CohortContainerManager : ICohortContainerManager
    {
        private readonly CatalogueRepository _catalogueRepository;

        public CohortContainerManager(CatalogueRepository catalogueRepository)
        {
            _catalogueRepository = catalogueRepository;
        }

        public CohortAggregateContainer GetCohortAggregateContainerIfAny(AggregateConfiguration aggregateConfiguration)
        {
            return
                _catalogueRepository.SelectAllWhere<CohortAggregateContainer>(
                    "SELECT CohortAggregateContainer_ID FROM CohortAggregateContainer_AggregateConfiguration WHERE AggregateConfiguration_ID = @AggregateConfiguration_ID",
                    "CohortAggregateContainer_ID",
                    new Dictionary<string, object>
                    {
                        {"AggregateConfiguration_ID", aggregateConfiguration.ID}
                    }).SingleOrDefault();
        }

        public void AddConfigurationToContainer(AggregateConfiguration configuration, CohortAggregateContainer cohortAggregateContainer,
            int order)
        {

            _catalogueRepository.Insert(
                "INSERT INTO CohortAggregateContainer_AggregateConfiguration (CohortAggregateContainer_ID, AggregateConfiguration_ID, [Order]) VALUES (@CohortAggregateContainer_ID, @AggregateConfiguration_ID, @Order)",
                new Dictionary<string, object>
                {
                    {"CohortAggregateContainer_ID", cohortAggregateContainer.ID},
                    {"AggregateConfiguration_ID", configuration.ID},
                    {"Order", order}
                });

        }

        public void RemoveConfigurationFromContainer(AggregateConfiguration configuration,
            CohortAggregateContainer cohortAggregateContainer)
        {
            _catalogueRepository.Delete("DELETE FROM CohortAggregateContainer_AggregateConfiguration WHERE CohortAggregateContainer_ID = @CohortAggregateContainer_ID AND AggregateConfiguration_ID = @AggregateConfiguration_ID", new Dictionary<string, object>
            {
                {"CohortAggregateContainer_ID", cohortAggregateContainer.ID},
                {"AggregateConfiguration_ID", configuration.ID}
            });
        }

        
        public int? GetOrderIfExistsFor(AggregateConfiguration configuration)
        {
            if (configuration.Repository != this)
                if (((CatalogueRepository)configuration.Repository).ConnectionString != _catalogueRepository.ConnectionString)
                    throw new NotSupportedException("AggregateConfiguration is from a different repository than this with a different connection string");

            using (var con = _catalogueRepository.GetConnection())
            {
                DbCommand cmd = DatabaseCommandHelper.GetCommand("SELECT [Order] FROM CohortAggregateContainer_AggregateConfiguration WHERE AggregateConfiguration_ID = @AggregateConfiguration_ID", con.Connection, con.Transaction);

                cmd.Parameters.Add(DatabaseCommandHelper.GetParameter("@AggregateConfiguration_ID", cmd));
                cmd.Parameters["@AggregateConfiguration_ID"].Value = configuration.ID;

                return _catalogueRepository.ObjectToNullableInt(cmd.ExecuteScalar());
            }
        }

        public CohortAggregateContainer[] GetSubContainers(CohortAggregateContainer cohortAggregateContainer)
        {
            return _catalogueRepository.SelectAllWhere<CohortAggregateContainer>("SELECT CohortAggregateContainer_ChildID FROM CohortAggregateSubContainer WHERE CohortAggregateContainer_ParentID=@CohortAggregateContainer_ParentID",
                "CohortAggregateContainer_ChildID",
                new Dictionary<string, object>
                {
                    {"CohortAggregateContainer_ParentID", cohortAggregateContainer.ID}
                }).ToArray();
        }

        public AggregateConfiguration[] GetAggregateConfigurations(CohortAggregateContainer cohortAggregateContainer)
        {
            return _catalogueRepository.SelectAll<AggregateConfiguration>("SELECT AggregateConfiguration_ID FROM CohortAggregateContainer_AggregateConfiguration where CohortAggregateContainer_ID=" + cohortAggregateContainer.ID).OrderBy(config => config.Order).ToArray();
        }

        public CohortAggregateContainer GetParentContainerIfAny(CohortAggregateContainer cohortAggregateContainer)
        {
            return _catalogueRepository.SelectAllWhere<CohortAggregateContainer>("SELECT CohortAggregateContainer_ParentID FROM CohortAggregateSubContainer WHERE CohortAggregateContainer_ChildID=@CohortAggregateContainer_ChildID",
                "CohortAggregateContainer_ParentID",
                new Dictionary<string, object>
                {
                    {"CohortAggregateContainer_ChildID", cohortAggregateContainer.ID}
                }).SingleOrDefault();
        }

        public void RemoveSubContainerFrom(CohortAggregateContainer parent, CohortAggregateContainer child)
        {
            _catalogueRepository.Delete("DELETE FROM CohortAggregateSubContainer WHERE CohortAggregateContainer_ChildID = @CohortAggregateContainer_ChildID", new Dictionary<string, object>
            {
                {"CohortAggregateContainer_ChildID", child.ID}
            });
        }
    }
}