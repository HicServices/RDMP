using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Providers;
using MapsDirectlyToDatabaseTableUI;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace DataExportManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandChooseCohort : BasicUICommandExecution,IAtomicCommand
    {
        private readonly ExtractionConfiguration _extractionConfiguration;
        private DataExportChildProvider _childProvider;
        List<ExtractableCohort> _compatibleCohorts = new List<ExtractableCohort>();

        public ExecuteCommandChooseCohort(IActivateItems activator, ExtractionConfiguration extractionConfiguration):base(activator)
        {
            _extractionConfiguration = extractionConfiguration;

            var project = _extractionConfiguration.Project;

            if (extractionConfiguration.IsReleased)
            {
                SetImpossible("ExtractionConfiguration has already been released");
                return;
            }

            if (!project.ProjectNumber.HasValue)
            {
                SetImpossible("Project does not have a ProjectNumber, this determines which cohorts are eligible");
                return;
            }

            _childProvider = Activator.CoreChildProvider as DataExportChildProvider;

            if (_childProvider == null)
            {
                SetImpossible("Activator.CoreChildProvider is not an DataExportChildProvider");
                return;
            }

            //find cohorts that match the project number
            if (_childProvider.ProjectNumberToCohortsDictionary.ContainsKey(project.ProjectNumber.Value))
                _compatibleCohorts = (_childProvider.ProjectNumberToCohortsDictionary[project.ProjectNumber.Value]).ToList();

            //if theres only one compatible cohort and that one is already selected
            if (_compatibleCohorts.Count == 1 && _compatibleCohorts.Single().ID == _extractionConfiguration.Cohort_ID)
                SetImpossible("The currently select cohort is the only one available");

            if(!_compatibleCohorts.Any())
                SetImpossible("There are no cohorts currently configured with ProjectNumber " + project.ProjectNumber.Value);
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.ExtractableCohort, OverlayKind.Link);
        }

        public override void Execute()
        {
            base.Execute();
            
            var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(_compatibleCohorts.Where(c => c.ID != _extractionConfiguration.Cohort_ID), false, false);

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                //clear current one
                _extractionConfiguration.Cohort_ID = ((ExtractableCohort)dialog.Selected).ID;
                _extractionConfiguration.SaveToDatabase();
                Publish(_extractionConfiguration);
            }
        
        }
    }
}