using CatalogueLibrary.Nodes;
using CatalogueManager.Collections;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus;

namespace CohortManager.Menus
{
    internal class JoinableCollectionNodeMenu : RDMPContextMenuStrip
    {
        public JoinableCollectionNodeMenu(IActivateItems activator, JoinableCollectionNode patientIndexTablesNode, RDMPCollectionCommonFunctionality collection):base(activator,null, collection)
        {
            Add(new ExecuteCommandAddCatalogueToCohortIdentificationAsPatientIndexTable(activator,patientIndexTablesNode.Configuration));
        }

    }
}