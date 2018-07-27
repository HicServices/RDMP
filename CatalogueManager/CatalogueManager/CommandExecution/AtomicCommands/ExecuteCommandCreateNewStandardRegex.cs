using System.Drawing;
using CatalogueLibrary.Data;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCreateNewStandardRegex : BasicUICommandExecution,IAtomicCommand
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

        public override string GetCommandHelp()
        {
            return "Regular Expressions are patterns that match a given text input.  StandardRegex allow a central declaration of a given pattern rather than copying and pasting it everywhere";
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.StandardRegex, OverlayKind.Add);
        }
    }
}