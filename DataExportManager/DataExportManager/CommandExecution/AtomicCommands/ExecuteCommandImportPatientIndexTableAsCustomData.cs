using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort.Joinables;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Data.DataTables;
using DataExportManager.CohortUI.ImportCustomData;
using MapsDirectlyToDatabaseTableUI;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace DataExportManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandImportPatientIndexTableAsCustomData : BasicUICommandExecution,IAtomicCommand
    {
        private readonly ExtractableCohort _cohort;

        public ExecuteCommandImportPatientIndexTableAsCustomData(IActivateItems activator, ExtractableCohort cohort) : base(activator)
        {
            _cohort = cohort;
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.PatientIndexTable, OverlayKind.Import);
        }

        public override void Execute()
        {
            base.Execute();

            var patientIndexTables = Activator.RepositoryLocator.CatalogueRepository.GetAllObjects<JoinableCohortAggregateConfiguration>().Select(j => j.AggregateConfiguration).Distinct().ToArray();

            var chooser = new SelectIMapsDirectlyToDatabaseTableDialog(patientIndexTables, false, false);

            if (chooser.ShowDialog() == DialogResult.OK)
            {
                var chosen = chooser.Selected as AggregateConfiguration;
                if (chosen != null)
                {
                    var importer = new ImportCustomDataFileUI(Activator, _cohort, chosen);
                    importer.RepositoryLocator = Activator.RepositoryLocator;
                    Activator.ShowWindow(importer, true);
                }
            }
        }
    }
}