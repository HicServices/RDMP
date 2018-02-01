using System;
using System.Drawing;
using CatalogueLibrary.Data.Aggregation;
using CatalogueManager.Icons.IconOverlays;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.Icons.IconProvision.StateBasedIconProviders
{
    public class AggregateConfigurationStateBasedIconProvider : IObjectStateBasedIconProvider
    {
        private readonly IconOverlayProvider _overlayProvider;
        private Bitmap _cohortAggregates;
        private Bitmap _aggregates;
        private Bitmap _patientIndexTable;

        public AggregateConfigurationStateBasedIconProvider(IconOverlayProvider overlayProvider)
        {
            _overlayProvider = overlayProvider;
            _cohortAggregates = CatalogueIcons.CohortAggregate;
            _aggregates = CatalogueIcons.AggregateGraph;
            _patientIndexTable = CatalogueIcons.PatientIndexTable;
        }

        public Bitmap GetImageIfSupportedObject(object o)
        {
            if (o is Type && o.Equals(typeof (AggregateConfiguration)))
                return _aggregates;

            var ac = o as AggregateConfiguration;

            if (ac == null)
                return null;
            
            Bitmap img = ac.IsCohortIdentificationAggregate ? _cohortAggregates : _aggregates;

            if (ac.IsJoinablePatientIndexTable(true))
                img = _patientIndexTable;

            if (ac.IsExtractable)
                img = _overlayProvider.GetOverlay(img, OverlayKind.Extractable);

            if (ac.OverrideFiltersByUsingParentAggregateConfigurationInstead_ID != null)
                img =_overlayProvider.GetOverlay(img, OverlayKind.Shortcut);

            return img;
        }
    }
}