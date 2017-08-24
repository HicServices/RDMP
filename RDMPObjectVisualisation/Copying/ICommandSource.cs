using ReusableUIComponents.Copying;

namespace RDMPObjectVisualisation.Copying
{
    public interface ICommandSource
    {
        ICommand GetCommand();
    }
}