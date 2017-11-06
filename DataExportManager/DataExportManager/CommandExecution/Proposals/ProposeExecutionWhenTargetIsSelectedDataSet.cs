using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CatalogueManager.CommandExecution.Proposals;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Data.LinkCreators;
using DataExportManager.ProjectUI;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace DataExportManager.CommandExecution.Proposals
{
    public class ProposeExecutionWhenTargetIsSelectedDataSet:RDMPCommandExecutionProposal<SelectedDataSets>
    {
        public ProposeExecutionWhenTargetIsSelectedDataSet(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override bool CanActivate(SelectedDataSets target)
        {
            return true;
        }

        public override void Activate(SelectedDataSets target)
        {
            ItemActivator.Activate<ConfigureDatasetUI, SelectedDataSets>(target);
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, SelectedDataSets target, InsertOption insertOption = InsertOption.Default)
        {
            return null;
        }
    }
}
