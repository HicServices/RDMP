using BrightIdeasSoftware;
using CatalogueLibrary.Data;
using CatalogueLibrary.Providers;

namespace CatalogueManager.Collections.Providers.Filtering
{
    public class CatalogueCollectionFilter : IModelFilter
    {
        private readonly bool _isInternal;
        private readonly bool _isDeprecated;
        private readonly bool _isColdStorage;

        public CatalogueCollectionFilter(bool isInternal, bool isDeprecated, bool isColdStorage)
        {
            _isInternal = isInternal;
            _isDeprecated = isDeprecated;
            _isColdStorage = isColdStorage;
        }

        public bool Filter(object modelObject)
        {
            var cata = modelObject as Catalogue;


            //doesn't relate to us anyway
            if (cata == null)
                return true;

            //do not show it if the flags don't match
            if(cata.IsInternalDataset)
                return _isInternal;

            if (cata.IsColdStorageDataset)
                return _isColdStorage;

            if (cata.IsDeprecated)
                return _isDeprecated;
            
            return true;
        }
    }
}