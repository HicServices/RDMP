using System.Drawing;
using System.Windows.Forms;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Data.DataTables;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace DataExportManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandUnfreezeExtractionConfiguration:BasicUICommandExecution,IAtomicCommand
    {
        private readonly ExtractionConfiguration _configuration;

        public ExecuteCommandUnfreezeExtractionConfiguration(IActivateItems activator, ExtractionConfiguration configuration):base(activator)
        {
            _configuration = configuration;

            if(!_configuration.IsReleased)
                SetImpossible("Extraction Configuration is not Frozen");
        }

        public override string GetCommandHelp()
        {
            return "Reopens a released extraction configuration and deletes all record of it ever having been released";
        }

        public override void Execute()
        {
            base.Execute();

            if(MessageBox.Show("This will mean deleting the Release Audit for the Configuration making it appear like it was never released in the first place.  If you just want to execute the Configuration again you can Clone it instead if you want.  Are you sure you want to Unfreeze?","Confirm Unfreeze",MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
            {
                _configuration.Unfreeze();
                Publish(_configuration);
            }
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return CatalogueIcons.UnfreezeExtractionConfiguration;
        }
    }
}
