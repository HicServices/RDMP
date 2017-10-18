using System;
using CatalogueLibrary.Nodes;
using CatalogueManager.CommandExecution;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands.UIFactory;
using CatalogueManager.Menus;
using CohortManager.CommandExecution.AtomicCommands;
using CohortManager.ItemActivation;

namespace CohortManager.Menus
{
    internal class JoinableCollectionNodeMenu : RDMPContextMenuStrip
    {
        public JoinableCollectionNodeMenu(IActivateCohortIdentificationItems activator, JoinableCollectionNode patientIndexTablesNode):base(activator,null)
        {
            Add(new ExecuteCommandAddCatalogueToCohortIdentificationAsPatientIndexTable(activator,patientIndexTablesNode.Configuration));
        }

    }
}