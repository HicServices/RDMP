using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.Refreshing;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.DataRelease.Audit;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportManager.ItemActivation;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents;
using ReusableUIComponents.Icons.IconProvision;

namespace DataExportManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandUnfreezeExtractionConfiguration:BasicCommandExecution,IAtomicCommand
    {
        private readonly IActivateDataExportItems _activator;
        private readonly ExtractionConfiguration _configuration;

        public ExecuteCommandUnfreezeExtractionConfiguration(IActivateDataExportItems activator, ExtractionConfiguration configuration)
        {
            _activator = activator;
            _configuration = configuration;

            if(!_configuration.IsReleased)
                SetImpossible("Extraction Configuration is not Frozen");

            
        }

        public override void Execute()
        {
            base.Execute();

            if(MessageBox.Show("This will mean deleting the Release Audit for the Configuration making it appear like it was never released in the first place.  If you just want to execute the Configuration again you can Clone it instead if you want.  Are you sure you want to Unfreeze?","Confirm Unfreeze",MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
            {
                _configuration.Unfreeze();
                _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(_configuration));
            }
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return CatalogueIcons.UnfreezeExtractionConfiguration;
        }
    }
}
