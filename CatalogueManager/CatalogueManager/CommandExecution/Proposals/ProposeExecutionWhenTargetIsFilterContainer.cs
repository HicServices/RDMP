using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueManager.ItemActivation;
using CatalogueManager.Copying.Commands;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution.Proposals
{
    class ProposeExecutionWhenTargetIsFilterContainer:RDMPCommandExecutionProposal<IContainer>
    {
        public ProposeExecutionWhenTargetIsFilterContainer(IActivateItems itemActivator) : base(itemActivator)
        {

        }

        public override bool CanActivate(IContainer target)
        {
            return false;
        }

        public override void Activate(IContainer target)
        {
            
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, IContainer targetContainer, InsertOption insertOption = InsertOption.Default)
        {
            var sourceFilterCommand = cmd as FilterCommand;

            //drag a filter into a container
            if (sourceFilterCommand != null)
            {
                //if filter is already in the target container
                if (sourceFilterCommand.ImmediateContainerIfAny.Equals(targetContainer))
                    return null;

                //if the target container is one that is part of the filters tree then it's a move
                if (sourceFilterCommand.AllContainersInEntireTreeFromRootDown.Contains(targetContainer))
                    return new ExecuteCommandMoveFilterIntoContainer(ItemActivator, sourceFilterCommand, targetContainer);
                
                //otherwise it's an import    

                //so instead lets let them create a new copy (possibly including changing the type e.g. importing a master
                //filter into a data export AND/OR container
                return new ExecuteCommandImportNewCopyOfFilterIntoContainer(ItemActivator, sourceFilterCommand, targetContainer);
                
            }

            var sourceContainerCommand = cmd as ContainerCommand;
            
            //drag a container into another container
            if (sourceContainerCommand != null)
            {
                //if the source and target are the same container
                if (sourceContainerCommand.Container.Equals(targetContainer))
                    return null;

                //is it a movement within the current container tree
                if (sourceContainerCommand.AllContainersInEntireTreeFromRootDown.Contains(targetContainer))
                    return new ExecuteCommandMoveContainerIntoContainer(ItemActivator, sourceContainerCommand, targetContainer);
            }
            
            return null;
        

        }
    }
}
