using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    public class ExecuteCommandMergeCohortIdentificationConfigurations : BasicCommandExecution
    {
        public CohortIdentificationConfiguration[] ToMerge { get; }

        public ExecuteCommandMergeCohortIdentificationConfigurations(IBasicActivateItems activator,CohortIdentificationConfiguration[] toMerge):base(activator)
        {
            ToMerge = toMerge;
        }

        public override void Execute()
        {
            base.Execute();

            var toMerge = ToMerge;

            if(toMerge == null || toMerge.Length <= 1)
            {
                if(!SelectMany(BasicActivator.RepositoryLocator.CatalogueRepository.GetAllObjects<CohortIdentificationConfiguration>(),out toMerge))
                    return;
            }

            if(toMerge == null || toMerge.Length <= 1)
            {
                BasicActivator.Show($"You must select at least 2 configurations to merge");
                return;
            }

            var merger = new CohortIdentificationConfigurationMerger((CatalogueRepository)BasicActivator.RepositoryLocator.CatalogueRepository);
            var result = merger.Merge(toMerge,SetOperation.UNION);

            if(result != null)
            { 
                BasicActivator.Show($"Succesfully created '{result}'");
                Publish(result);
                Emphasise(result);
            }
        }

    }
}
