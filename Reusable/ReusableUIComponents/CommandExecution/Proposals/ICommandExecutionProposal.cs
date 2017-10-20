using System.ComponentModel.Composition;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.Annotations;

namespace ReusableUIComponents.CommandExecution.Proposals
{
    [InheritedExport(typeof(ICommandExecutionProposal))]
    public interface ICommandExecutionProposal
    {
        ICommandExecution ProposeExecution(ICommand cmd, object target, InsertOption insertOption = InsertOption.Default);
    }
}
