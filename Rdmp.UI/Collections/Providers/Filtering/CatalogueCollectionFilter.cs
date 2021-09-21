// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using BrightIdeasSoftware;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Providers;
using ReusableLibraryCode.Settings;

namespace Rdmp.UI.Collections.Providers.Filtering
{
    /// <summary>
    /// Filters objects in a <see cref="CatalogueCollectionUI"/> based on whether the <see cref="Catalogue"/> is marked
    /// with various flags (e.g. <see cref="Catalogue.IsDeprecated"/>
    /// </summary>
    public class CatalogueCollectionFilter : IModelFilter
    {
        public ICoreChildProvider ChildProvider { get; set; }
        private readonly bool _isInternal;
        private readonly bool _isDeprecated;
        private readonly bool _isColdStorage;
        private readonly bool _isProjectSpecific;
        private readonly bool _isNonExtractable;

        public CatalogueCollectionFilter(ICoreChildProvider childProvider)
        {
            ChildProvider = childProvider;
            _isInternal = UserSettings.ShowInternalCatalogues;
            _isDeprecated = UserSettings.ShowDeprecatedCatalogues;
            _isColdStorage = UserSettings.ShowColdStorageCatalogues;
            _isProjectSpecific = UserSettings.ShowProjectSpecificCatalogues;
            _isNonExtractable = UserSettings.ShowNonExtractableCatalogues;
        }

        public bool Filter(object modelObject)
        {
            var cata = modelObject as ICatalogue;
            
            //doesn't relate to us... 
            if (cata == null)
            {
                // or are we one of these things that can be tied to a catalogue
                if (modelObject is ExtractableDataSet eds)
                {
                    cata = eds.Catalogue;
                }
                else
                if (modelObject is SelectedDataSets sds)
                {
                    cata = sds.GetCatalogue();
                }
                else
                {
                    // but maybe we are descended from a Catalogue?
                    var descendancy = ChildProvider.GetDescendancyListIfAnyFor(modelObject);
                    if (descendancy != null)
                        cata = descendancy.Parents.OfType<Catalogue>().SingleOrDefault();
                }

                if (cata == null)
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