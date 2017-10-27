using System.IO;
using ReusableUIComponents.CommandExecution;

namespace RDMPObjectVisualisation.Copying.Commands
{
    public class FileCollectionCommand : ICommand
    {
        public FileInfo[] Files { get; set; }

        public FileCollectionCommand(FileInfo[] files)
        {
            Files = files;
        }

        public string GetSqlString()
        {
            return null;
        }
    }
}