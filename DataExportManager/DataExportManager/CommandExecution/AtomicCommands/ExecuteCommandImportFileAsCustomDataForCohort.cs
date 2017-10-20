using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueManager.Icons.IconProvision;
using DataExportLibrary.Data.DataTables;
using DataExportManager.CohortUI.ImportCustomData;
using DataExportManager.ItemActivation;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.Icons.IconProvision;

namespace DataExportManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandImportFileAsCustomDataForCohort : BasicCommandExecution, IAtomicCommandWithTarget
    {
        private readonly IActivateDataExportItems _activator;
        private ExtractableCohort _cohort;

        public ExecuteCommandImportFileAsCustomDataForCohort(IActivateDataExportItems activator, ExtractableCohort cohort):this(activator)
        {
            SetTarget(cohort);
        }

        public ExecuteCommandImportFileAsCustomDataForCohort(IActivateDataExportItems activator)
        {
            _activator = activator;
        }

        public override void Execute()
        {
            if(_cohort == null)
                SetImpossible("Cohort has not been selected yet, either initialize it in the constructor or make sure that SetTarget is called first");

            base.Execute();

            ImportFileAsCustomData();

        }

        private void ImportFileAsCustomData()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;

            DialogResult dialogResult = ofd.ShowDialog();

            if (dialogResult == DialogResult.OK)
            {
                foreach (string file in ofd.FileNames)
                {
                    var importer = new ImportCustomDataFileUI(_activator, _cohort, new FlatFileToLoad(new FileInfo(file)));
                    importer.RepositoryLocator = _activator.RepositoryLocator;
                    _activator.ShowWindow(importer, true);
                }
            }
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.CustomDataTableNode, OverlayKind.Add);
        }

        public void SetTarget(DatabaseEntity target)
        {
            _cohort = (ExtractableCohort)target;
        }
    }
}