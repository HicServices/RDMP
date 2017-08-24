namespace CatalogueLibrary.DataFlowPipeline
{
    /// <summary>
    /// Functions like a normal IDataFlowComponent except you should always return null from T ProcessPipelineData, allows the component to be used as the final component in a 
    /// DataFlowPipelineContext
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDataFlowDestination<T> : IDataFlowComponent<T>
    {
        
    }
}
