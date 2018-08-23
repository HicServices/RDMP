using CatalogueLibrary.Nodes;
using CatalogueManager.CommandExecution.AtomicCommands;

namespace CatalogueManager.Menus
{
    class AllStandardRegexesNodeMenu : RDMPContextMenuStrip
    {
        public AllStandardRegexesNodeMenu(RDMPContextMenuStripArgs args, AllStandardRegexesNode o) : base(args, o)
        {
            Add(new ExecuteCommandCreateNewStandardRegex(args.ItemActivator));
        }
    }
}
