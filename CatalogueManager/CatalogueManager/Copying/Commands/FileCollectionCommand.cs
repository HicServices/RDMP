using System.IO;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.Copying.Commands
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