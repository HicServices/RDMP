using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueManager.CommandExecution.Proposals;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Data;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace DataExportManager.CommandExecution.Proposals
{
    class ProposeExecutionWhenTargetIsProjectCohortIdentificationConfigurationAssociation : RDMPCommandExecutionProposal<ProjectCohortIdentificationConfigurationAssociation>
    {
        public ProposeExecutionWhenTargetIsProjectCohortIdentificationConfigurationAssociation(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override bool CanActivate(ProjectCohortIdentificationConfigurationAssociation target)
        {
            return true;
        }

        public override void Activate(ProjectCohortIdentificationConfigurationAssociation target)
        {
            //this class proxies for cic so just activate that instead
            ItemActivator.CommandExecutionFactory.Activate(target.CohortIdentificationConfiguration);
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, ProjectCohortIdentificationConfigurationAssociation target,InsertOption insertOption = InsertOption.Default)
        {
            //nothing can be dropped on this node
            return null;
        }
    }
}
