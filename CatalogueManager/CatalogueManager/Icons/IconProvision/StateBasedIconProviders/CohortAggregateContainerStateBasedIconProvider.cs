using System;
using System.Drawing;
using CatalogueLibrary.Data.Cohort;

namespace CatalogueManager.Icons.IconProvision.StateBasedIconProviders
{
    public class CohortAggregateContainerStateBasedIconProvider : IObjectStateBasedIconProvider
    {
        private Bitmap _union;
        private Bitmap _intersect;
        private Bitmap _except;

        public CohortAggregateContainerStateBasedIconProvider()
        {
            _union = CatalogueIcons.UNION;
            _intersect = CatalogueIcons.INTERSECT;
            _except = CatalogueIcons.EXCEPT;            
        }
        public Bitmap GetImageIfSupportedObject(object o)
        {
            if (o is Type && o.Equals(typeof (CohortAggregateContainer)))
                return _intersect;

            if (o is SetOperation)
                return GetImage((SetOperation) o);

            var container = o as CohortAggregateContainer;
            
            if (container == null)
                return null;

            return GetImage(container.Operation);
        }

        private Bitmap GetImage(SetOperation operation)
        {
            switch (operation)
            {
                case SetOperation.UNION:
                    return _union;
                case SetOperation.INTERSECT:
                    return _intersect;
                case SetOperation.EXCEPT:
                    return _except;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}