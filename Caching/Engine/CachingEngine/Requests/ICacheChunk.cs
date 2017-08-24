using System;

namespace CachingEngine.Requests
{
    /// <summary>
    /// Base interface for templated Cache pipelines, you should inherit from this class and add whatever properties you will use in your data classes e.g FileInfo[] property if your
    /// cache involves downloading lots of files to fulfil the request
    /// </summary>
    public interface ICacheChunk
    {
        ICacheFetchRequest Request { get; }
    }
}