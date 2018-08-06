using CatalogueLibrary.Data.DataLoad;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.Copying.Commands
{
    public class ProcessTaskCommand : ICommand
    {
        public ProcessTask ProcessTask { get; set; }

        public ProcessTaskCommand(ProcessTask processTask)
        {
            ProcessTask = processTask;
        }

        public string GetSqlString()
        {
            return null;
        }
    }
}