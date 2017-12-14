using System.Collections.Generic;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data.DataLoad
{
    /// <summary>
    /// A single component in a Data Load Engine configuration (See ProcessTask)
    /// </summary>
    public interface IProcessTask:IMapsDirectlyToDatabaseTable
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