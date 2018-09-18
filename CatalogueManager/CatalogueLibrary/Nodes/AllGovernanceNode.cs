using CatalogueLibrary.Data.Cohort;

namespace CatalogueLibrary.Nodes
{
    /// <summary>
    /// The RDMP is designed to store sensitive clinical datasets and make them available in research ready (anonymous) form.  This usually requires governance approval from the data
    /// provider.  This node lets you create periods of governance for your datasets (See GovernancePeriodUI).
    /// </summary>
    public class AllGovernanceNode:SingletonNode,IOrderable
    {
        public int Order { get { return -5000; }set{}}

        public AllGovernanceNode() : base("Governance")
        {
        }
    }
}
