using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Automation;
using CatalogueLibrary.Data.Remoting;
using CatalogueManager.AggregationUIs.Advanced;
using CatalogueManager.ItemActivation;
using CatalogueManager.SimpleDialogs.Automation;
using CatalogueManager.SimpleDialogs.Remoting;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution.Proposals
{
    public class ProposeExecutionWhenTargetIsRemoteRDMP:RDMPCommandExecutionProposal<RemoteRDMP>
    {
        public ProposeExecutionWhenTargetIsRemoteRDMP(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override bool CanActivate(RemoteRDMP target)
        {
            return true;
        }

        public override void Activate(RemoteRDMP target)
        {
            ItemActivator.Activate<RemoteRDMPUI, RemoteRDMP>(target);
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, RemoteRDMP targetAggregateConfiguration, InsertOption insertOption = InsertOption.Default)
        {
            return null;
        }
    }
}
