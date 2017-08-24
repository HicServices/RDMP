using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogueManager.Icons.IconProvision.Exceptions
{
    public class IconProvisionException:Exception
    {
        public IconProvisionException(string msg) : base(msg)
        {
            
        }

        public IconProvisionException(string msg, Exception ex):base(msg,ex)
        {
            
        }
    }
}
