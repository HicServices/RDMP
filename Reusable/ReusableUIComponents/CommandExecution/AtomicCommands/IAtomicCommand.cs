using System.Drawing;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.Icons.IconProvision;

namespace ReusableUIComponents.CommandExecution.AtomicCommands
{
    public interface IAtomicCommand : ICommandExecution
    {
        Image GetImage(IIconProvider iconProvider);
    }
}