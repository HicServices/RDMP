using CatalogueLibrary.Nodes;
using CatalogueLibrary.Repositories;
using CatalogueManager.Collections;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus.MenuItems;
using RDMPStartup;

namespace CatalogueManager.Menus
{
    internal class DocumentationNodeMenu : RDMPContextMenuStrip
    {
        public DocumentationNode DocumentationNode { get; set; }

        public DocumentationNodeMenu(RDMPContextMenuStripArgs args, DocumentationNode documentationNode): base(args, documentationNode)
        {
            DocumentationNode = documentationNode;

            Add(new ExecuteCommandAddNewSupportingDocument(_activator, DocumentationNode.Catalogue));
            Add(new ExecuteCommandAddNewSupportingSqlTable(_activator, DocumentationNode.Catalogue));
        }
    }
}