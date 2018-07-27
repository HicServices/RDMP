using System.Drawing;
using CatalogueLibrary.Data;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandCreateNewStandardRegex : BasicUICommandExecution,IAtomicCommand
    {
        public ExecuteCommandCreateNewStandardRegex(IActivateItems activator):base(activator)
        {
            
        }

        public override void Execute()
        {
            var regex = new StandardRegex(Activator.RepositoryLocator.CatalogueRepository);

            Publish(regex);
            Emphasise(regex);
            Activate(regex);
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.StandardRegex, OverlayKind.Add);
        }
    }
}