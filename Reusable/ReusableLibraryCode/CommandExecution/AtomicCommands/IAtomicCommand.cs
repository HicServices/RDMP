using System.Drawing;
using ReusableLibraryCode.Icons.IconProvision;

namespace ReusableLibraryCode.CommandExecution.AtomicCommands
{
    /// <summary>
    /// ICommandExecution with an Image designed for use with MenuItems and HomeUI  
    /// </summary>
    public interface IAtomicCommand : ICommandExecution
    {
        Image GetImage(IIconProvider iconProvider);
    }
}