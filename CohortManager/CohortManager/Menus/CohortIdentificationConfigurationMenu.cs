using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort;
using CatalogueManager.Collections.Providers;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands.UIFactory;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus;
using CatalogueManager.Refreshing;
using CohortManager.Collections.Providers;
using CohortManager.CommandExecution.AtomicCommands;
using CohortManager.ItemActivation;
using CohortManagerLibrary.QueryBuilding;
using RDMPStartup;
using ReusableUIComponents.ChecksUI;

namespace CohortManager.Menus
{

    [System.ComponentModel.DesignerCategory("")]
    public class CohortIdentificationConfigurationMenu :RDMPContextMenuStrip
    {
        private CohortIdentificationConfiguration _cic;

        public CohortIdentificationConfigurationMenu(IActivateCohortIdentificationItems activator, CohortIdentificationConfiguration cic) : base( activator,cic)
        {
            _cic = cic;

            if(cic!=null)
            {
                var execute = new ToolStripMenuItem("Execute Configuration", CatalogueIcons.ExecuteArrow,(s, e) => activator.ExecuteCohortIdentificationConfiguration(this, cic));
                execute.Enabled = !cic.Frozen;
                Items.Add(execute);

                Items.Add("View SQL", _activator.CoreIconProvider.GetImage(RDMPConcept.SQL), (s, e) => _activator.ActivateViewCohortIdentificationConfigurationSql(this, cic));
            }
            
            Add(new ExecuteCommandCreateNewCohortIdentificationConfiguration(activator));


            if (cic != null)
            {
                Items.Add("Clone Configuration", CohortIdentificationIcons.cloneCohortIdentificationConfiguration,
                    (s, e) => CloneCohortIdentificationConfiguration());

                var freeze = new ToolStripMenuItem("Freeze Configuration",
                    CatalogueIcons.FrozenCohortIdentificationConfiguration, (s, e) => FreezeConfiguration());
                freeze.Enabled = !cic.Frozen;
                Items.Add(freeze);
            }

            if(cic != null)
                AddCommonMenuItems();
        }


        private void CloneCohortIdentificationConfiguration()
        {
            if (
                MessageBox.Show(
                    "This will create a 100% copy of the entire CohortIdentificationConfiguration including all datasets, filters, parameters and set operations, are you sure this is what you want?",
                    "Confirm Cloning", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {

                var checks = new PopupChecksUI("Cloning " + _cic, false);
                var clone = _cic.CreateClone(checks);

                //Load the clone up
                _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(clone));
            }
        }

        private void FreezeConfiguration()
        {
            _cic.Freeze();
            _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(_cic));
        }
    }

    
}
