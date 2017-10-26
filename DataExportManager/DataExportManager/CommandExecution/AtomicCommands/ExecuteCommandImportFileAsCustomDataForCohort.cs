using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueManager.Icons.IconProvision;
using DataExportLibrary.Data.DataTables;
using DataExportManager.CohortUI.ImportCustomData;
using DataExportManager.ItemActivation;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.Icons.IconProvision;

namespace DataExportManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandImportFileAsCustomDataForCohort : BasicCommandExecution, IAtomicCommandWithTarget
    {
        private readonly IActivateDataExportItems _activator;
        private ExtractableCohort _cohort;
        private FileInfo _file;

        public ExecuteCommandImportFileAsCustomDataForCohort(IActivateDataExportItems activator, ExtractableCohort cohort= null, FileCollectionCommand fileCommand=null)
        {
            _activator = activator;

            if(cohort != null)
                SetTarget(cohort);
            
            if(fileCommand != null)
            {
                if(!fileCommand.Files.Any())
                    return;

                if(fileCommand.Files.Count() > 1)
                {
                    SetImpossible("Only one file can be added at once to an ExtractableCohort");
                    return;
                }

                _file = fileCommand.Files[0];
            }
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
            if (_file != null)
            {
                //file came from a command or somehow otherwise the file was selected already (e.g. drag and drop)
                ImportFile(_file);
                return;
            }

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;

            DialogResult dialogResult = ofd.ShowDialog();

            if (dialogResult == DialogResult.OK)
                foreach (string file in ofd.FileNames)
                    ImportFile(new FileInfo(file));
        }

        private void ImportFile(FileInfo file)
        {
            var importer = new ImportCustomDataFileUI(_activator, _cohort, new FlatFileToLoad(file));
            importer.RepositoryLocator = _activator.RepositoryLocator;
            _activator.ShowWindow(importer, true);
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