using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Repositories;
using Rdmp.Core.Repositories.Construction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rdmp.Core.CommandExecution.AtomicCommands
{
    public class ExecuteCommandUnMergeCohortIdentificationConfiguration : BasicCommandExecution
    {
        private readonly CohortAggregateContainer _target;

        [UseWithObjectConstructor]
        public ExecuteCommandUnMergeCohortIdentificationConfiguration(IBasicActivateItems activator, CohortIdentificationConfiguration cic) :
            this(activator,cic?.RootCohortAggregateContainer)
        {

        }
        public ExecuteCommandUnMergeCohortIdentificationConfiguration(IBasicActivateItems activator,CohortAggregateContainer container): base(activator)
        {
            _target = container;
            
            if(_target == null)
            {
                SetImpossible("No root container");
                return;
            }
            
            if(!_target.IsRootContainer())
            { 
                SetImpossible("Only root containers can be unmerged");
                return;
            }

            if(_target.GetAggregateConfigurations().Any())
            { 
                SetImpossible("Container must contain only subcontainers (i.e. no aggregate sets)");
                return;
            }

            if(_target.GetSubContainers().Count() <= 1)
            { 
                SetImpossible("Container must have 2 or more immediate subcontainers for unmerging");
                return;
            }
        }

        public override void Execute()
        {
            base.Execute();

            var merger = new CohortIdentificationConfigurationMerger((CatalogueRepository)BasicActivator.RepositoryLocator.CatalogueRepository);
            var results = merger.UnMerge(_target);

            if(results != null && results.Any())
            {
                BasicActivator.Show($"Created {results.Length} new configurations:{Environment.NewLine} {string.Join(Environment.NewLine,results.Select(r=>r.Name))}");
                Publish(results.First());
                Emphasise(results.First());
            }
        }
    }
}
