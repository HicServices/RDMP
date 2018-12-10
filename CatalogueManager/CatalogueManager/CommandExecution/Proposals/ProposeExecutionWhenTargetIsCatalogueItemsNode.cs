using System;
using CatalogueLibrary.Nodes;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Copying.Commands;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution.Proposals
{
    class ProposeExecutionWhenTargetIsCatalogueItemsNode : RDMPCommandExecutionProposal<CatalogueItemsNode>
    {
        public ProposeExecutionWhenTargetIsCatalogueItemsNode(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override bool CanActivate(CatalogueItemsNode target)
        {
            return true;
        }

        public override void Activate(CatalogueItemsNode target)
        {
            var cmd = new ExecuteCommandBulkProcessCatalogueItems(ItemActivator, target.Catalogue);
            cmd.Execute();
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, CatalogueItemsNode target, InsertOption insertOption = InsertOption.Default)
        {
            var colInfo = cmd as ColumnInfoCommand;
            var tableInfo = cmd as TableInfoCommand;

            if (colInfo != null)
                return new ExecuteCommandAddNewCatalogueItem(ItemActivator, target.Catalogue, colInfo);

            if (tableInfo != null)
                return new ExecuteCommandAddNewCatalogueItem(ItemActivator, target.Catalogue, tableInfo.TableInfo.ColumnInfos);

            return null;
        }
    }
}