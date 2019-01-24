using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogueLibrary.Nodes.SharingNodes
{
    /// <summary>
    /// Collection of ObjectExport records which document RDMP metadata objects which you have exported (ready for sharing with another RDMP user).
    /// </summary>
    public class AllObjectExportsNode:SingletonNode
    {
        public AllObjectExportsNode() : base("All Exports")
        {
        }
    }
}
