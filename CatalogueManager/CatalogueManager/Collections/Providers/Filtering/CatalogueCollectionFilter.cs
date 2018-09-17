using System.Linq;
using BrightIdeasSoftware;
using CatalogueLibrary.Data;
using CatalogueLibrary.Providers;
using ReusableLibraryCode.Settings;

namespace CatalogueManager.Collections.Providers.Filtering
{
    public class CatalogueCollectionFilter : IModelFilter
    {
        private readonly ICoreChildProvider _childProvider;
        private readonly bool _isInternal;
        private readonly bool _isDeprecated;
        private readonly bool _isColdStorage;
        private readonly bool _isProjectSpecific;
        private readonly bool _isNonExtractable;

        public CatalogueCollectionFilter(ICoreChildProvider childProvider)
        {
            _childProvider = childProvider;
            _isInternal = UserSettings.ShowInternalCatalogues;
            _isDeprecated = UserSettings.ShowDeprecatedCatalogues;
            _isColdStorage = UserSettings.ShowColdStorageCatalogues;
            _isProjectSpecific = UserSettings.ShowProjectSpecificCatalogues;
            _isNonExtractable = UserSettings.ShowNonExtractableCatalogues;
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
            bool isExtractable = cata.GetExtractabilityStatus(null) != null && cata.GetExtractabilityStatus(null).IsExtractable;
            
            return ( isExtractable && !cata.IsColdStorageDataset && !cata.IsDeprecated && !cata.IsInternalDataset && !isProjectSpecific) ||
                    ((_isColdStorage && cata.IsColdStorageDataset) ||
                    (_isDeprecated && cata.IsDeprecated) ||
                    (_isInternal && cata.IsInternalDataset) ||
                    (_isProjectSpecific && isProjectSpecific) ||
                    (_isNonExtractable && !isExtractable));
        }
    }
}