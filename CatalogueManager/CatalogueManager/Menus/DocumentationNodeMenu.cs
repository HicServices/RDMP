using CatalogueLibrary.Nodes;
using CatalogueLibrary.Repositories;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus.MenuItems;
using RDMPStartup;

namespace CatalogueManager.Menus
{
    internal class DocumentationNodeMenu : RDMPContextMenuStrip
    {
        public DocumentationNode DocumentationNode { get; set; }

        public DocumentationNodeMenu(IActivateItems activator, DocumentationNode documentationNode):base(activator,null)
        {
            DocumentationNode = documentationNode;

            Items.Add(new AddSupportingDocumentMenuItem(activator, DocumentationNode.Catalogue));
            Items.Add(new AddSupportingSqlTableMenuItem(activator, DocumentationNode.Catalogue));
        }
    }
}