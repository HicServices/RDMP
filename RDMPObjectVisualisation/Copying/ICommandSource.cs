using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace RDMPObjectVisualisation.Copying
{
    public interface ICommandSource
    {
        ICommand GetCommand();
    }
}