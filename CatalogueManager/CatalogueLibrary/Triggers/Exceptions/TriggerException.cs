using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogueLibrary.Triggers.Exceptions
{
    public class TriggerException:Exception
    {
        public TriggerException(string str) : base(str)
        {
            
        }
    }
}
