using System.Collections.Generic;
using CatalogueLibrary.Data.Cohort;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace RDMPObjectVisualisation.Copying.Commands
{
    public class CohortAggregateContainerCommand : ICommand
    {
        public CohortAggregateContainer ParentContainerIfAny { get; private set; }
        public CohortAggregateContainer AggregateContainer { get; private set; }
        public List<CohortAggregateContainer> AllSubContainersRecursively  { get; private set; }

        public CohortAggregateContainerCommand(CohortAggregateContainer aggregateContainer)
        {
            AggregateContainer = aggregateContainer;
            AllSubContainersRecursively = AggregateContainer.GetAllSubContainersRecursively();

            ParentContainerIfAny = AggregateContainer.GetParentContainerIfAny();
        }

        

        public string GetSqlString()
        {
            return null;
        }
    }
}