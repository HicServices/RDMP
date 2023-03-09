// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using SixLabors.ImageSharp;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Icons.IconOverlays;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.Icons.IconProvision.StateBasedIconProviders
{
    public class AggregateConfigurationStateBasedIconProvider : IObjectStateBasedIconProvider
    {
        private readonly IconOverlayProvider _overlayProvider;
        private readonly Image<Rgba32> _cohortAggregates;
        private readonly Image<Rgba32> _aggregates;
        private readonly Image<Rgba32> _patientIndexTable;

        public AggregateConfigurationStateBasedIconProvider(IconOverlayProvider overlayProvider)
        {
            _overlayProvider = overlayProvider;
            _cohortAggregates = Image.Load<Rgba32>(CatalogueIcons.CohortAggregate);
            _aggregates = Image.Load<Rgba32>(CatalogueIcons.AggregateGraph);
            _patientIndexTable = Image.Load<Rgba32>(CatalogueIcons.PatientIndexTable);
        }

        public Image<Rgba32> GetImageIfSupportedObject(object o)
        {
            if (o is Type && o.Equals(typeof (AggregateConfiguration)))
                return _aggregates;

            if (o is not AggregateConfiguration ac)
                return null;
            
            var img = ac.IsCohortIdentificationAggregate ? _cohortAggregates : _aggregates;

            if (ac.IsJoinablePatientIndexTable())
                img = _patientIndexTable;

            if (ac.IsExtractable)
                img = _overlayProvider.GetOverlay(img, OverlayKind.Extractable);

            if (ac.OverrideFiltersByUsingParentAggregateConfigurationInstead_ID != null)
                img =_overlayProvider.GetOverlay(img, OverlayKind.Shortcut);

            if (ac.Catalogue.IsApiCall())
                img = _overlayProvider.GetOverlay(img, OverlayKind.Cloud);

            return img;
        }
    }
}