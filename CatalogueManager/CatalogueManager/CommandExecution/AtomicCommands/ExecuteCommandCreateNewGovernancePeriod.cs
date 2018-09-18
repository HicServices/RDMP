using System.Drawing;
using CatalogueLibrary.Data.Governance;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    class ExecuteCommandCreateNewGovernancePeriod:BasicUICommandExecution,IAtomicCommand
    {
        public ExecuteCommandCreateNewGovernancePeriod(IActivateItems activator) : base(activator)
        {
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.GovernancePeriod, OverlayKind.Add);
        }

        public override void Execute()
        {
            base.Execute();

            var period = new GovernancePeriod(Activator.RepositoryLocator.CatalogueRepository);
            Publish(period);
            Emphasise(period);
            Activate(period);
        }
    }
}
