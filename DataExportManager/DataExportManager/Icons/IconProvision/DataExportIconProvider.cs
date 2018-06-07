using System;
using System.Drawing;
using System.Windows.Forms;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.Icons.IconProvision.Exceptions;
using CatalogueManager.PluginChildProvision;
using DataExportLibrary.Data;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.LinkCreators;
using DataExportLibrary.Providers.Nodes;
using DataExportLibrary.Providers.Nodes.UsedByNodes;
using DataExportLibrary.Providers.Nodes.UsedByProject;
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

            if (concept is SelectedDataSets)
                return base.GetImage(((SelectedDataSets)concept).ExtractableDataSet.Catalogue, OverlayKind.Link);

            if (concept is PackageContentNode)
                return base.GetImage(RDMPConcept.ExtractableDataSet, OverlayKind.Link);

            //if it is a ProjectUsageNode then get the icon for the underlying object being used e.g. ExtractableCohortUsedByProject would return the icon for ExtractableCohort
            if (concept is IObjectUsedByOtherObjectNode)
                return GetImage(((IObjectUsedByOtherObjectNode)concept).MasqueradingAs(), kind);

            if (concept is ProjectCohortIdentificationConfigurationAssociation)
                return GetImage(((ProjectCohortIdentificationConfigurationAssociation)concept).GetCohortIdentificationConfigurationCached(), OverlayKind.Link);

            //fallback on parent implementation if none of the above unique snowflake cases are met
            return base.GetImage(concept, kind);
        }
    }
}
