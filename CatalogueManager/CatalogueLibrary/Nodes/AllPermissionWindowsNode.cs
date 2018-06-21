using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.Cohort;

namespace CatalogueLibrary.Nodes
{
    public class AllPermissionWindowsNode:SingletonNode,IOrderable
    {
        public AllPermissionWindowsNode() : base("Permission Windows")
        {
        }

        public int Order { get { return 0; } set{} }
    }
}
