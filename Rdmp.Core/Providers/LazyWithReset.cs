using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Providers
{
    public class LazyWithReset<T>
    {
        private readonly Func<T> _valueFactory;
        private volatile Lazy<T> _lazy;

        public LazyWithReset(Func<T> valueFactory)
        {
            ArgumentNullException.ThrowIfNull(valueFactory);
            _valueFactory = valueFactory;
            _lazy = new(valueFactory);
        }

        public T Value => _lazy.Value;
        public bool IsValueCreated => _lazy.IsValueCreated;

        public void Reset() => _lazy = new(_valueFactory);
    }
}
