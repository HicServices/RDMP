using Rdmp.Core.CommandExecution.AtomicCommands.Automation;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Rdmp.Core.CommandExecution.AtomicCommands.CohortCreationCommands;

public class ExecuteCommandCreateSavableInstanceOfCohortIdentificationConfiguration : BasicCommandExecution, IAtomicCommand
{
    private CohortIdentificationConfiguration _cohortIdentificationConfiguration;
    private IBasicActivateItems _basicActivateItems;

    public ExecuteCommandCreateSavableInstanceOfCohortIdentificationConfiguration(IBasicActivateItems activator, CohortIdentificationConfiguration cic) : base(activator)
    {
        _cohortIdentificationConfiguration = cic;
        _basicActivateItems = activator;
    }

    public override void Execute()
    {
        base.Execute();
        CohortAggregateContainer rootCohortAggregateContainer = _cohortIdentificationConfiguration.RootCohortAggregateContainer;
        var x = JsonSerializer.Serialize<CohortAggregateContainer>(rootCohortAggregateContainer);
        Console.WriteLine(x);
        foreach (AggregateConfiguration aggregateConfiguration in rootCohortAggregateContainer.GetAggregateConfigurations())
        {
            var dimensions = _basicActivateItems.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<AggregateDimension>("AggregateConfiguration_ID", aggregateConfiguration.ID);
 
            foreach (AggregateDimension dimension in dimensions)
            { 
            }

            var rootFilterContainerID = aggregateConfiguration.RootFilterContainer_ID;
            if (rootFilterContainerID != null)
            {
                var rootFilterContainer = _basicActivateItems.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<AggregateFilterContainer>("ID", rootFilterContainerID).FirstOrDefault();
                if (rootFilterContainer != null)
                {
                    //filters
                    foreach (AggregateFilter filter in rootFilterContainer.GetFilters())
                    {
                        var filterParameters = filter.GetAllParameters();
                        Console.WriteLine(filterParameters.ToString());
                    }

                    //sub containers
                }
            }

        }
    }
}
