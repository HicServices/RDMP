using System;
using System.Collections.Generic;
using System.Drawing;
using CatalogueLibrary.Ticketing;
using DataExportLibrary.DataRelease.Potential;

namespace CatalogueManager.Icons.IconProvision.StateBasedIconProviders
{
    public class ReleaseabilityStateBasedIconProvider : IObjectStateBasedIconProvider
    {
        private readonly Dictionary<Releaseability,Bitmap> _images = new Dictionary<Releaseability, Bitmap>();
        private readonly Dictionary<TicketingReleaseabilityEvaluation, Bitmap> _environmentImages = new Dictionary<TicketingReleaseabilityEvaluation, Bitmap>();

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

        public Bitmap GetImageIfSupportedObject(object o)
        {
            if (o is Releaseability) 
                return _images[(Releaseability) o];

            if (o is TicketingReleaseabilityEvaluation)
                return _environmentImages[(TicketingReleaseabilityEvaluation)o];

            return null;
        }
    }
}