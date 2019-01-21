using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatalogueLibrary.Nodes.PipelineNodes
{
    /// <summary>
    /// Pipelines are sequences of tailorable components which achieve a given goal (e.g. load a cohort).  This node is a collection of all
    /// common use cases for pipelines.
    /// </summary>
    public class AllPipelinesNode:SingletonNode
    {
        public AllPipelinesNode() : base("Pipelines")
        {

        }
    }
}
