using ReusableUIComponents.CommandExecution;

namespace RDMPObjectVisualisation.Copying
{
    public interface ICommandSource
    {
        ICommand GetCommand();
    }
}