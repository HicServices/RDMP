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
    class ProposeExecutionWhenTargetIsCatalogue:RDMPCommandExecutionProposal<Catalogue>
    {
        public ProposeExecutionWhenTargetIsCatalogue(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override bool CanActivate(Catalogue target)
        {
            return true;
        }

        public override void Activate(Catalogue c)
        {
            ItemActivator.Activate<CatalogueTab, Catalogue>(c);
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, Catalogue targetCatalogue, InsertOption insertOption = InsertOption.Default)
        {
            var sourceFileCollection = cmd as FileCollectionCommand;

            if(sourceFileCollection != null)
                return new ExecuteCommandAddFilesAsSupportingDocuments(ItemActivator,sourceFileCollection, targetCatalogue);

            return null;
        }
    }
}
