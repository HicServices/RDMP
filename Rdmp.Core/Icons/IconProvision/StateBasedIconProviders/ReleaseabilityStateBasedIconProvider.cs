// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using Rdmp.Core.DataExport.DataRelease.Potential;
using Rdmp.Core.Ticketing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.Icons.IconProvision.StateBasedIconProviders;

public class ReleaseabilityStateBasedIconProvider : IObjectStateBasedIconProvider
{
    private readonly Dictionary<TicketingReleaseabilityEvaluation, Image<Rgba32>> _environmentImages;
    private readonly Dictionary<Releaseability, Image<Rgba32>> _images;

    public ReleaseabilityStateBasedIconProvider()
    {
        _images = new Dictionary<Releaseability, Image<Rgba32>>
        {
            { Releaseability.Undefined, Image.Load<Rgba32>(CatalogueIcons.TinyRed) },

            {
                Releaseability.ExceptionOccurredWhileEvaluatingReleaseability,
                Image.Load<Rgba32>(CatalogueIcons.TinyRed)
            },
            { Releaseability.NeverBeenSuccessfullyExecuted, Image.Load<Rgba32>(CatalogueIcons.Failed) },
            { Releaseability.ExtractFilesMissing, Image.Load<Rgba32>(CatalogueIcons.FileMissing) },
            { Releaseability.ExtractionSQLDesynchronisation, Image.Load<Rgba32>(CatalogueIcons.Diff) },
            { Releaseability.CohortDesynchronisation, Image.Load<Rgba32>(CatalogueIcons.Failed) },
            { Releaseability.ColumnDifferencesVsCatalogue, Image.Load<Rgba32>(CatalogueIcons.TinyYellow) },
            { Releaseability.Releaseable, Image.Load<Rgba32>(CatalogueIcons.TinyGreen) }
        };

        _environmentImages = new Dictionary<TicketingReleaseabilityEvaluation, Image<Rgba32>>
        {
            {
                TicketingReleaseabilityEvaluation.CouldNotAuthenticateAgainstServer,
                Image.Load<Rgba32>(CatalogueIcons.TinyRed)
            },
            {
                TicketingReleaseabilityEvaluation.CouldNotReachTicketingServer,
                Image.Load<Rgba32>(CatalogueIcons.TinyRed)
            },
            { TicketingReleaseabilityEvaluation.NotReleaseable, Image.Load<Rgba32>(CatalogueIcons.TinyRed) },
            { TicketingReleaseabilityEvaluation.Releaseable, Image.Load<Rgba32>(CatalogueIcons.TinyGreen) },
            { TicketingReleaseabilityEvaluation.TicketingLibraryCrashed, Image.Load<Rgba32>(CatalogueIcons.TinyRed) },
            {
                TicketingReleaseabilityEvaluation.TicketingLibraryMissingOrNotConfiguredCorrectly,
                Image.Load<Rgba32>(CatalogueIcons.TinyYellow)
            }
        };
    }

    public Image<Rgba32> GetImageIfSupportedObject(object o)
    {
        return o switch
        {
            Releaseability releaseability => _images[releaseability],
            TicketingReleaseabilityEvaluation evaluation => _environmentImages[evaluation],
            _ => null
        };
    }
}