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

        public CatalogueCollectionFilter(ICoreChildProvider childProvider,bool isInternal, bool isDeprecated, bool isColdStorage)
        {
            _childProvider = childProvider;
            _isInternal = isInternal;
            _isDeprecated = isDeprecated;
            _isColdStorage = isColdStorage;
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
            
            //it has hiding flags, show only if one of the true flags matches the checkbox (doesn't need to match on all if a dataset is internal and deprecated then it shows if either is ticked)
            if(cata.IsInternalDataset || cata.IsColdStorageDataset || cata.IsDeprecated)
                return 
                    (_isColdStorage && _isColdStorage == cata.IsColdStorageDataset) ||
                    (_isDeprecated && _isDeprecated == cata.IsDeprecated) ||
                    (_isInternal && _isInternal == cata.IsInternalDataset);
            
            return true;
        }
    }
}