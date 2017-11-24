using CatalogueLibrary.Nodes;
using CatalogueManager.Collections;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus;

namespace CohortManager.Menus
{
    internal class JoinableCollectionNodeMenu : RDMPContextMenuStrip
    {
        public JoinableCollectionNodeMenu(RDMPContextMenuStripArgs args, JoinableCollectionNode patientIndexTablesNode): base(args, null)
        {
            Add(new ExecuteCommandAddCatalogueToCohortIdentificationAsPatientIndexTable(_activator,patientIndexTablesNode.Configuration));
        }

    }
}