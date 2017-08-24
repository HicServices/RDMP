using CatalogueLibrary.Data;
using HIC.Logging;

namespace CatalogueLibrary.DataFlowPipeline.Requirements
{
    public class DataFlowPipelineContextFactory<T>
    {
        public DataFlowPipelineContext<T> Create(PipelineUsage flags)
        {
            DataFlowPipelineContext<T> toReturn = new DataFlowPipelineContext<T>();

            //context has a fixed destination so we cannot allow any alternate destinations to sneak in
            if (flags.HasFlag(PipelineUsage.FixedDestination))
            {
                toReturn.MustHaveDestination = null;
                toReturn.CannotHave.Add(typeof(IDataFlowDestination<T>));
            }
            else
                toReturn.MustHaveDestination = typeof(IDataFlowDestination<T>);//context does not have a fixed destination so the pipeline configuration must specify the destination itself

            if (!flags.HasFlag(PipelineUsage.LoadsSingleTableInfo))
                toReturn.CannotHave.Add(typeof(IPipelineRequirement<TableInfo>));

            if (!flags.HasFlag(PipelineUsage.LogsToTableLoadInfo))
                toReturn.CannotHave.Add(typeof(IPipelineRequirement<TableLoadInfo>));

            if (flags.HasFlag(PipelineUsage.LoadsSingleFlatFile))
                toReturn.MustHaveSource = typeof (IPipelineRequirement<FlatFileToLoad>);

            return toReturn;
        }

    }
}