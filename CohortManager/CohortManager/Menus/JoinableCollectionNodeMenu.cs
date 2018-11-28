using CatalogueLibrary.Nodes;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Menus;

namespace CohortManager.Menus
{
    internal class JoinableCollectionNodeMenu : RDMPContextMenuStrip
    {
        public JoinableCollectionNodeMenu(RDMPContextMenuStripArgs args, JoinableCollectionNode patientIndexTablesNode): base(args, patientIndexTablesNode)
        {
            Add(new ExecuteCommandAddCatalogueToCohortIdentificationAsPatientIndexTable(_activator,patientIndexTablesNode.Configuration));
        }

    }
}