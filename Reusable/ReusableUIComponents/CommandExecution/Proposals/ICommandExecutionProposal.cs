using System;
using System.ComponentModel.Composition;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.Annotations;

namespace ReusableUIComponents.CommandExecution.Proposals
{
    [InheritedExport(typeof(ICommandExecutionProposal))]
    public interface ICommandExecutionProposal
    {
        bool IsCompatibleTarget(object target);
        ICommandExecution ProposeExecution(ICommand cmd, object target, InsertOption insertOption = InsertOption.Default);

        bool CanActivate(object target);
        void Activate(object target);
    }
}
