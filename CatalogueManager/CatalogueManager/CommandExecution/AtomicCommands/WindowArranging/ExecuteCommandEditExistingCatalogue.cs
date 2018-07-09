using System.ComponentModel.Composition;
using System.Drawing;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands.WindowArranging
{
    public class ExecuteCommandEditExistingCatalogue : BasicUICommandExecution, IAtomicCommandWithTarget
    {
        public Catalogue Catalogue { get; set; }

        [ImportingConstructor]
        public ExecuteCommandEditExistingCatalogue(IActivateItems activator,Catalogue catalogue)
            : base(activator)
        {
            Catalogue = catalogue;
        }


        public ExecuteCommandEditExistingCatalogue(IActivateItems activator) : base(activator)
        {
            
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Catalogue, OverlayKind.Edit);
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            Catalogue = (Catalogue) target;
            return this;
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
            Activator.WindowArranger.SetupEditCatalogue(this, Catalogue);
        }
    }
}