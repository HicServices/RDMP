using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueManager.CommandExecution;
using CatalogueManager.CommandExecution.Proposals;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Data.DataTables;
using DataExportManager.ProjectUI;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace DataExportManager.CommandExecution.Proposals
{
    public class ProposeExecutionWhenTargetIsExtractionConfiguration:RDMPCommandExecutionProposal<ExtractionConfiguration>
    {
        public ProposeExecutionWhenTargetIsExtractionConfiguration(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override bool CanActivate(ExtractionConfiguration target)
        {
            return !target.IsReleased;
        }

        public override void Activate(ExtractionConfiguration target)
        {
            if (!target.IsReleased)
                ItemActivator.Activate<ExtractionConfigurationUI, ExtractionConfiguration>(target);
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, ExtractionConfiguration targetExtractionConfiguration, InsertOption insertOption = InsertOption.Default)
        {
            //user is trying to set the cohort of the configuration
            var sourceExtractableCohortComand = cmd as ExtractableCohortCommand;

            if (sourceExtractableCohortComand != null)
                return new ExecuteCommandAddCohortToExtractionConfiguration(ItemActivator, sourceExtractableCohortComand, targetExtractionConfiguration);

            //user is trying to add datasets to a configuration
            var sourceExtractableDataSetCommand = cmd as ExtractableDataSetCommand;

            if (sourceExtractableDataSetCommand != null)
                return new ExecuteCommandAddDatasetsToConfiguration(ItemActivator, sourceExtractableDataSetCommand, targetExtractionConfiguration);

            return null;
        }
    }
}
