using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data.Datasets;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SubComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.UI.CommandExecution.Proposals
{
    public class ProposeExecutionWhenTargetIsDatasetProviderConfiguration : RDMPCommandExecutionProposal<DatasetProviderConfiguration>
    {
        public ProposeExecutionWhenTargetIsDatasetProviderConfiguration(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override void Activate(DatasetProviderConfiguration target)
        {
            ItemActivator.Activate<DatasetProviderConfigurationUI, DatasetProviderConfiguration>(target);
        }

        public override bool CanActivate(DatasetProviderConfiguration target)
        {
            return true;
        }

        public override ICommandExecution ProposeExecution(ICombineToMakeCommand cmd, DatasetProviderConfiguration target, InsertOption insertOption = InsertOption.Default)
        {
            return null;
        }
    }
}
