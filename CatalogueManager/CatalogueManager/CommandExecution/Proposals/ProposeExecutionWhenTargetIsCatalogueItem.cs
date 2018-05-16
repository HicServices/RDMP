using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueManager.ItemActivation;
using CatalogueManager.MainFormUITabs;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution.Proposals
{
    class ProposeExecutionWhenTargetIsCatalogueItem:RDMPCommandExecutionProposal<CatalogueItem>
    {
        public ProposeExecutionWhenTargetIsCatalogueItem(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override bool CanActivate(CatalogueItem target)
        {
            return true;
        }

        public override void Activate(CatalogueItem target)
        {
            ItemActivator.Activate<CatalogueItemTab, CatalogueItem>(target);
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, CatalogueItem target, InsertOption insertOption = InsertOption.Default)
        {
            var sourceColumnInfo = cmd as ColumnInfoCommand;

            if (sourceColumnInfo != null)
                return new ExecuteCommandLinkCatalogueItemToColumnInfo(ItemActivator, sourceColumnInfo, target);

            return null;
        }
    }
}
