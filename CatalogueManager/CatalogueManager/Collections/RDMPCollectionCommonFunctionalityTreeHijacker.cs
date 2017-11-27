using BrightIdeasSoftware;

namespace CatalogueManager.Collections
{
    /// <summary>
    /// The Tree data model that is served by TreeFactoryGetter in RDMPCollectionCommonFunctionality.  Allows overriding of default TreeListView object model
    /// functionality e.g. sorting.
    /// </summary>
    internal class RDMPCollectionCommonFunctionalityTreeHijacker : TreeListView.Tree
    {
        public RDMPCollectionCommonFunctionalityTreeHijacker(TreeListView treeView) : base(treeView)
        {
                
        }

        protected override TreeListView.BranchComparer GetBranchComparer()
        {
            if (TreeView.PrimarySortColumn == null)
                return null;

            return new TreeListView.BranchComparer(new OrderableComparer(TreeView.PrimarySortColumn,TreeView.PrimarySortOrder));
        }
    }
}