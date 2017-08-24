using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapsDirectlyToDatabaseTable.RepositoryResultCaching
{
    internal class SuperCachingDisposalToken :IDisposable
    {
        private Action _turnOffMethod;

        public SuperCachingDisposalToken(Action turnOffMethod)
        {
            _turnOffMethod = turnOffMethod;
        }

        public void Dispose()
        {
            _turnOffMethod();
        }
    }
}
