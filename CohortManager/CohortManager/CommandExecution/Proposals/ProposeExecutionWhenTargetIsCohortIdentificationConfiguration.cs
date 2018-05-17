using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.Cohort;
using CatalogueManager.CommandExecution.Proposals;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CohortManager.SubComponents;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CohortManager.CommandExecution.Proposals
{
    class ProposeExecutionWhenTargetIsCohortIdentificationConfiguration:RDMPCommandExecutionProposal<CohortIdentificationConfiguration>
    {
        public ProposeExecutionWhenTargetIsCohortIdentificationConfiguration(IActivateItems itemActivator) : base(itemActivator)
        {
        }
        
        public override bool CanActivate(CohortIdentificationConfiguration target)
        {
            return true;
        }

        public override void Activate(CohortIdentificationConfiguration target)
        {
            ItemActivator.Activate<CohortIdentificationConfigurationUI, CohortIdentificationConfiguration>(target);
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, CohortIdentificationConfiguration target,
            InsertOption insertOption = InsertOption.Default)
        {
            return null;
        }
    }
}
