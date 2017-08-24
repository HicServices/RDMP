using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using CatalogueManager.Collections.Providers;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands.UIFactory;
using CatalogueManager.DataLoadUIs.LoadMetadataUIs;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using MapsDirectlyToDatabaseTableUI;
using RDMPStartup;
using ReusableUIComponents;

namespace CatalogueManager.Menus
{
    [System.ComponentModel.DesignerCategory("")]
    public class LoadMetadataMenu:RDMPContextMenuStrip
    {
        private LoadMetadata _loadMetadata;
        
        public LoadMetadataMenu( IActivateItems activator, LoadMetadata loadMetadata):base(activator,loadMetadata)
        {
            _loadMetadata = loadMetadata;

            AtomicCommandUIFactory factory = new AtomicCommandUIFactory(activator.CoreIconProvider);
            Items.Add(factory.CreateMenuItem(new ExecuteCommandCreateNewLoadMetadata(activator)));

            var execute = new ToolStripMenuItem("Start Data Load", CatalogueIcons.ExecuteArrow,(s,e)=>activator.ExecuteLoadMetadata(this, loadMetadata));
            execute.Enabled = activator.AllowExecute;


            Items.Add("View Load Diagram", CatalogueIcons.LoadBubble, (s, e) => _activator.ActivateViewLoadMetadataDiagram(this, loadMetadata));


            Items.Add(execute);

            var edit = new ToolStripMenuItem("Edit", null, (s, e) => _activator.ActivateLoadMetadata(this,loadMetadata));
            
            Items.Add(edit);
            
            Items.Add("View Load Log", CatalogueIcons.Logging, (s, e) => _activator.ActivateViewLoadMetadataLog(this,loadMetadata));

            AddCommonMenuItems();

        }

        public void Delete()
        {
            if (MessageBox.Show("Are you sure you want to Delete LoadMetadata '" + _loadMetadata + "'?", "Confirm Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                //delete it from the database
                _loadMetadata.DeleteInDatabase();
              
                _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(_loadMetadata));
            }
        }
    }
}
