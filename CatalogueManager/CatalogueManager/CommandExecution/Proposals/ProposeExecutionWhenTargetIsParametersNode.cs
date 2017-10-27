using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Nodes;
using CatalogueManager.ExtractionUIs.FilterUIs.ParameterUIs;
using CatalogueManager.ExtractionUIs.FilterUIs.ParameterUIs.Options;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution.Proposals
{
    public class ProposeExecutionWhenTargetIsParametersNode:RDMPCommandExecutionProposal<ParametersNode>
    {
        public ProposeExecutionWhenTargetIsParametersNode(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override bool CanActivate(ParametersNode target)
        {
            return true;
        }

        public override void Activate(ParametersNode target)
        {
            var parameterCollectionUI = new ParameterCollectionUI();

            ParameterCollectionUIOptionsFactory factory = new ParameterCollectionUIOptionsFactory();
            var options = factory.Create(target.Collector);
            parameterCollectionUI.SetUp(options);

            ItemActivator.ShowWindow(parameterCollectionUI, true);
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, ParametersNode target, InsertOption insertOption = InsertOption.Default)
        {
            return null;
        }
    }
}
