using System.Drawing;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Validation;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandConfigureCatalogueValidationRules : BasicUICommandExecution,IAtomicCommandWithTarget
    {
        private Catalogue _catalogue;

        public ExecuteCommandConfigureCatalogueValidationRules(IActivateItems activator) : base(activator)
        {
            
        }

        public override string GetCommandHelp()
        {
            return "Allows you to specify validation rules for columns in the dataset and pick the time coverage/pivot fields";
        }

        public override string GetCommandName()
        {
            return "Validation Rules...";
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.DQE, OverlayKind.Edit);
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            _catalogue = (Catalogue) target;
            return this;
        }

        public override void Execute()
        {
            base.Execute();

            if (_catalogue == null)
                _catalogue = SelectOne<Catalogue>(Activator.RepositoryLocator.CatalogueRepository);

            if(_catalogue == null)
                return;

            Activator.Activate<ValidationSetupForm, Catalogue>(_catalogue);
        }
    }
}