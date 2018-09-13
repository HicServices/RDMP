using System.IO;
using ReusableLibraryCode.CommandExecution;

namespace CatalogueManager.Copying.Commands
{
    public class FileCollectionCommand : ICommand
    {
        public FileInfo[] Files { get; set; }
        
        public bool IsShareDefinition { get; set; }
        

        public FileCollectionCommand(FileInfo[] files)
        {
            Files = files;
            IsShareDefinition = files.Length == 1 && files[0].Extension == ".sd";

        }
        
        public string GetSqlString()
        {
            return null;
        }
    }
}