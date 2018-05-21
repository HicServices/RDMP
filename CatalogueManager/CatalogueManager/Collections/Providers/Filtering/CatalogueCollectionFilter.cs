using System.Linq;
using BrightIdeasSoftware;
using CatalogueLibrary.Data;
using CatalogueLibrary.Providers;

namespace CatalogueManager.Collections.Providers.Filtering
{
    public class CatalogueCollectionFilter : IModelFilter
    {
        private readonly ICoreChildProvider _childProvider;
        private readonly bool _isInternal;
        private readonly bool _isDeprecated;
        private readonly bool _isColdStorage;
        private readonly bool _isProjectSpecific;

        public CatalogueCollectionFilter(ICoreChildProvider childProvider, bool isInternal, bool isDeprecated, bool isColdStorage, bool isProjectSpecific)
        {
            _childProvider = childProvider;
            _isInternal = isInternal;
            _isDeprecated = isDeprecated;
            _isColdStorage = isColdStorage;
            _isProjectSpecific = isProjectSpecific;
        }

        public bool Filter(object modelObject)
        {
            var cata = modelObject as Catalogue;
            
            //doesn't relate to us... but maybe we are descended from a Catalogue?
            if (cata == null)
            {
                var descendancy = _childProvider.GetDescendancyListIfAnyFor(modelObject);
                if (descendancy != null)
                    cata = descendancy.Parents.OfType<Catalogue>().SingleOrDefault();

                if(cata == null)
                    return true;
            }

            bool isProjectSpecific = cata.IsProjectSpecific(null);
            
            //it has hiding flags, show only if one of the true flags matches the checkbox (doesn't need to match on all if a dataset is internal and deprecated then it shows if either is ticked)
            if (cata.IsInternalDataset || cata.IsColdStorageDataset || cata.IsDeprecated || isProjectSpecific)
                return 
                    (_isColdStorage && _isColdStorage == cata.IsColdStorageDataset) ||
                    (_isDeprecated && _isDeprecated == cata.IsDeprecated) ||
                    (_isInternal && _isInternal == cata.IsInternalDataset) ||
                    (_isProjectSpecific && _isProjectSpecific == isProjectSpecific);
            
            return true;
        }
    }
}