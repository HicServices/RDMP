using System;
using System.Drawing;
using System.Windows.Forms;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.Icons.IconProvision.Exceptions;
using CatalogueManager.PluginChildProvision;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.LinkCreators;
using DataExportManager.Collections.Nodes;
using DataExportManager.Collections.Nodes.UsedByProject;
using DataExportManager.Collections.Providers;
using DataExportManager.Icons.IconProvision.StateBasedIconProviders;

namespace DataExportManager.Icons.IconProvision
{
    public class DataExportIconProvider : CatalogueIconProvider
    {
        private readonly ProjectStateBasedIconProvider _projectIconProvider;
        private ExtractionConfigurationStateBasedIconProvider _extractionConfigurationIconProvider;

        public DataExportIconProvider(IIconProvider[] pluginIconProviders): base(pluginIconProviders)
        {
            //Calls to the Resource manager cause file I/O (I think or at the least CPU use anyway) so cache them all at once  
            _projectIconProvider = new ProjectStateBasedIconProvider(this);
            StateBasedIconProviders.Add(_projectIconProvider);
            StateBasedIconProviders.Add(new ExtractableDataSetStateBasedIconProvider());
            StateBasedIconProviders.Add(new CustomDataTableNodeStateBasedIconProvider());
            _extractionConfigurationIconProvider = new ExtractionConfigurationStateBasedIconProvider(this);
            StateBasedIconProviders.Add(_extractionConfigurationIconProvider);

        }

        public void SetProviders(DataExportChildProvider childProvider, DataExportProblemProvider problemProvider)
        {
            _projectIconProvider.SetProviders(childProvider,problemProvider);
            _extractionConfigurationIconProvider.SetProviders(childProvider,problemProvider);
        }

        public override Bitmap GetImage(object concept, OverlayKind kind = OverlayKind.None)
        {
            if (concept is LinkedCohortNode)
                return base.GetImage(RDMPConcept.ExtractableCohort, OverlayKind.Link);

            if (concept is SelectedDataSets)
                return base.GetImage(RDMPConcept.ExtractableDataSet, OverlayKind.Link);

            if (concept is PackageContentNode)
                return base.GetImage(RDMPConcept.ExtractableDataSet, OverlayKind.Link);

            //if it is a ProjectUsageNode then get the icon for the underlying object being used e.g. ExtractableCohortUsedByProject would return the icon for ExtractableCohort
            if (concept is IObjectUsedByProjectNode)
                return GetImage(((IObjectUsedByProjectNode) concept).UnderlyingObjectConceptualType, kind);
            
            //fallback on parent implementation if none of the above unique snowflake cases are met
            return base.GetImage(concept, kind);
        }
        
        
    }
}
