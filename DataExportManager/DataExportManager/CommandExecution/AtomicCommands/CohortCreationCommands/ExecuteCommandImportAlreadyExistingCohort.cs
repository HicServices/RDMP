using System;
using System.Drawing;
using System.Windows.Forms;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Data.DataTables;
using DataExportManager.SimpleDialogs;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace DataExportManager.CommandExecution.AtomicCommands.CohortCreationCommands
{
    internal class ExecuteCommandImportAlreadyExistingCohort : BasicUICommandExecution,IAtomicCommand
    {
        private readonly ExternalCohortTable _externalCohortTable;

        public ExecuteCommandImportAlreadyExistingCohort(IActivateItems activator, ExternalCohortTable externalCohortTable):base(activator)
        {
            _externalCohortTable = externalCohortTable;
        }

        public override void Execute()
        {
            base.Execute();

            SelectWhichCohortToImport importDialog = new SelectWhichCohortToImport(Activator,_externalCohortTable);

            if (importDialog.ShowDialog() == DialogResult.OK)
            {
                int toAddID = importDialog.IDToImport;
                try
                {
                    var newCohort = new ExtractableCohort(Activator.RepositoryLocator.DataExportRepository, _externalCohortTable, toAddID);
                    Publish(_externalCohortTable);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                }
            }
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.CohortAggregate, OverlayKind.Import);
        }
    }
}