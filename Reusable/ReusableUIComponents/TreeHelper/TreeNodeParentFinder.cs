using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrightIdeasSoftware;

namespace ReusableUIComponents.TreeHelper
{
    public class TreeNodeParentFinder
    {
        private TreeListView _tree;

        public TreeNodeParentFinder(TreeListView tree)
        {
            _tree = tree;
        }
        public T GetFirstOrNullParentRecursivelyOfType<T>(object modelObject) where T : class
        {
            //get parent of node
            var parent = _tree.GetParent(modelObject);

            //if there is no parent
            if (parent == null)
                return default(T);//return null
            
            //if parent is correct type return it
            var correctType = parent as T;
            if (correctType != null)
                return correctType;

            //otherwise explore upwards on parent to get parent of correct type
            return GetFirstOrNullParentRecursivelyOfType<T>(parent);
        }

        public T GetLastOrNullParentRecursivelyOfType<T>(object modelObject,T lastOneFound = null) where T:class
        {
            //get parent of node
            var parent = _tree.GetParent(modelObject);

            //if there are no parents
            if (parent == null)
                return lastOneFound;//return what we found (if any)

            //found a parent of the correct type
            var correctType = parent as T;
            if (correctType != null)
                lastOneFound = correctType;

            //but either way we need to look further up for the last one
            return GetLastOrNullParentRecursivelyOfType<T>(parent, lastOneFound);

        }
    }
}
