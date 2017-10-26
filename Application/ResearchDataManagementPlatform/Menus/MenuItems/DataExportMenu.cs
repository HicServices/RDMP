using System;
using System.Windows.Forms;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus.MenuItems;
using DataExportManager.SimpleDialogs;

namespace ResearchDataManagementPlatform.Menus.MenuItems
{
    internal class DataExportMenu : RDMPToolStripMenuItem
    {
        public DataExportMenu(IActivateItems activator):base(activator,"Data Export Options")
        {

            Enabled = _activator.RepositoryLocator.DataExportRepository != null;

            DropDownItems.Add(new ToolStripMenuItem("Configure Disclaimer", null, ConfigureDisclaimer));
            DropDownItems.Add(new ToolStripMenuItem("Configure Hashing Algorithm", null, ConfigureHashingAlgorithm));
        }

        private void ConfigureHashingAlgorithm(object sender, EventArgs e)
        {
            var hash = new ConfigureHashingAlgorithm();
            hash.RepositoryLocator = _activator.RepositoryLocator;
            hash.ShowDialog();
        }

        private void ConfigureDisclaimer(object sender, EventArgs e)
        {
            var disclaimer = new ConfigureDisclaimer();
            disclaimer.RepositoryLocator = _activator.RepositoryLocator;
            disclaimer.ShowDialog();
        }
    }
}