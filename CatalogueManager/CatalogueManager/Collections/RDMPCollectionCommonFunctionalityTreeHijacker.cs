using System.Windows.Forms;
using BrightIdeasSoftware;

namespace CatalogueManager.Collections
{
    /// <summary>
    /// The Tree data model that is served by TreeFactoryGetter in RDMPCollectionCommonFunctionality.  Allows overriding of default TreeListView object model
    /// functionality e.g. sorting.
    /// </summary>
    internal class RDMPCollectionCommonFunctionalityTreeHijacker : TreeListView.Tree
    {
        private OLVColumn _lastSortColumn;
        private SortOrder _lastSortOrder;

        public RDMPCollectionCommonFunctionalityTreeHijacker(TreeListView treeView) : base(treeView)
        {
                
        }

        public override void Sort(OLVColumn column, SortOrder order)
        {
            _lastSortColumn = column;
            _lastSortOrder = order;

            base.Sort(column, order);

        }

        protected override TreeListView.BranchComparer GetBranchComparer()
        {
            if (_lastSortColumn == null)
                return null;

            return new TreeListView.BranchComparer(new OrderableComparer(_lastSortColumn, _lastSortOrder));
        }
    }
}