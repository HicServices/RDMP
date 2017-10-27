using CatalogueLibrary.Nodes;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus;

namespace CohortManager.Menus
{
    internal class JoinableCollectionNodeMenu : RDMPContextMenuStrip
    {
        public JoinableCollectionNodeMenu(IActivateItems activator, JoinableCollectionNode patientIndexTablesNode):base(activator,null)
        {
            Add(new ExecuteCommandAddCatalogueToCohortIdentificationAsPatientIndexTable(activator,patientIndexTablesNode.Configuration));
        }

    }
}