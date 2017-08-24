using System;
using System.Windows.Forms;
using CatalogueLibrary.Repositories;
using DataExportManager.SimpleDialogs;
using RDMPStartup;

namespace ResearchDataManagementPlatform
{
    [System.ComponentModel.DesignerCategory("")]
    internal class DataExportMenu : ToolStripMenuItem
    {
        public IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get; set; }

        public DataExportMenu(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {
            RepositoryLocator = repositoryLocator;

            Enabled = RepositoryLocator.DataExportRepository != null;

            Text = "Data Export Options";

            DropDownItems.Add(new ToolStripMenuItem("Configure Disclaimer", null, ConfigureDisclaimer));
            DropDownItems.Add(new ToolStripMenuItem("Configure Hashing Algorithm", null, ConfigureHashingAlgorithm));
            

        }

        private void ConfigureHashingAlgorithm(object sender, EventArgs e)
        {
            var hash = new ConfigureHashingAlgorithm();
            hash.RepositoryLocator = RepositoryLocator;
            hash.ShowDialog();
        }

        private void ConfigureDisclaimer(object sender, EventArgs e)
        {
            var disclaimer = new ConfigureDisclaimer();
            disclaimer.RepositoryLocator = RepositoryLocator;
            disclaimer.ShowDialog();
        }
    }
}