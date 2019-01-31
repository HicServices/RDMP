using System.Windows.Forms;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    class LoadMetadataMenu:RDMPContextMenuStrip
    {
        public LoadMetadataMenu(RDMPContextMenuStripArgs args, LoadMetadata loadMetadata) : base(args, loadMetadata)
        {
            Add(new ExecuteCommandViewLoadDiagram(_activator,loadMetadata));

            Add(new ExecuteCommandEditLoadMetadataDescription(_activator, loadMetadata));
            
            Items.Add(new ToolStripSeparator());

            Add(new ExecuteCommandOverrideRawServer(_activator, loadMetadata));
            Add(new ExecuteCommandCreateNewLoadMetadata(_activator));
            
            ReBrandActivateAs("Check and Execute",RDMPConcept.LoadMetadata,OverlayKind.Execute);
        }
    }
}
