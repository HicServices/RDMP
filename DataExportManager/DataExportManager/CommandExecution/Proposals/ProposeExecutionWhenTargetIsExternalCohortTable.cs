using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueManager.CommandExecution.Proposals;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Data.DataTables;
using DataExportManager.CohortUI.CohortSourceManagement;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace DataExportManager.CommandExecution.Proposals
{
    class ProposeExecutionWhenTargetIsExternalCohortTable:RDMPCommandExecutionProposal<ExternalCohortTable>
    {
        public ProposeExecutionWhenTargetIsExternalCohortTable(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override bool CanActivate(ExternalCohortTable target)
        {
            return true;
        }

        public override void Activate(ExternalCohortTable target)
        {
            ItemActivator.Activate<ExternalCohortTableUI, ExternalCohortTable>(target);
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, ExternalCohortTable target, InsertOption insertOption = InsertOption.Default)
        {
            return null;
        }
    }
}
