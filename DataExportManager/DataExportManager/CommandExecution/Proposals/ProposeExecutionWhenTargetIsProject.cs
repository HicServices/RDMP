using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueManager.CommandExecution.Proposals;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Data.DataTables;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace DataExportManager.CommandExecution.Proposals
{
    public class ProposeExecutionWhenTargetIsProject:RDMPCommandExecutionProposal<Project>
    {
        public ProposeExecutionWhenTargetIsProject(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override bool CanActivate(Project target)
        {
            return true;
        }

        public override void Activate(Project target)
        {
            ItemActivator.Activate<ProjectUI.ProjectUI, Project>(target);
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, Project target, InsertOption insertOption = InsertOption.Default)
        {
            return null;
        }
    }
}
