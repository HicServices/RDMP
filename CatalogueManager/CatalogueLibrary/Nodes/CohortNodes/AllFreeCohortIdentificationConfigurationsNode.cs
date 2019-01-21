namespace CatalogueLibrary.Nodes.CohortNodes
{
    /// <summary>
    /// Collection of all the cohort building queries (CohortIdentificationConfiguration) you have built which are not associated with
    /// a project yet.  These might be reusable template configurations (for cloning) or just cohort queries which have not been 
    /// executed/committed to a project yet.
    /// </summary>
    public class AllFreeCohortIdentificationConfigurationsNode : SingletonNode
    {
        public AllFreeCohortIdentificationConfigurationsNode()
            : base("Free Configurations")
        {

        }
    }
}