using System.Drawing;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.Copying;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands.WindowArranging
{
    public class ExecuteCommandEditExistingCatalogue : BasicCommandExecution, IAtomicCommandWithTarget
    {
        private readonly IActivateItems activator;

        public Catalogue Catalogue { get; set; }

        public ExecuteCommandEditExistingCatalogue(IActivateItems activator)
        {
            this.activator = activator;
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Catalogue, OverlayKind.Edit);
        }

        public void SetTarget(DatabaseEntity target)
        {
            Catalogue = (Catalogue) target;
        }

        public override string GetCommandHelp()
        {
            return "This will take you to the Catalogues list and allow you to Edit the Catalogue and Dataset table metadata." +
                   "\r\n" +
                   "You must choose a Catalogue from the list before proceeding.";
        }

        public override void Execute()
        {
            if (Catalogue == null)
                SetImpossible("You must choose a Catalogue.");

            base.Execute();
            activator.WindowArranger.SetupEditCatalogue(this, Catalogue);
        }
    }
}