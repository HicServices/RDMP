using System;
using System.Drawing;
using CatalogueManager.Icons.IconProvision;
using DataExportLibrary.Data;
using DataExportLibrary.Data.LinkCreators;
using DataExportLibrary.Providers.Nodes;
using DataExportManager.Icons.IconProvision.StateBasedIconProviders;
using ReusableLibraryCode.Icons.IconProvision;

namespace DataExportManager.Icons.IconProvision
{
    public class DataExportIconProvider : CatalogueIconProvider
    {
        public DataExportIconProvider(IIconProvider[] pluginIconProviders): base(pluginIconProviders)
        {
            //Calls to the Resource manager cause file I/O (I think or at the least CPU use anyway) so cache them all at once  
            StateBasedIconProviders.Add(new ExtractableDataSetStateBasedIconProvider());
            StateBasedIconProviders.Add(new ExtractionConfigurationStateBasedIconProvider(this));
        }

        public override Bitmap GetImage(object concept, OverlayKind kind = OverlayKind.None)
        {
            if (concept is LinkedCohortNode)
                return base.GetImage(RDMPConcept.ExtractableCohort, OverlayKind.Link);

            if (concept as Type == typeof(SelectedDataSets))
                return base.GetImage(RDMPConcept.Catalogue, OverlayKind.Link);

            if (concept is SelectedDataSets)
                return base.GetImage(((SelectedDataSets)concept).ExtractableDataSet.Catalogue, OverlayKind.Link);

            if (concept is PackageContentNode)
                return base.GetImage(RDMPConcept.ExtractableDataSet, OverlayKind.Link);
            
            if (concept is ProjectCohortIdentificationConfigurationAssociation)
                return GetImage(((ProjectCohortIdentificationConfigurationAssociation)concept).GetCohortIdentificationConfigurationCached(), OverlayKind.Link);

            //fallback on parent implementation if none of the above unique snowflake cases are met
            return base.GetImage(concept, kind);
        }
    }
}
