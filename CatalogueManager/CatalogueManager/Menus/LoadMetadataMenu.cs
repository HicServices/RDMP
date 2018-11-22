using System.Windows.Forms;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.DataLoadUIs.LoadMetadataUIs;
using CatalogueManager.Icons.IconProvision;
using ReusableLibraryCode.Icons.IconProvision;

namespace CatalogueManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    class LoadMetadataMenu:RDMPContextMenuStrip
    {
        public LoadMetadataMenu(RDMPContextMenuStripArgs args, LoadMetadata loadMetadata) : base(args, loadMetadata)
        {
            Items.Add("View Load Diagram", CatalogueIcons.LoadBubble, (s, e) => _activator.ActivateViewLoadMetadataDiagram(this, loadMetadata));

            Items.Add("Edit description", null,(s, e) => _activator.Activate<LoadMetadataUI, LoadMetadata>(loadMetadata));
            
            Items.Add(new ToolStripSeparator());

            Add(new ExecuteCommandOverrideRawServer(_activator, loadMetadata));
            Add(new ExecuteCommandCreateNewLoadMetadata(_activator));
            
            ReBrandActivateAs("Check and Execute",RDMPConcept.LoadMetadata,OverlayKind.Execute);
        }
    }
}
