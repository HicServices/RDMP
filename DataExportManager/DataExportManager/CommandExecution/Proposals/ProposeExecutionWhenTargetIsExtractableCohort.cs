using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueManager.CommandExecution.Proposals;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Data.DataTables;
using DataExportManager.CohortUI;
using DataExportManager.CommandExecution.AtomicCommands;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace DataExportManager.CommandExecution.Proposals
{
    public class ProposeExecutionWhenTargetIsExtractableCohort : RDMPCommandExecutionProposal<ExtractableCohort>
    {
        public ProposeExecutionWhenTargetIsExtractableCohort(IActivateItems activator):base(activator)
        {
        }

        public override bool CanActivate(ExtractableCohort target)
        {
            return false;
        }

        public override void Activate(ExtractableCohort target)
        {
            ItemActivator.Activate<ExtractableCohortUI, ExtractableCohort>(target);
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, ExtractableCohort target, InsertOption insertOption = InsertOption.Default)
        {
            var fileCommand = cmd as FileCollectionCommand; 
            if(fileCommand != null)
                return new ExecuteCommandImportFileAsCustomDataForCohort(ItemActivator, target, fileCommand);

            //no command possible, dragged command must have been something else
            return null;
        }
    }
}