using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;

namespace CatalogueLibrary.Providers
{
    public class CatalogueProblemProvider : IProblemProvider
    {
        private ICoreChildProvider _childProvider;
        private HashSet<int> _orphanCatalogueItems = new HashSet<int>();

        public void RefreshProblems(ICoreChildProvider childProvider)
        {
            _childProvider = childProvider;

            //Take all the catalogue items which DONT have an associated ColumnInfo (should hopefully be quite rare)
            var orphans = _childProvider.AllCatalogueItems.Where(ci => ci.ColumnInfo_ID == null);
            
            //now identify those which have an ExtractionInformation (that's a problem! they are extractable but orphaned)
            _orphanCatalogueItems = new HashSet<int>(
                orphans.Where(o => _childProvider.AllExtractionInformations.Any(ei => ei.CatalogueItem_ID == o.ID))

                //store just the ID for performance
                .Select(i=>i.ID));
            
        }

        public bool HasProblem(object o)
        {
            return !string.IsNullOrWhiteSpace(DescribeProblem(o));
        }

        public string DescribeProblem(object o)
        {
            var ci = o as CatalogueItem;
            if (ci != null)
                return DescribeProblem((CatalogueItem) o);

            return null;
        }

        public string DescribeProblem(CatalogueItem catalogueItem)
        {
            if (_orphanCatalogueItems.Contains(catalogueItem.ID))
                return "CatalogueItem is extractable but has no associated ColumnInfo";

            return null;
        }
    }
}
