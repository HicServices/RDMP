using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.Cohort.Joinables;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.Copying.Commands
{
    public class AggregateConfigurationCommand : ICommand
    {
        public AggregateConfiguration Aggregate { get; private set; }
        public CohortIdentificationConfiguration CohortIdentificationConfigurationIfAny { get; private set; }
        public CohortAggregateContainer ContainerIfAny { get; set; }
        
        public List<CohortAggregateContainer> AllContainersInTreeIfPartOfOne { get; private set; }
        public bool IsPatientIndexTable { get; set; }

        public JoinableCohortAggregateConfiguration JoinableDeclarationIfAny { get; set; }
        public AggregateConfiguration[] JoinableUsersIfAny { get; set; }

        public AggregateConfigurationCommand(AggregateConfiguration aggregate)
        {
            Aggregate = aggregate;

            IsPatientIndexTable = Aggregate.IsJoinablePatientIndexTable();
            
            //is the aggregate part of cohort identification
            CohortIdentificationConfigurationIfAny = Aggregate.GetCohortIdentificationConfigurationIfAny();
            
            //assume no join users
            JoinableUsersIfAny = new AggregateConfiguration[0];

            //unless theres a cic
            if(CohortIdentificationConfigurationIfAny != null)
            {
                //with this aggregate as a joinable
                JoinableDeclarationIfAny = CohortIdentificationConfigurationIfAny.GetAllJoinables().SingleOrDefault(j=>j.AggregateConfiguration_ID == Aggregate.ID);

                //then get the joinable users if any and use that array
                if (JoinableDeclarationIfAny != null)
                    JoinableUsersIfAny = JoinableDeclarationIfAny.Users.Select(u => u.AggregateConfiguration).ToArray();

            }

            //if so we should find out all the containers in the tree (Containers are INTERSECT\EXCEPT\UNION)
            AllContainersInTreeIfPartOfOne = new List<CohortAggregateContainer>();

            //if it is part of cohort identification
            if (CohortIdentificationConfigurationIfAny != null)
            {
                //find all the containers so we can prevent us being copied into one of them
                var root = CohortIdentificationConfigurationIfAny.RootCohortAggregateContainer;
                AllContainersInTreeIfPartOfOne.Add(root);
                AllContainersInTreeIfPartOfOne.AddRange(root.GetAllSubContainersRecursively());
            }
            
            ContainerIfAny = Aggregate.GetCohortAggregateContainerIfAny();
        }

        public string GetSqlString()
        {
            return null;
        }
    }
}