using System;

namespace MapsDirectlyToDatabaseTable.Injection
{
    /// <summary>
    /// Captures the current cached value of an object of Type T including whether it has been fetched (incase it is null).  Use GetValueIfKnownOrRun
    /// to read the value and implicitly define how to obtain the instance of T in the event that it isn't cached yet.  The InjectedValue should be immutable
    /// and deterministic for the instance declaration e.g. if you declare an InjectedValue of type string then every call to GetValueIfKnownOrRun should 
    /// return the same value for the same host class instance.
    /// </summary>
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
    /// <seealso cref="IInjectKnown{T}"/>
    /// <typeparam name="T"></typeparam>
    public class InjectedValue<T>
    {
        private T _value;

        /// <summary>
        /// True if the Value is known to be it's current state, even if it's value is null
        /// </summary>
        public bool ValueKnown { get; set; }

        /// <summary>
        /// Defines a new as yet uncached InjectedValue.  Use this on construction so that you can call <see cref="GetValueIfKnownOrRun"/> at any
        /// time without an error while still also supporting overwritting with <seealso cref="IInjectKnown{T}.InjectKnown"/>.
        /// </summary>
        public InjectedValue()
        {
            ValueKnown = false;
        }

        /// <summary>
        /// Records that the value T is the definitive deterministic answer to any call ever to GetValueIfKnownOrRun for the current instance.
        /// </summary>
        /// <param name="value"></param>
        public InjectedValue(T value)
        {
            _value = value;
            ValueKnown = true;
        }
        
        /// <summary>
        /// Clears the known answer to T
        /// </summary>
        public void Clear()
        {
            _value = default(T);
            ValueKnown = false;
        }

        /// <summary>
        /// Returns the known value of T or uses the supplied method to fetch it.  This should be deterministic (always return the same T no matter who calls
        /// GetValueIfKnownOrRun.
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public T GetValueIfKnownOrRun(Func<T> func)
        {
            if (!ValueKnown)
            {
                _value = func();
                ValueKnown = true;
            }

            return _value;
        }
    }
}