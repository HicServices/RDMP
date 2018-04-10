using System;

namespace MapsDirectlyToDatabaseTable.Injection
{
    /// <summary>
    /// Defines that the implementing class has an expensive operation for fetching a T but that a known instance might already be
    /// available (e.g. in a cache) which can be injected into it. 
    /// </summary>
    /// <example>
    /// <code>
    /// public class Bob:IInjectKnown&lt;byte[]&gt;
    /// {
    ///     private Lazy&lt;byte[]&gt; _knownBytes;
    /// 
    ///     public Bob()
    ///     {
    ///         ClearAllInjections();   
    ///     }
    /// 
    ///     public void InjectKnown(byte[] instance)
    ///     {
    ///         _knownBytes = new Lazy&lt;byte[]&gt;(()=>instance);
    ///     }
    /// 
    ///     public void ClearAllInjections()
    ///     {
    ///         _knownBytes = new Lazy&lt;byte[]&gt;(FetchBytesExpensive);
    ///     }
    /// 
    ///     private byte[] FetchBytesExpensive()
    ///     {
    ///         return new byte[10000];
    ///     }
    /// }
    /// 
    /// </code></example>
    /// <typeparam name="T"></typeparam>
    public interface IInjectKnown<T>
    {
        /// <summary>
        /// Records the known state of T.
        /// </summary>
        /// <param name="instance"></param>
        void InjectKnown(T instance);

        /// <summary>
        /// Informs the implementing class that it should forget about all values provided by any InjectKnown calls
        /// </summary>
        void ClearAllInjections();
    }
}