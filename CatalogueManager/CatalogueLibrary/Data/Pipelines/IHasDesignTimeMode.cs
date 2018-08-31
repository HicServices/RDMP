namespace CatalogueLibrary.Data.Pipelines
{
    /// <summary>
    /// Interface for classes which are requirements of a Pipeline (e.g. the file you want to load) but which might not be available at design time
    /// (e.g. when the user wants to edit the 'BULK UPLOAD Files' pipeline).  Rather than making the user pick a file implement this interface and 
    /// provide a suitable static method for constructing the object  and mark it as IsDesignTime too.  
    /// 
    /// <para>PipelineComponents should check objects they are initialized with (See <see cref=" CatalogueLibrary.DataFlowPipeline.Requirements.IPipelineRequirement"/>)
    /// to see if they are <see cref="IHasDesignTimeMode"/> and have<see cref="IsDesignTime"/> before checking on them (e.g. checking a file exists on disk).</para>
    /// </summary>
    public interface IHasDesignTimeMode
    {
        /// <summary>
        /// True if the user is trying to edit a <see cref="Pipeline"/> independent of carrying out the task (i.e. no input objects have been selected).
        /// </summary>
        bool IsDesignTime { get; }
    }
}