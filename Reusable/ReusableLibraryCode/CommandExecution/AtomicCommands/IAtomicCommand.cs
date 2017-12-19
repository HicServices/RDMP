using System.Drawing;
using ReusableLibraryCode.Icons.IconProvision;

namespace ReusableLibraryCode.CommandExecution.AtomicCommands
{
    public interface IAtomicCommand : ICommandExecution
    {
        Image GetImage(IIconProvider iconProvider);
    }
}