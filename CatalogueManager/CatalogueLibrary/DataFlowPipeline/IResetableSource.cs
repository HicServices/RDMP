namespace CatalogueLibrary.DataFlowPipeline
{
    /// <summary>
    /// Usually when an IDataFlowSource reads from a source it will do so iteratively and possibly in a consumable mannner (removing source files or corrupting private variables).
    /// If a source implements this interface it has a contract to reset itself to the initial state and serve up the same content again.  The use case for this is for previewing
    /// pipelines at various stages before executing the full end to end pipeline for real.
    /// 
    /// e.g. typical functionality might include resetting recordsRead to 0, refreshing a DataTable from the remote server, rebuilding internal schemas etc.
    /// </summary>
    public interface IResetableSource
    {
        void Reset();
    }
}