using System.Drawing;
using CatalogueLibrary.Data.Remoting;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents.CommandExecution.AtomicCommands;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCreateNewRemoteRDMP : BasicUICommandExecution, IAtomicCommand
    {
        public ExecuteCommandCreateNewRemoteRDMP(IActivateItems activator) : base(activator)
        {
        }

        public override void Execute()
        {
            base.Execute();
            var remote = new RemoteRDMP(Activator.RepositoryLocator.CatalogueRepository);
            Publish(remote);
            Emphasise(remote);
            Activate(remote);
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.RemoteRDMP, OverlayKind.Add);
        }
    }
}