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
using CatalogueManager.CommandExecution.AtomicCommands.WindowArranging;
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

            Add(new ExecuteCommandCreateNewLoadMetadata(activator));

            Add(new ExecuteCommandEditExistingLoadMetadata(activator).SetTarget(loadMetadata));
            
            Items.Add("View Load Diagram", CatalogueIcons.LoadBubble, (s, e) => _activator.ActivateViewLoadMetadataDiagram(this, loadMetadata));
            
            AddCommonMenuItems();

        }

        public void Delete()
        {
            if (MessageBox.Show("Are you sure you want to Delete LoadMetadata '" + _loadMetadata + "'?", "Confirm Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                //delete it from the database
                _loadMetadata.DeleteInDatabase();
              
                Publish(_loadMetadata);
            }
        }
    }
}
