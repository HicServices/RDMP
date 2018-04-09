namespace MapsDirectlyToDatabaseTable.Injection
{
    /// <summary>
    /// Defines that the implementing class has an expensive operation for fetching a T but that a known instance might already be
    /// available (e.g. in a cache) which can be injected into it. 
    /// <example><code>
    /// public class Bob : IInjectKnown&lt;byte[]&gt;
    /// {
    ///     private InjectedValue&lt;byte[]> _bytes = new InjectedValue&lt;byte[]&gt;();
    /// 
    ///     public byte[] GetBytes()
    ///     {
    ///         return _bytes.GetValueIfKnownOrRun(ExpensiveOperation);
    ///     }
    /// 
    ///     public void InjectKnown(InjectedValue&lt;byte[]&gt; instance)
    ///     {
    ///         _bytes = instance;
    ///     }
    /// 
    ///     private byte[] ExpensiveOperation()
    ///     {
    ///         return new byte[100232];
    ///     }
    /// }
    /// </code></example>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IInjectKnown<T>
    {
        /// <summary>
        /// Records the known state of T 
        /// </summary>
        /// <param name="instance"></param>
        void InjectKnown(InjectedValue<T> instance);
    }
}