using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Data.DataTables;
using DataExportManager.CohortUI.ImportCustomData;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.Icons.IconProvision;

namespace DataExportManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandImportFileAsCustomDataForCohort : BasicUICommandExecution, IAtomicCommandWithTarget
    {
        private ExtractableCohort _cohort;
        private FileInfo _file;

        public ExecuteCommandImportFileAsCustomDataForCohort(IActivateItems activator, ExtractableCohort cohort= null, FileCollectionCommand fileCommand=null) : base(activator)
        {
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
            var importer = new ImportCustomDataFileUI(Activator, _cohort, new FlatFileToLoad(file));
            importer.RepositoryLocator = Activator.RepositoryLocator;
            Activator.ShowWindow(importer, true);
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.CustomDataTableNode, OverlayKind.Add);
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            _cohort = (ExtractableCohort)target;
            return this;
        }
    }
}