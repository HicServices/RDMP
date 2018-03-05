using CatalogueLibrary.Data;
using HIC.Logging;

namespace CatalogueLibrary.DataFlowPipeline.Requirements
{
    /// <summary>
    /// Factory for constructing DataFlowPipelineContexts based on some handy presets.  Particularly helpful because of the wierd way we enforce FixedDestination
    /// (basically we forbid the IPipeline from having any IDataFlowDestination).  Feel free to adjust your context after the factory creates it.  This is very low
    /// level functionality you should only need it if you are trying to define a new IPipelineUseCase for an entirely novel kind of pipeline usage.
    /// </summary>
    /// <typeparam name="T"></typeparam>
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

            if (flags.HasFlag(PipelineUsage.FixedSource))
            {
                toReturn.MustHaveSource = null;
                toReturn.CannotHave.Add(typeof(IDataFlowSource<T>));
            }
            else
                toReturn.MustHaveSource = typeof(IDataFlowSource<T>);//context does not have a fixed source so the pipeline configuration must specify the source itself
            
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