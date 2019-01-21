using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogueLibrary.Nodes.PipelineNodes
{
    /// <summary>
    /// Pipelines are sequences of tailorable components which achieve a given goal (e.g. load a cohort).  This node is a collection of all
    /// pipelines for which are not compatible with core RDMP use cases (these might be broken or designed for plugin code / custom goals - 
    /// e.g. loading imaging data).
    /// </summary>
    public class OtherPipelinesNode:SingletonNode
    {
        public OtherPipelinesNode():base("Other Pipelines")
        {
            
        }
    }
}
