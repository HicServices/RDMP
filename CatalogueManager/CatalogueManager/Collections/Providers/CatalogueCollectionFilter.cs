using System;
using System.Collections.Generic;
using System.Linq;
using BrightIdeasSoftware;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.PerformanceImprovement;
using CatalogueLibrary.Nodes;
using CatalogueLibrary.Providers;
using CatalogueLibrary.Repositories;
using HIC.Common.Validation.Constraints.Primary;

namespace CatalogueManager.Collections.Providers
{
    public class CatalogueCollectionFilter : IModelFilter
    {
        private readonly ICoreChildProvider _coreChildProvider;
        private readonly bool _isInternal;
        private readonly bool _isDeprecated;
        private readonly bool _isColdStorage;

        public CatalogueCollectionFilter(ICoreChildProvider coreChildProvider, bool isInternal, bool isDeprecated, bool isColdStorage)
        {
            _coreChildProvider = coreChildProvider;
            _isInternal = isInternal;
            _isDeprecated = isDeprecated;
            _isColdStorage = isColdStorage;
        }

        public bool Filter(object modelObject)
        {
            var cata = modelObject as Catalogue;
            
            //do not show it if the flags don't match
            if(cata != null)
                if (! 
                    (cata.IsInternalDataset == _isInternal &&
                        cata.IsDeprecated == _isDeprecated &&
                        cata.IsColdStorageDataset == _isColdStorage)) //matching on one of the dataset flags
                    return false; //do not show it
            
            return true;
        }
    }
}