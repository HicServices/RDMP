using System.Linq;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using DataExportLibrary.Data.DataTables;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.Copying;

namespace CatalogueManager.CommandExecution
{
    public class ExecuteCommandAddCohortToExtractionConfiguration : BasicCommandExecution
    {
        private readonly IActivateItems _activator;
        private readonly ExtractableCohortCommand _sourceExtractableCohortComand;
        private readonly ExtractionConfiguration _targetExtractionConfiguration;

        public ExecuteCommandAddCohortToExtractionConfiguration(IActivateItems activator, ExtractableCohortCommand sourceExtractableCohortComand, ExtractionConfiguration targetExtractionConfiguration)
        {
            _activator = activator;
            _sourceExtractableCohortComand = sourceExtractableCohortComand;
            _targetExtractionConfiguration = targetExtractionConfiguration;

            if(_sourceExtractableCohortComand.ErrorGettingCohortData != null)
            {
                SetImpossible("Could not fetch Cohort data:" + _sourceExtractableCohortComand.ErrorGettingCohortData.Message);
                return;
            }

            if (!sourceExtractableCohortComand.CompatibleExtractionConfigurations.Contains(_targetExtractionConfiguration))
            {
                SetImpossible("Cohort has project number " + sourceExtractableCohortComand.ExternalProjectNumber + " so can only be added to ExtractionConfigurations belonging to Projects with that same number");
                return;
            }

            if(_targetExtractionConfiguration.Cohort_ID != null)
            {
                if(_targetExtractionConfiguration.Cohort_ID == sourceExtractableCohortComand.Cohort.ID)
                    SetImpossible("ExtractionConfiguration already uses this cohort");
                else
                    SetImpossible("ExtractionConfiguration already uses a different cohort (delete the relationship to the old cohort first)");
                
                return;
            }
        }

        public override void Execute()
        {
            base.Execute();

            _targetExtractionConfiguration.Cohort_ID = _sourceExtractableCohortComand.Cohort.ID;
            _targetExtractionConfiguration.SaveToDatabase();
            _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(_targetExtractionConfiguration));

        }
    }
}