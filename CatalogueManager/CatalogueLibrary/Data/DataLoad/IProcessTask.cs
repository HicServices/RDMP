using System.Collections.Generic;

namespace CatalogueLibrary.Data.DataLoad
{
    public interface IProcessTask
    {
        int Order { get; }
        string Path { get; }
        string Name { get; }
        LoadStage LoadStage { get; }
        ProcessTaskType ProcessTaskType { get; }
        int? RelatesSolelyToCatalogue_ID { get; }
        
        IEnumerable<IArgument> GetAllArguments();
    }
}