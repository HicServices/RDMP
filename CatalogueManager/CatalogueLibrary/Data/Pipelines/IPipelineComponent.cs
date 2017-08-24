using System;
using System.Collections.Generic;
using CatalogueLibrary.Data.DataLoad;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Data.Pipelines
{
    public interface IPipelineComponent : IArgumentHost, ISaveable, IDeleteable, IMapsDirectlyToDatabaseTable
    {
        string Name { get; set; }
        int Order { get; set; }
        int Pipeline_ID { get; set; }
        string Class { get; set; }
        IEnumerable<IPipelineComponentArgument> PipelineComponentArguments { get; }

        Type GetClassAsSystemType();
        string GetClassNameLastPart();
        IEnumerable<PipelineComponentArgument> CreateArgumentsForClassIfNotExists<T>();
        PipelineComponent Clone(Pipeline intoTargetPipeline);
    }
}