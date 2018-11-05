using System.ComponentModel.Composition;
using System.Drawing;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueManager.DataQualityUIs;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandRunDQEOnCatalogue:BasicUICommandExecution,IAtomicCommandWithTarget
    {
        private Catalogue _catalogue;
        
        [ImportingConstructor]
        public ExecuteCommandRunDQEOnCatalogue(IActivateItems activator,Catalogue catalogue): base(activator)
        {
            _catalogue = catalogue;
        }

        public ExecuteCommandRunDQEOnCatalogue(IActivateItems activator):base(activator)
        {
        }

        public override string GetCommandHelp()
        {
            return "Runs the data quality engine on the dataset using the currently configured validation rules and stores the results in the default DQE results database";
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.DQE, OverlayKind.Execute);
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            _catalogue = (Catalogue) target;
            return this;
        }

        public override void Execute()
        {
            base.Execute();
            Activator.Activate<DQEExecutionControl, Catalogue>(_catalogue);
        }

        public override string GetCommandName()
        {
            return "Data Quality Engine";
        }
    }
}
