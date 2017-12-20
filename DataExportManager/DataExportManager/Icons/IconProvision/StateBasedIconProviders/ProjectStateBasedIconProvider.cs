using System.Drawing;
using System.Linq;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.Icons.IconProvision.StateBasedIconProviders;
using DataExportLibrary.Data.DataTables;
using DataExportManager.Collections.Providers;
using ReusableLibraryCode.Icons.IconProvision;

namespace DataExportManager.Icons.IconProvision.StateBasedIconProviders
{
    public class ProjectStateBasedIconProvider : IObjectStateBasedIconProvider
    {
        private readonly CatalogueIconProvider _iconProvider;
        private DataExportProblemProvider _problemProvider;
        private DataExportChildProvider _childProvider;


        public ProjectStateBasedIconProvider(CatalogueIconProvider iconProvider)
        {
            _iconProvider = iconProvider;
        }

        public void SetProviders( DataExportChildProvider childProvider,DataExportProblemProvider problemProvider)
        {
            _problemProvider = problemProvider;
            _childProvider = childProvider;
        }

        public Bitmap GetImageIfSupportedObject(object o)
        {
            var project = o as Project;

            if (project == null)
                return null;

            Bitmap basicImage;

            //if we know children and that this project doesn't have any
            if (_childProvider != null && !_childProvider.GetActiveConfigurationsOnly(project).Any())
                basicImage = _iconProvider.GetImage(RDMPConcept.EmptyProject);//then it's an empty project
            else
                //either project is ok or we are missing a provider
                basicImage = _iconProvider.GetImage(RDMPConcept.Project);

            //now that we have a basic image to represent the Project, are they any problems with the Project?
            if (_problemProvider != null && _problemProvider.HasProblems(project))
                return _iconProvider.OverlayProvider.GetOverlay(basicImage,OverlayKind.Problem);

            return basicImage;
        }
    }
}