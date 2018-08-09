using CatalogueLibrary.Data.Pipelines;
using ReusableLibraryCode.CommandExecution;

namespace CatalogueManager.Copying.Commands
{
    public class PipelineCommand : ICommand
    {
        public Pipeline Pipeline { get; private set; }
        public bool IsEmpty { get; private set; }

        public PipelineCommand(Pipeline pipeline)
        {
            Pipeline = pipeline;

            IsEmpty = Pipeline.PipelineComponents.Count == 0;
        }

        public string GetSqlString()
        {
            return "";
        }
    }
}