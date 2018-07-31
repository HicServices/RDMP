using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.ImportExport;
using CatalogueLibrary.Nodes.SharingNodes;
using CatalogueManager.CommandExecution.AtomicCommands;

namespace CatalogueManager.Menus
{
    public class AllObjectImportsNodeMenu:RDMPContextMenuStrip
    {
        public AllObjectImportsNodeMenu(RDMPContextMenuStripArgs args, AllObjectImportsNode node): base(args, node)
        {
            Add(new ExecuteCommandImportShareDefinitionList(_activator));
        }

        public AllObjectImportsNodeMenu(RDMPContextMenuStripArgs args, ObjectImport node) : base(args, node)
        {
            Add(new ExecuteCommandShowRelatedObject(_activator, node));
        }

        public AllObjectImportsNodeMenu(RDMPContextMenuStripArgs args, ObjectExport node) : base(args, node)
        {
            Add(new ExecuteCommandShowRelatedObject(_activator, node));
        }
    }
}
