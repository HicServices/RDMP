using System.Collections;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary.Data.Cohort;

namespace CatalogueManager.Collections
{
    /// <summary>
    /// Compares model objects bearing in mind that anything that is compared against IOrderable MUST come in that order.  This class is a wrapper for 
    /// ModelObjectComparer that looks out for IOrderable objects passing through and enforces that order.
    /// 
    /// This class is designed to modify the behaviour of TreeListView to ensure that despite the users worst efforts, the order of IOrderable nodes is always
    /// Ascending
    /// </summary>
    public class OrderableComparer : IComparer
    {
        private readonly ModelObjectComparer _modelComparer;

        public OrderableComparer(OLVColumn primarySortColumn, SortOrder primarySortOrder)
        {
            _modelComparer = new ModelObjectComparer(primarySortColumn, primarySortOrder);
        }

        public int Compare(object x, object y)
        {
            var explicitOrderX = GetExplicitOrder(x);
            var explicitOrderY = GetExplicitOrder(y);

            if (explicitOrderX != explicitOrderY)
                return explicitOrderX - explicitOrderY;


            return _modelComparer.Compare(x, y);
        }

        private int GetExplicitOrder(object o)
        {
            var orderable = o as IOrderable;

            return orderable != null ? orderable.Order : -1;
        }
    }
}