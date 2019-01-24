using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.DataLoad;

namespace CatalogueLibrary.Nodes
{
    /// <summary>
    /// Collection of all ANOTable objects.  These define how column level anonymisation happens in the data load engine
    /// </summary>
    public class AllANOTablesNode:SingletonNode
    {
        public AllANOTablesNode():base("Anonymisation Tables")
        {
            
        }
    }
}
