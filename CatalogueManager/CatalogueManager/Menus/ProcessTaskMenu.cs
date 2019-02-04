using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.CommandExecution.AtomicCommands;

namespace CatalogueManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    class ProcessTaskMenu : RDMPContextMenuStrip
    {
        public ProcessTaskMenu(RDMPContextMenuStripArgs args, ProcessTask processTask) : base(args, processTask)
        {
            Add(new ExecuteCommandDisableOrEnable(args.ItemActivator,processTask));
        }
    }
}