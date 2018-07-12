using System;
using System.Collections.Generic;
using System.Drawing;
using DataExportLibrary.DataRelease.Potential;

namespace CatalogueManager.Icons.IconProvision.StateBasedIconProviders
{
    public class ReleaseabilityStateBasedIconProvider : IObjectStateBasedIconProvider
    {
        private readonly Dictionary<Releaseability,Bitmap> _images = new Dictionary<Releaseability, Bitmap>();

        public ReleaseabilityStateBasedIconProvider()
        {
            _images.Add(Releaseability.Undefined,CatalogueIcons.TinyRed);

            _images.Add(Releaseability.ExceptionOccurredWhileEvaluatingReleaseability, CatalogueIcons.TinyRed);
            _images.Add(Releaseability.NeverBeenSuccessfullyExecuted, CatalogueIcons.Failed);
            _images.Add(Releaseability.ExtractFilesMissing, CatalogueIcons.Failed);
            _images.Add(Releaseability.ExtractionSQLDesynchronisation, CatalogueIcons.Failed);
            _images.Add(Releaseability.CohortDesynchronisation, CatalogueIcons.Failed);
            _images.Add(Releaseability.ColumnDifferencesVsCatalogue, CatalogueIcons.TinyYellow);
            _images.Add(Releaseability.Releaseable, CatalogueIcons.TinyRed);
       
        }

        public Bitmap GetImageIfSupportedObject(object o)
        {
            if (!(o is Releaseability))
                return null;

            return _images[(Releaseability) o];
        }
    }
}