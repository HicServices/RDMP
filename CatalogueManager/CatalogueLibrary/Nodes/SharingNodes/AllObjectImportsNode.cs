using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogueLibrary.Nodes.SharingNodes
{
    /// <summary>
    /// Collection of ObjectImport records which document RDMP metadata objects which you have imported (from another RDMP instance).
    /// </summary>
    public class AllObjectImportsNode:SingletonNode
    {
        public AllObjectImportsNode():base("All Imports")
        {
            
        }
    }
}
