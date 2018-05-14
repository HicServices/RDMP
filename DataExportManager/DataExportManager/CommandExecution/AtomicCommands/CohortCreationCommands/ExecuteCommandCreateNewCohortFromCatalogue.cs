using System;
using System.Drawing;
using System.Linq;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Data.DataTables;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableLibraryCode.Icons.IconProvision;

namespace DataExportManager.CommandExecution.AtomicCommands.CohortCreationCommands
{
    public class ExecuteCommandCreateNewCohortFromCatalogue : CohortCreationCommandExecution
    {
        private ExtractionInformation _extractionIdentifierColumn;


        public ExecuteCommandCreateNewCohortFromCatalogue(IActivateItems activator,ExtractionInformation extractionInformation) : base(activator)
        {
            SetExtractionIdentifierColumn(extractionInformation);
        }

        public ExecuteCommandCreateNewCohortFromCatalogue(IActivateItems activator, Catalogue catalogue): base(activator)
        {
            SetExtractionIdentifierColumn(GetExtractionInformationFromCatalogue(catalogue));
        }

        public ExecuteCommandCreateNewCohortFromCatalogue(IActivateItems activator): base(activator)
        {
            
        }

        public override IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            var cata = target as Catalogue;
            var ei = target as ExtractionInformation;

            if (cata != null)
                SetExtractionIdentifierColumn(GetExtractionInformationFromCatalogue(cata));

            if (ei != null)
                SetExtractionIdentifierColumn(ei);

            return base.SetTarget(target);
        }

        private ExtractionInformation GetExtractionInformationFromCatalogue(Catalogue catalogue)
        {

            var eis = catalogue.GetAllExtractionInformation(ExtractionCategory.Any);

            if (eis.Count(ei => ei.IsExtractionIdentifier) != 1)
            {
                SetImpossible("Catalogue must have a single IsExtractionIdentifier column");
                return null;
            }

            return eis.Single(e => e.IsExtractionIdentifier);
        }

        private void SetExtractionIdentifierColumn(ExtractionInformation extractionInformation)
        {
            if (!extractionInformation.IsExtractionIdentifier)
                SetImpossible("Column is not marked IsExtractionIdentifier");

            _extractionIdentifierColumn = extractionInformation;
        }

        public override void Execute()
        {
            base.Execute();

            var request = GetCohortCreationRequest("All patient identifiers in ExtractionInformation '" + _extractionIdentifierColumn.CatalogueItem.Catalogue + "." + _extractionIdentifierColumn.GetRuntimeName() + "'  (ID=" + _extractionIdentifierColumn.ID +")");

            //user choose to cancel the cohort creation request dialogue
            if (request == null)
                return;

            var configureAndExecute = GetConfigureAndExecuteControl(request, "Import column " + _extractionIdentifierColumn + " as cohort and commmit results");

            configureAndExecute.AddInitializationObject(_extractionIdentifierColumn);
            configureAndExecute.TaskDescription = "You have selected a patient identifier column in a dataset, the unique identifier list in this column will be commmented to the named project/cohort ready for data export.  This dialog requires you to select/create an appropriate pipeline. " + TaskDescriptionGenerallyHelpfulText;

            Activator.ShowWindow(configureAndExecute);
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.ExtractableCohort, OverlayKind.Add);
        }
    }
}