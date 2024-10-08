using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.DataHelper.RegexRedaction;
using Rdmp.Core.ReusableLibraryCode.Annotations;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SubComponents;

namespace Rdmp.UI.CommandExecution.Proposals
{
    internal class ProposeExecutionWhenTargetIsRegexRedactionConfiguration: RDMPCommandExecutionProposal<RegexRedactionConfiguration>
    {
        public ProposeExecutionWhenTargetIsRegexRedactionConfiguration(IActivateItems itemActivator) : base(
      itemActivator)
        {
        }

        public override bool CanActivate(RegexRedactionConfiguration target) => true;

        public override void Activate(RegexRedactionConfiguration target)
        {
            ItemActivator.Activate<RegexRedactionConfigurationUI, RegexRedactionConfiguration>(target);
        }

        [CanBeNull]
        public override ICommandExecution ProposeExecution(ICombineToMakeCommand cmd,
            RegexRedactionConfiguration target,
            InsertOption insertOption = InsertOption.Default) =>
            null;
    }
}
