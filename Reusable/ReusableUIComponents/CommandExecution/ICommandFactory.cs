using System.IO;
using System.Windows.Forms;
using BrightIdeasSoftware;

namespace ReusableUIComponents.CommandExecution
{
    public interface ICommandFactory
    {
        ICommand Create(OLVDataObject o);
        ICommand Create(ModelDropEventArgs e);
        ICommand Create(DragEventArgs e);
        ICommand Create(FileInfo[] files);
        ICommand Create(object modelObject);
        
    }
}
