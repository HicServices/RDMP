using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueManager.CommandExecution.Proposals;
using DataExportLibrary.Data.DataTables;
using DataExportManager.CommandExecution.AtomicCommands;
using DataExportManager.ItemActivation;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;
using ReusableUIComponents.CommandExecution.Proposals;

namespace DataExportManager.CommandExecution.Proposals
{
    public class ProposeExecutionWhenTargetIsExtractableCohort : RDMPCommandExecutionProposal<ExtractableCohort>
    {
        public ProposeExecutionWhenTargetIsExtractableCohort(IActivateDataExportItems activator):base(activator)
        {
        }

        public override bool CanActivate(ExtractableCohort target)
        {
            return false;
        }

        public override void Activate(ExtractableCohort target)
        {
            //nothing
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, ExtractableCohort target, InsertOption insertOption = InsertOption.Default)
        {
            var fileCommand = cmd as FileCollectionCommand; 
            if(fileCommand != null)
                return new ExecuteCommandImportFileAsCustomDataForCohort((IActivateDataExportItems)ItemActivator, target, fileCommand);

            //no command possible, dragged command must have been something else
            return null;
        }
    }
}