using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogueLibrary.Triggers.Exceptions
{
    /// <summary>
    /// Exception describing a problem with a backup trigger <see cref="ITriggerImplementer"/> or a problem that prevents one being created etc.
    /// </summary>
    public class TriggerException:Exception
    {
        public TriggerException(string str) : base(str)
        {
            
        }
    }
}
