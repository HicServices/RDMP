using System;
using System.Drawing;
using CatalogueLibrary.Data;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents.CommandExecution.AtomicCommands;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandCreateNewEmptyCatalogue : BasicUICommandExecution,IAtomicCommand
    {
        public ExecuteCommandCreateNewEmptyCatalogue(IActivateItems activator) : base(activator)
        {
            
        }

        public override string GetCommandName()
        {
            return base.GetCommandName() + "(Not Recommended)";
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Catalogue, OverlayKind.Problem);
        }

        public override void Execute()
        {
            base.Execute();

            var c = new Catalogue(Activator.RepositoryLocator.CatalogueRepository, "New Catalogue " + Guid.NewGuid());
            Publish(c);
            Emphasise(c);
        }
    }
}