using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogueLibrary.Nodes
{
    /// <summary>
    /// Folder Node that can be added to TreeListViews.  You can only add one folder of each name because they inherit from <see cref="SingletonNode"/>.
    /// </summary>
    public class ArbitraryFolderNode:SingletonNode
    {
        public ArbitraryFolderNode(string caption) : base(caption)
        {

        }
    }
}
