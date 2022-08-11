// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using SixLabors.ImageSharp;
using Rdmp.Core.DataExport.DataRelease.Potential;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Ticketing;

namespace Rdmp.Core.Icons.IconProvision.StateBasedIconProviders
{
    public class ReleaseabilityStateBasedIconProvider : IObjectStateBasedIconProvider
    {
        private readonly Dictionary<Releaseability,Image> _images = new Dictionary<Releaseability, Image>();
        private readonly Dictionary<TicketingReleaseabilityEvaluation, Image> _environmentImages = new Dictionary<TicketingReleaseabilityEvaluation, Image>();

        public ReleaseabilityStateBasedIconProvider()
        {
            _images.Add(Releaseability.Undefined,CatalogueIcons.TinyRed);

            _images.Add(Releaseability.ExceptionOccurredWhileEvaluatingReleaseability, CatalogueIcons.TinyRed);
            _images.Add(Releaseability.NeverBeenSuccessfullyExecuted, CatalogueIcons.Failed);
            _images.Add(Releaseability.ExtractFilesMissing, CatalogueIcons.FileMissing);
            _images.Add(Releaseability.ExtractionSQLDesynchronisation, CatalogueIcons.Diff);
            _images.Add(Releaseability.CohortDesynchronisation, CatalogueIcons.Failed);
            _images.Add(Releaseability.ColumnDifferencesVsCatalogue, CatalogueIcons.TinyYellow);
            _images.Add(Releaseability.Releaseable, CatalogueIcons.TinyGreen);

            _environmentImages.Add(TicketingReleaseabilityEvaluation.CouldNotAuthenticateAgainstServer, CatalogueIcons.TinyRed);
            _environmentImages.Add(TicketingReleaseabilityEvaluation.CouldNotReachTicketingServer, CatalogueIcons.TinyRed);
            _environmentImages.Add(TicketingReleaseabilityEvaluation.NotReleaseable, CatalogueIcons.TinyRed);
            _environmentImages.Add(TicketingReleaseabilityEvaluation.Releaseable, CatalogueIcons.TinyGreen);
            _environmentImages.Add(TicketingReleaseabilityEvaluation.TicketingLibraryCrashed, CatalogueIcons.TinyRed);
            _environmentImages.Add(TicketingReleaseabilityEvaluation.TicketingLibraryMissingOrNotConfiguredCorrectly, CatalogueIcons.TinyYellow);
        }

        public Image GetImageIfSupportedObject(object o)
        {
            if (o is Releaseability) 
                return _images[(Releaseability) o];

            if (o is TicketingReleaseabilityEvaluation)
                return _environmentImages[(TicketingReleaseabilityEvaluation)o];

            return null;
        }
    }
}