using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataExportLibrary.Data.DataTables;
using DataExportManager.CommandExecution.AtomicCommands;
using DataExportManager.ItemActivation;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;
using ReusableUIComponents.CommandExecution.Proposals;

namespace DataExportManager.CommandExecution.Proposals
{
    public class ProposeExecutionWhenTargetIsExtractableCohort:ICommandExecutionProposal
    {
        private readonly IActivateDataExportItems _activator;

        public ProposeExecutionWhenTargetIsExtractableCohort(IActivateDataExportItems activator)
        {
            _activator = activator;
        }

        public ICommandExecution ProposeExecution(ICommand cmd, object target, InsertOption insertOption = InsertOption.Default)
        {
            var cohort = target as ExtractableCohort;

            //not a command that relates to us
            if (cohort == null)
                return null;

            var fileCommand = cmd as FileCollectionCommand; 
            if(fileCommand != null)
                return new ExecuteCommandImportFileAsCustomDataForCohort(_activator,cohort,fileCommand);

            //no command possible, dragged command must have been something else
            return null;
        }
    }
}